/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Data.Requests;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static HashSet<ElementId> ElementIds(this IRequest request, UIDocument uIDocument)
        {
            if (uIDocument == null || request == null)
                return null;

            Document document = uIDocument.Document;
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (request is ILogicalRequest)
            {
                HashSet<ElementId> logicalSet = null;
                foreach (IRequest logicalRequest in (request as ILogicalRequest).Requests)
                {
                    HashSet<ElementId> tempSet = ElementIds(logicalRequest, uIDocument);
                    if (tempSet == null)
                        continue;

                    if (logicalSet == null)
                        logicalSet = tempSet;
                    else
                    {
                        if (logicalRequest is LogicalAndRequest)
                            logicalSet.IntersectWith(tempSet);
                        else if (logicalRequest is LogicalOrRequest)
                            logicalSet.UnionWith(tempSet);
                    }
                }
                result = logicalSet;
            }
            else
            {

                IEnumerable<ElementId> elementIDs = null;
                if (request is SelectionRequest)
                    elementIDs = request.ElementIds(uIDocument);
                else
                    elementIDs = request.IElementIds(document);
                if (elementIDs != null)
                    result.UnionWith(elementIDs);
            }

            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> IElementIds(this SelectionRequest request, UIDocument uIDocument)
        {
            if (uIDocument.Selection == null)
                return null;

            return uIDocument.Selection.GetElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> IElementIds(this IRequest request, Document document)
        {
            return ElementIds(request as dynamic, document);
            
            
            //View
            if (queryType == RequestType.View)
            {
                List<View> allViews = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
                if (allViews != null)
                {
                    List<View> viewList = allViews.FindAll(x => !x.IsTemplate);
                    if (viewList.Count > 0)
                    {
                        RevitViewType? viewType = request.RevitViewType();
                        if (viewType != null && viewType.HasValue)
                        {
                            ViewType type = Query.ViewType(viewType.Value);
                            viewList.RemoveAll(x => x.ViewType != type);
                        }

                        string viewTemplateName = request.ViewTemplateName();
                        if (!string.IsNullOrWhiteSpace(viewTemplateName))
                        {
                            View view = allViews.Find(x => x.IsTemplate && x.Name == viewTemplateName);
                            if (view != null)
                                viewList.RemoveAll(x => x.ViewTemplateId != view.Id);
                        }
                    }
                    if (viewList != null && viewList.Count > 0)
                        viewList.ForEach(x => result.Add(x.Id));
                }
            }
            
            

            //Parameter
            if (queryType == RequestType.Parameter)
            {
                FilterRequest request = request.RelatedFilterRequest();
                if (request != null)
                {
                    string parameterName = BH.Engine.Adapters.Revit.Query.ParameterName(request);
                    if (!string.IsNullOrWhiteSpace(parameterName))
                    {
                        IComparisonRule comparisonRule = BH.Engine.Adapters.Revit.Query.ComparisonRule(request);
                        if (comparisonRule != null)
                        {
                            object value = BH.Engine.Adapters.Revit.Query.Value(request);

                            Dictionary<ElementId, List<FilterRequest>> dictionary = request.RequestDictionary(document);
                            if (dictionary != null && dictionary.Count > 0)
                            {
                                foreach (ElementId id in dictionary.Keys)
                                {
                                    Element element = document.GetElement(id);
                                    if (Query.IsValid(element, parameterName, comparisonRule, value))
                                        result.Add(id);
                                }

                            }
                        }
                    }
                }
            }

            return result;
        }

        public static IEnumerable<ElementId> ElementIds (this FilterRequest request, Document document)
        {
            return document.ElementIds(request.Type);
        }

        public static IEnumerable<ElementId> ElementIds(this DBTypeNameRequest request, Document document)
        {
            return ElementIds(document, "RevitAPI.dll", request.TypeName);
        }

        public static IEnumerable<ElementId> ElementIds(this CategoryRequest request, Document document)
        {
            return document.ElementIds(request.CategoryName);
        }

        public static IEnumerable<ElementId> ElementIds(this FamilyRequest request, Document document)
        {
            return document.ElementIds(request.FamilyName, request.FamilyTypeName, true);
        }

        public static IEnumerable<ElementId> ElementIds(this ActiveWorksetRequest request, Document document)
        {
            return document.WorksetElementIds(new List<WorksetId> { document.ActiveWorksetId() });
        }

        public static IEnumerable<ElementId> ElementIds(this OpenWorksetsRequest request, Document document)
        {
            return document.WorksetElementIds(document.OpenWorksetIds().ToList());
        }

        public static IEnumerable<ElementId> ElementIds(this WorksetRequest request, Document document)
        {
            return document.WorksetElementIds(new List<WorksetId> { document.WorksetId(request.WorksetName) });
        }

        public static IEnumerable<ElementId> ElementIds(this ElementIdsRequest request, Document document)
        {
            if (request.ElementIds != null)
                return request.ElementIds.Select(x => new ElementId(x));
            else
                return null;
        }

        public static IEnumerable<ElementId> ElementIds(this UniqueIdsRequest request, Document document)
        {
            if (request.UniqueIds != null)
            {
                List<ElementId> result = new List<ElementId>();
                foreach (string id in request.UniqueIds)
                {
                    Element element = document.GetElement(id);
                    if (element != null)
                        result.Add(element.Id);
                }
                return result;
            }
            else
                return null;
        }

        public static IEnumerable<ElementId> ElementIds(this SelectionSetRequest request, Document document)
        {
            return document.ElementIds(request.SelectionSetName, true);
        }

        public static IEnumerable<ElementId> ElementIds(this ViewByTemplateRequest request, Document document)
        {
            Element viewTemplate = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().Where(x => x.IsTemplate).Where(x => x.Name == request.TemplateName).FirstOrDefault();
            if (viewTemplate == null)
                return null;

            return new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().Where(x => !x.IsTemplate).Where(x => x.ViewTemplateId == viewTemplate.Id).Select(x => x.Id);
        }

        public static IEnumerable<ElementId> ElementIds(this ViewByTypeRequest request, Document document)
        {
            return new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().Where(x => !x.IsTemplate).Where(x => x.ViewType == request.RevitViewType.ViewType()).Select(x => x.Id);
        }

        public static IEnumerable<ElementId> ElementIds(this ViewTemplateRequest request, Document document)
        {
            return new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().Where(x => x.IsTemplate).Select(x => x.Id);
        }

        public static IEnumerable<ElementId> ElementIds(this ParameterExistsRequest request, Document document)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<ElementId> ElementIds(this ParameterNumberRequest request, Document document)
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<ElementId> ElementIds(this ParameterTextRequest request, Document document)
        {
            throw new NotImplementedException();
        }

        /***************************************************/

        public static List<ElementId> ElementIds(this Document document, IEnumerable<string> uniqueIds, bool removeNulls)
        {
            if (document == null || uniqueIds == null)
                return null;

            List<ElementId> elementIDs = new List<ElementId>();
            foreach (string uniqueID in uniqueIds)
            {
                if (!string.IsNullOrEmpty(uniqueID))
                {
                    Element element = document.GetElement(uniqueID);
                    if (element != null)
                    {
                        elementIDs.Add(element.Id);
                        continue;
                    }
                }

                if (!removeNulls)
                    elementIDs.Add(null);
            }

            return elementIDs;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, Type type)
        {
            if (document == null || type == null)
                return null;

            IEnumerable<Type> types = null;
            IEnumerable<BuiltInCategory> builtInCategories = null;
            if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(Element)))
                types = new List<Type>() { type };
            else if (typeof(IBHoMObject).IsAssignableFrom(type))//BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(IBHoMObject)))
            {
                types = RevitTypes(type);
                builtInCategories = BuiltInCategories(type);
            }

            IEnumerable<ElementId> elementIDs = null;
            if (types != null)
                types = types.ToList().FindAll(x => BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(x, typeof(Element)));

            if (types == null || types.Count() == 0)
            {
                if ((builtInCategories != null && builtInCategories.Count() > 0))
                    elementIDs = new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();
                else
                    return null;
            }

            //Some of the Revit API types do not exist in the object model of Revit - they cannot be filtered...
            List<Type> supportedAPITypes = new List<Type>();
            List<Tuple<Type, Type>> unsupportedAPITypes = new List<Tuple<Type, Type>>();
            foreach (Type t in types)
            {
                Type supportedAPIType = t.SupportedAPIType();
                if (supportedAPIType != t)
                    unsupportedAPITypes.Add(new Tuple<Type, Type>(t, supportedAPIType));
                else
                    supportedAPITypes.Add(t);
            }

            if (supportedAPITypes.Count != 0)
            {
                if ((builtInCategories == null || builtInCategories.Count() == 0))
                    elementIDs = new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(supportedAPITypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter))).ToElementIds();
                else
                    elementIDs = new FilteredElementCollector(document).WherePasses(new LogicalAndFilter(new LogicalOrFilter(supportedAPITypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter)), new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)))).ToElementIds();
            }

            if (unsupportedAPITypes.Count != 0)
            {
                HashSet<ElementId> extraElementIds = new HashSet<ElementId>();
                foreach (Tuple<Type, Type> unsupportedAPIType in unsupportedAPITypes)
                {
                    IEnumerable<ElementId> elementIds;
                    if ((builtInCategories == null || builtInCategories.Count() == 0))
                        elementIds = new FilteredElementCollector(document).WherePasses(new ElementClassFilter(unsupportedAPIType.Item2)).ToElementIds();
                    else
                        elementIds = new FilteredElementCollector(document).WherePasses(new LogicalAndFilter(new ElementClassFilter(unsupportedAPIType.Item2), new LogicalOrFilter(builtInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)))).ToElementIds();

                    foreach (ElementId e in elementIds)
                    {
                        if (unsupportedAPIType.Item1.IsAssignableFrom(document.GetElement(e).GetType()))
                            extraElementIds.Add(e);
                    }
                }

                foreach (ElementId e in elementIDs)
                {
                    extraElementIds.Add(e);
                }

                elementIDs = extraElementIds;
            }

            //Special Cases
            if (elementIDs != null && elementIDs.Count() > 0)
            {
                if (type == typeof(BH.oM.Adapters.Revit.Elements.ModelInstance))
                {
                    //OST_DetailComponents BuiltInCategory is wrongly set to CategoryType.Model.
                    int detailComponentsCategoryId = Category.GetCategory(document, Autodesk.Revit.DB.BuiltInCategory.OST_DetailComponents).Id.IntegerValue;

                    List<ElementId> newElementIds = new List<Autodesk.Revit.DB.ElementId>();
                    foreach (ElementId e in elementIDs)
                    {
                        Category category = document.GetElement(e).Category;
                        if ((category.CategoryType == CategoryType.AnalyticalModel || category.CategoryType == CategoryType.Model) && category.Id.IntegerValue != detailComponentsCategoryId)
                            newElementIds.Add(e);
                    }

                    elementIDs = newElementIds;
                }

                if (type == typeof(oM.Physical.Elements.Window))
                {
                    //Revit returns additional "parent" Autodesk.Revit.DB.Panel with no geometry when pulling all panels from model. This part of the code filter them out
                    List<ElementId> elementIDList = new List<ElementId>();
                    foreach (ElementId elementID in elementIDs)
                    {
                        Panel panel = document.GetElement(elementID) as Panel;
                        if (panel != null)
                        {
                            ElementId hostID = panel.FindHostPanel();
                            if (hostID != null && hostID != Autodesk.Revit.DB.ElementId.InvalidElementId)
                                continue;
                        }

                        elementIDList.Add(elementID);
                    }
                    elementIDs = elementIDList;
                }
            }

            return elementIDs;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string familyName, string familyTypeName, bool caseSensitive)
        {
            if (document == null)
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
            if (!string.IsNullOrEmpty(familyName))
            {
                if (caseSensitive)
                    elementTypes = elementTypes.FindAll(x => x.FamilyName == familyName);
                else
                    elementTypes = elementTypes.FindAll(x => !string.IsNullOrEmpty(x.FamilyName) && x.FamilyName.ToUpper() == familyName.ToUpper());
            }

            if (elementTypes == null)
                return null;

            if (!string.IsNullOrEmpty(familyTypeName))
            {
                if (caseSensitive)
                    elementTypes = elementTypes.FindAll(x => x.Name == familyTypeName);
                else
                    elementTypes = elementTypes.FindAll(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper() == familyTypeName.ToUpper());
            }

            if (elementTypes == null)
                return null;

            List<ElementId> result = new List<ElementId>();
            foreach (ElementType elementType in elementTypes)
            {
                if (elementType is FamilySymbol)
                {
                    foreach (ElementId elementId in new FilteredElementCollector(document).WherePasses(new FamilyInstanceFilter(document, elementType.Id)).ToElementIds())
                        result.Add(elementId);
                }
                else
                {
                    Type type = null;

                    if (elementType is WallType)
                        type = typeof(Wall);
                    else if (elementType is FloorType)
                        type = typeof(Floor);
                    else if (elementType is CeilingType)
                        type = typeof(Ceiling);
                    else if (elementType is CurtainSystemType)
                        type = typeof(CurtainSystem);
                    else if (elementType is PanelType)
                        type = typeof(Panel);
                    else if (elementType is MullionType)
                        type = typeof(Mullion);
                    else if (elementType is Autodesk.Revit.DB.Mechanical.DuctType)
                        type = typeof(Autodesk.Revit.DB.Mechanical.Duct);
                    else if (elementType is Autodesk.Revit.DB.Mechanical.FlexDuctType)
                        type = typeof(Autodesk.Revit.DB.Mechanical.FlexDuct);
                    else if (elementType is Autodesk.Revit.DB.Mechanical.DuctInsulationType)
                        type = typeof(Autodesk.Revit.DB.Mechanical.DuctInsulation);
                    else if (elementType is Autodesk.Revit.DB.Plumbing.PipeType)
                        type = typeof(Autodesk.Revit.DB.Plumbing.Pipe);
                    else if (elementType is Autodesk.Revit.DB.Plumbing.FlexPipeType)
                        type = typeof(Autodesk.Revit.DB.Plumbing.FlexPipe);
                    else if (elementType is Autodesk.Revit.DB.Plumbing.PipeInsulationType)
                        type = typeof(Autodesk.Revit.DB.Plumbing.PipeInsulation);
                    else if (elementType is Autodesk.Revit.DB.Electrical.ConduitType)
                        type = typeof(Autodesk.Revit.DB.Electrical.Conduit);
                    else if (elementType is Autodesk.Revit.DB.Electrical.CableTrayType)
                        type = typeof(Autodesk.Revit.DB.Electrical.CableTray);

                    if (type == null)
                        continue;

                    List<Element> elements = new FilteredElementCollector(document).OfClass(type).ToList();
                    if (elements == null || elements.Count == 0)
                        continue;

                    foreach (Element element in elements)
                        if (element != null && element.GetTypeId() == elementType.Id)
                            result.Add(element.Id);
                }
            }
            return result;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string categoryName)
        {
            if (document == null || string.IsNullOrEmpty(categoryName))
                return null;

            BuiltInCategory builtInCategory = Query.BuiltInCategory(document, categoryName);
            if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                return null;

            return new FilteredElementCollector(document).OfCategory(builtInCategory).ToElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string selectionFilterElementName, bool caseSensitive)
        {
            if (document == null || string.IsNullOrEmpty(selectionFilterElementName))
                return null;

            List<SelectionFilterElement> selectionFilterElements = new FilteredElementCollector(document).OfClass(typeof(SelectionFilterElement)).Cast<SelectionFilterElement>().ToList();

            SelectionFilterElement selectionFilterElement = null;
            if (caseSensitive)
                selectionFilterElement = selectionFilterElements.Find(x => x.Name == selectionFilterElementName);
            else
                selectionFilterElement = selectionFilterElements.Find(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper() == selectionFilterElementName.ToUpper());

            if (selectionFilterElement == null)
                return null;

            return selectionFilterElement.GetElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string currentDomainAssembly, string typeName)
        {
            if (document == null || string.IsNullOrEmpty(currentDomainAssembly) || string.IsNullOrEmpty(typeName))
                return null;

            Assembly assembly = BH.Engine.Adapters.Revit.Query.CurrentDomainAssembly(currentDomainAssembly);
            if (assembly == null)
                return null;

            string fullTypeName = null; ;

            if (currentDomainAssembly == "RevitAPI.dll")
            {
                if (!typeName.StartsWith("Autodesk.Revit.DB."))
                    fullTypeName = "Autodesk.Revit.DB." + typeName;
                else
                    fullTypeName = typeName;
            }

            if (string.IsNullOrEmpty(fullTypeName))
                return null;

            Type type = null;

            try
            {
                type = assembly.GetType(fullTypeName, false, false);
            }
            catch
            {

            }

            if (type == null)
            {
                foreach (TypeInfo typeInfo in assembly.DefinedTypes)
                {
                    if (typeInfo.Name == typeName)
                    {
                        type = typeInfo.AsType();
                        break;
                    }
                }
            }

            return new FilteredElementCollector(document).OfClass(type).ToElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> WorksetElementIds(this Document document, List<WorksetId> worksetIds)
        {
            if (document == null || worksetIds == null)
                return null;

            if (worksetIds.Count == 0)
                return new List<ElementId>();
            else if (worksetIds.Count == 1)
                return new FilteredElementCollector(document).WherePasses(new ElementWorksetFilter(worksetIds.First(), false)).ToElementIds();
            else
                return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(worksetIds.ConvertAll(x => new ElementWorksetFilter(x, false) as ElementFilter))).ToElementIds();
        }
    }
}
