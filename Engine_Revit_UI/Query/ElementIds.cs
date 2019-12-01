/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

                foreach(ElementId e in elementIDs)
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
                        if(panel != null)
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
            if (document == null )
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

            if(!string.IsNullOrEmpty(familyTypeName))
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
                if(elementType is FamilySymbol)
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

                    foreach(Element element in elements)
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

        public static IEnumerable<ElementId> ElementIds(this Document document, bool activeWorkset, bool openWorksets, string worksetName = null)
        {
            if (document == null)
                return null;

            List<WorksetId> worksetIDs = new List<WorksetId>();
            if (!string.IsNullOrEmpty(worksetName))
            {
                WorksetId worksetID = Query.WorksetId(document, worksetName);
                if (worksetID != null && worksetIDs.Find(x => x == worksetID) == null)
                    worksetIDs.Add(worksetID);
            }

            if (activeWorkset)
            {
                WorksetId worksetID = Query.ActiveWorksetId(document);
                if (worksetID != null && worksetIDs.Find(x => x == worksetID) == null)
                    worksetIDs.Add(worksetID);
            }

            if (openWorksets)
            {
                IEnumerable<WorksetId> worksetIDList = Query.OpenWorksetIds(document);
                if (worksetIDList != null && worksetIDList.Count() > 0)
                {
                    foreach (WorksetId worksetID in worksetIDList)
                    {
                        if (worksetID != null && worksetIDs.Find(x => x == worksetID) == null)
                            worksetIDs.Add(worksetID);
                    }
                }
            }

            if (worksetIDs == null || worksetIDs.Count == 0)
                return null;

            if (worksetIDs.Count == 1)
                return new FilteredElementCollector(document).WherePasses(new ElementWorksetFilter(worksetIDs.First(), false)).ToElementIds();
            else
                return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(worksetIDs.ConvertAll(x => new ElementWorksetFilter(x, false) as ElementFilter))).ToElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterRequest filterRequest, UIDocument uIDocument)
        {
            if (uIDocument == null || filterRequest == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            Document document = uIDocument.Document;

            IEnumerable<ElementId> elementIDs = null;

            RequestType queryType = BH.Engine.Adapters.Revit.Query.RequestType(filterRequest);

            //Type
            if (queryType == RequestType.Undefined && filterRequest.Type != null)
            {
                elementIDs = ElementIds(uIDocument.Document, filterRequest.Type);
                if (elementIDs != null)
                {
                    foreach (ElementId elementId in elementIDs)
                        result.Add(elementId);
                }
            }

            //Workset
            string worksetName = BH.Engine.Adapters.Revit.Query.WorksetName(filterRequest);
            bool activeWorkset = queryType == RequestType.ActiveWorkset;
            bool openWorksets = queryType == RequestType.OpenWorksets;
            elementIDs = ElementIds(document, activeWorkset, openWorksets, worksetName);
            if (elementIDs != null)
            {
                foreach (ElementId elementId in elementIDs)
                    result.Add(elementId);
            }

            //Category
            string categoryName = BH.Engine.Adapters.Revit.Query.CategoryName(filterRequest);
            if (!string.IsNullOrEmpty(categoryName))
            {
                elementIDs = ElementIds(document, categoryName);
                if (elementIDs != null)
                {
                    foreach (ElementId elementId in elementIDs)
                            result.Add(elementId);
                }
            }

            //IncludeSelected
            if (BH.Engine.Adapters.Revit.Query.IncludeSelected(filterRequest) && uIDocument.Selection != null)
            {
                ICollection<ElementId> elementIDCollection = uIDocument.Selection.GetElementIds();
                if (elementIDCollection != null)
                {
                    foreach (ElementId elementId in elementIDCollection)
                        result.Add(elementId);
                }
            }

            //ElementIds
            IEnumerable<int> elementIDList = BH.Engine.Adapters.Revit.Query.ElementIds(filterRequest);
            if (elementIDList != null)
            {
                foreach (int id in elementIDList)
                {
                    ElementId elementId = new ElementId(id);
                    Element element = document.GetElement(elementId);
                    if (element != null)
                        result.Add(elementId);
                }
            }

            //UniqueIds
            IEnumerable<string> uniqueIDs = BH.Engine.Adapters.Revit.Query.UniqueIds(filterRequest);
            if (uniqueIDs != null)
            {
                foreach (string id in uniqueIDs)
                {
                    Element element = document.GetElement(id);
                    if (element != null)
                        result.Add(element.Id);
                }
            }

            //ViewTemplate
            if (queryType == RequestType.ViewTemplate)
            {
                List<View> viewList = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
                if (viewList != null)
                {
                    viewList.RemoveAll(x => !x.IsTemplate);
                    if (viewList.Count > 0)
                    {
                        string viewTemplateName = BH.Engine.Adapters.Revit.Query.ViewTemplateName(filterRequest);

                        if (!string.IsNullOrEmpty(viewTemplateName))
                        {
                            View view = viewList.Find(x => x.Name == viewTemplateName);
                            viewList = new List<View>();
                            if (view != null)
                                viewList.Add(view);
                        }

                        foreach (View view in viewList)
                                result.Add(view.Id);
                    }
                }
            }

            //View
            if (queryType == RequestType.View)
            {
                List<View> allViews = new FilteredElementCollector(document).OfClass(typeof(View)).Cast<View>().ToList();
                if (allViews != null)
                {
                    List<View> viewList = allViews.FindAll(x => !x.IsTemplate);
                    if (viewList.Count > 0)
                    {
                        RevitViewType? viewType = filterRequest.RevitViewType();
                        if(viewType != null && viewType.HasValue)
                        {
                            ViewType type = Query.ViewType(viewType.Value);
                            viewList.RemoveAll(x => x.ViewType != type);
                        }

                        string viewTemplateName = filterRequest.ViewTemplateName();
                        if(!string.IsNullOrWhiteSpace(viewTemplateName))
                        {
                            View view = allViews.Find(x => x.IsTemplate && x.Name == viewTemplateName);
                            if(view != null)
                                viewList.RemoveAll(x => x.ViewTemplateId != view.Id);
                        }
                    }
                    if (viewList != null && viewList.Count > 0)
                        viewList.ForEach(x => result.Add(x.Id));
                }
            }

            //TypeName
            string typeName = BH.Engine.Adapters.Revit.Query.TypeName(filterRequest);
            if (!string.IsNullOrEmpty(typeName))
            {
                elementIDs = ElementIds(document, "RevitAPI.dll", typeName);
                if (elementIDs != null)
                {
                    foreach (ElementId id in elementIDs)
                        result.Add(id);
                }
            }

            //FamilyName and FamilySymbolName
            if (queryType == RequestType.Family)
            {
                elementIDs = ElementIds(document, BH.Engine.Adapters.Revit.Query.FamilyName(filterRequest), BH.Engine.Adapters.Revit.Query.FamilyTypeName(filterRequest), true);
                if (elementIDs != null && elementIDs.Count() > 0)
                {
                    foreach (ElementId id in elementIDs)
                        result.Add(id);
                }
            }

            //SelectionSet
            if (queryType == RequestType.SelectionSet)
            {
                elementIDs = ElementIds(document, BH.Engine.Adapters.Revit.Query.SelectionSetName(filterRequest), true);
                if (elementIDs != null && elementIDs.Count() > 0)
                {
                    foreach (ElementId id in elementIDs)
                        result.Add(id);
                }
            }

            //Parameter
            if (queryType == RequestType.Parameter)
            {
                FilterRequest request = filterRequest.RelatedFilterRequest();
                if (request != null)
                {
                    string parameterName = BH.Engine.Adapters.Revit.Query.ParameterName(filterRequest);
                    if (!string.IsNullOrWhiteSpace(parameterName))
                    {
                        IComparisonRule comparisonRule = BH.Engine.Adapters.Revit.Query.ComparisonRule(filterRequest);
                        if (comparisonRule != null)
                        {
                            object value = BH.Engine.Adapters.Revit.Query.Value(filterRequest);

                            Dictionary<ElementId, List<FilterRequest>> dictionary = request.FilterRequestDictionary(uIDocument);
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

        /***************************************************/
    }
}