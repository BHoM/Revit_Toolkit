/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<ElementId> ElementIds(this Document document, IEnumerable<string> uniqueIds, bool removeNulls)
        {
            if (document == null || uniqueIds == null)
                return null;


            List<ElementId> aElementIdList = new List<ElementId>();
            foreach (string aUniqueId in uniqueIds)
            {
                if (!string.IsNullOrEmpty(aUniqueId))
                {
                    Element aElement = document.GetElement(aUniqueId);
                    if (aElement != null)
                    {
                        aElementIdList.Add(aElement.Id);
                        continue;
                    }
                }

                if (!removeNulls)
                    aElementIdList.Add(null);
            }

            return aElementIdList;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, Type type)
        {
            if (document == null || type == null)
                return null;

            IEnumerable<Type> aTypes = null;
            IEnumerable<BuiltInCategory> aBuiltInCategories = null;
            if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(Element)))
                aTypes = new List<Type>() { type };
            else if (typeof(IBHoMObject).IsAssignableFrom(type))//BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(IBHoMObject)))
            {
                aTypes = RevitTypes(type);
                aBuiltInCategories = BuiltInCategories(type);
            }

            if (aTypes == null || aTypes.Count() == 0)
                return null;

            aTypes = aTypes.ToList().FindAll(x => BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(x, typeof(Element)));
            if (aTypes.Count() == 0)
                return null;

            IEnumerable<ElementId> aElementIds = null;
            if (aTypes.Count() == 1)
            {
                if (aBuiltInCategories == null || aBuiltInCategories.Count() == 0)
                    aElementIds = new FilteredElementCollector(document).OfClass(aTypes.First()).ToElementIds();
                else

                aElementIds = new FilteredElementCollector(document).OfClass(aTypes.First()).WherePasses(new LogicalOrFilter(aBuiltInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();
            }
            else
            {
                if ((aBuiltInCategories == null || aBuiltInCategories.Count() == 0) && (aTypes != null && aTypes.Count() > 0))
                    aElementIds = new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(aTypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter))).ToElementIds();
                else if ((aTypes == null || aTypes.Count() == 0) && (aBuiltInCategories != null && aBuiltInCategories.Count() > 0))
                    aElementIds = new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(aBuiltInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();
                else
                    aElementIds = new FilteredElementCollector(document).WherePasses(new LogicalAndFilter(new LogicalOrFilter(aTypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter)), new LogicalOrFilter(aBuiltInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)))).ToElementIds();
            }

            //Special Cases
            if (aElementIds != null && aElementIds.Count() > 0)
            {
                //oM.Physical.Elements.Window
                if (type == typeof(oM.Physical.Elements.Window))
                {
                    //Revit returns additional "parent" Autodesk.Revit.DB.Panel with no geometry when pulling all panels from model. This part of the code filter them out
                    List<ElementId> aElementIdList = new List<ElementId>();
                    foreach (ElementId aElementId in aElementIds)
                    {
                        Panel aPanel = document.GetElement(aElementId) as Panel;
                        if(aPanel != null)
                        {
                            ElementId aElementId_Host = aPanel.FindHostPanel();
                            if (aElementId_Host != null && aElementId_Host != Autodesk.Revit.DB.ElementId.InvalidElementId)
                                continue;
                        }

                        aElementIdList.Add(aElementId);
                    }
                    aElementIds = aElementIdList;
                }
            }



            return aElementIds;
                
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string familyName, string familyTypeName, bool caseSensitive)
        {
            if (document == null )
                return null;

            List<ElementType> aElementTypeList = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();
            if (!string.IsNullOrEmpty(familyName))
            {
                if (caseSensitive)
                    aElementTypeList = aElementTypeList.FindAll(x => x.FamilyName == familyName);
                else
                    aElementTypeList = aElementTypeList.FindAll(x => !string.IsNullOrEmpty(x.FamilyName) && x.FamilyName.ToUpper() == familyName.ToUpper());
            }

            if (aElementTypeList == null)
                return null;

            if(!string.IsNullOrEmpty(familyTypeName))
            {
                if (caseSensitive)
                    aElementTypeList = aElementTypeList.FindAll(x => x.Name == familyTypeName);
                else
                    aElementTypeList = aElementTypeList.FindAll(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper() == familyTypeName.ToUpper());
            }

            if (aElementTypeList == null)
                return null;


            List<ElementId> aResult = new List<ElementId>();
            foreach (ElementType aElementType in aElementTypeList)
            {
                if(aElementType is FamilySymbol)
                {
                    foreach (ElementId aElementId in new FilteredElementCollector(document).WherePasses(new FamilyInstanceFilter(document, aElementType.Id)).ToElementIds())
                        aResult.Add(aElementId);
                }
                else
                {
                    Type aType = null;

                    if (aElementType is WallType)
                        aType = typeof(Wall);
                    else if (aElementType is FloorType)
                        aType = typeof(Floor);
                    else if (aElementType is CeilingType)
                        aType = typeof(Ceiling);
                    else if (aElementType is CurtainSystemType)
                        aType = typeof(CurtainSystem);
                    else if (aElementType is PanelType)
                        aType = typeof(Panel);
                    else if (aElementType is MullionType)
                        aType = typeof(Mullion);
                    else if (aElementType is Autodesk.Revit.DB.Mechanical.DuctType)
                        aType = typeof(Autodesk.Revit.DB.Mechanical.Duct);
                    else if (aElementType is Autodesk.Revit.DB.Mechanical.FlexDuctType)
                        aType = typeof(Autodesk.Revit.DB.Mechanical.FlexDuct);
                    else if (aElementType is Autodesk.Revit.DB.Mechanical.DuctInsulationType)
                        aType = typeof(Autodesk.Revit.DB.Mechanical.DuctInsulation);
                    else if (aElementType is Autodesk.Revit.DB.Plumbing.PipeType)
                        aType = typeof(Autodesk.Revit.DB.Plumbing.Pipe);
                    else if (aElementType is Autodesk.Revit.DB.Plumbing.FlexPipeType)
                        aType = typeof(Autodesk.Revit.DB.Plumbing.FlexPipe);
                    else if (aElementType is Autodesk.Revit.DB.Plumbing.PipeInsulationType)
                        aType = typeof(Autodesk.Revit.DB.Plumbing.PipeInsulation);
                    else if (aElementType is Autodesk.Revit.DB.Electrical.ConduitType)
                        aType = typeof(Autodesk.Revit.DB.Electrical.Conduit);
                    else if (aElementType is Autodesk.Revit.DB.Electrical.CableTrayType)
                        aType = typeof(Autodesk.Revit.DB.Electrical.CableTray);

                    if (aType == null)
                        continue;

                    List<Element> aElementList = new FilteredElementCollector(document).OfClass(aType).ToList();
                    if (aElementList == null || aElementList.Count == 0)
                        continue;

                    foreach(Element aElement in aElementList)
                        if (aElement != null && aElement.GetTypeId() == aElementType.Id)
                            aResult.Add(aElement.Id);
                }
            }
            return aResult;
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string categoryName)
        {
            if (document == null || string.IsNullOrEmpty(categoryName))
                return null;

            BuiltInCategory aBuiltInCategory = Query.BuiltInCategory(document, categoryName);
            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                return null;

            return new FilteredElementCollector(document).OfCategory(aBuiltInCategory).ToElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string selectionFilterElementName, bool caseSensitive)
        {
            if (document == null || string.IsNullOrEmpty(selectionFilterElementName))
                return null;

            List<SelectionFilterElement> aSelectionFilterElementList = new FilteredElementCollector(document).OfClass(typeof(SelectionFilterElement)).Cast<SelectionFilterElement>().ToList();

            SelectionFilterElement aSelectionFilterElement = null;
            if (caseSensitive)
                aSelectionFilterElement = aSelectionFilterElementList.Find(x => x.Name == selectionFilterElementName);
            else
                aSelectionFilterElement = aSelectionFilterElementList.Find(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper() == selectionFilterElementName.ToUpper());

            if (aSelectionFilterElement == null)
                return null;

            return aSelectionFilterElement.GetElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, string currentDomainAssembly, string typeName)
        {
            if (document == null || string.IsNullOrEmpty(currentDomainAssembly) || string.IsNullOrEmpty(typeName))
                return null;

            Assembly aAssembly = BH.Engine.Adapters.Revit.Query.CurrentDomainAssembly(currentDomainAssembly);
            if (aAssembly == null)
                return null;

            string aFullTypeName = null; ;

            if (currentDomainAssembly == "RevitAPI.dll")
            {
                if (!typeName.StartsWith("Autodesk.Revit.DB."))
                    aFullTypeName = "Autodesk.Revit.DB." + typeName;
                else
                    aFullTypeName = typeName;
            }

            if (string.IsNullOrEmpty(aFullTypeName))
                return null;

            Type aType = null;

            try
            {
                aType = aAssembly.GetType(aFullTypeName, false, false);
            }
            catch
            {

            }

            if (aType == null)
            {
                foreach(TypeInfo aTypeInfo in aAssembly.DefinedTypes)
                    if(aTypeInfo.Name == typeName)
                    {
                        aType = aTypeInfo.AsType();
                        break;
                    }
            }


            return new FilteredElementCollector(document).OfClass(aType).ToElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this Document document, bool activeWorkset, bool openWorksets, string worksetName = null)
        {
            if (document == null)
                return null;

            List<WorksetId> aWorksetIdList = new List<WorksetId>();
            if (!string.IsNullOrEmpty(worksetName))
            {
                WorksetId aWorksetId = Query.WorksetId(document, worksetName);
                if (aWorksetId != null && aWorksetIdList.Find(x => x == aWorksetId) == null)
                    aWorksetIdList.Add(aWorksetId);
            }

            if (activeWorkset)
            {
                WorksetId aWorksetId = Query.ActiveWorksetId(document);
                if (aWorksetId != null && aWorksetIdList.Find(x => x == aWorksetId) == null)
                    aWorksetIdList.Add(aWorksetId);
            }

            if (openWorksets)
            {
                IEnumerable<WorksetId> aWorksetIds = Query.OpenWorksetIds(document);
                if (aWorksetIds != null && aWorksetIds.Count() > 0)
                {
                    foreach (WorksetId aWorksetId in aWorksetIds)
                        if (aWorksetId != null && aWorksetIdList.Find(x => x == aWorksetId) == null)
                            aWorksetIdList.Add(aWorksetId);
                }
            }

            if (aWorksetIdList == null || aWorksetIdList.Count == 0)
                return null;

            if (aWorksetIdList.Count == 1)
                return new FilteredElementCollector(document).WherePasses(new ElementWorksetFilter(aWorksetIdList.First(), false)).ToElementIds();
            else
                return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(aWorksetIdList.ConvertAll(x => new ElementWorksetFilter(x, false) as ElementFilter))).ToElementIds();
        }

        /***************************************************/

        public static IEnumerable<ElementId> ElementIds(this FilterRequest filterRequest, UIDocument uIDocument)
        {
            if (uIDocument == null || filterRequest == null)
                return null;

            HashSet<ElementId> aResult = new HashSet<ElementId>();
            Document aDocument = uIDocument.Document;

            IEnumerable<ElementId> aElementIds = null;

            RequestType aQueryType = BH.Engine.Adapters.Revit.Query.RequestType(filterRequest);

            //Type
            if (aQueryType == RequestType.Undefined && filterRequest.Type != null)
            {
                aElementIds = ElementIds(uIDocument.Document, filterRequest.Type);
                if (aElementIds != null)
                {
                    foreach (ElementId aElementId in aElementIds)
                        aResult.Add(aElementId);
                }
            }

            //Workset
            string aWorksetName = BH.Engine.Adapters.Revit.Query.WorksetName(filterRequest);
            bool aActiveWorkset = aQueryType == RequestType.ActiveWorkset;
            bool aOpenWorksets = aQueryType == RequestType.OpenWorksets;
            aElementIds = ElementIds(aDocument, aActiveWorkset, aOpenWorksets, aWorksetName);
            if (aElementIds != null)
            {
                foreach (ElementId aElementId in aElementIds)
                    aResult.Add(aElementId);
            }

            //Category
            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(filterRequest);
            if (!string.IsNullOrEmpty(aCategoryName))
            {
                aElementIds = ElementIds(aDocument, aCategoryName);
                if (aElementIds != null)
                {
                    foreach (ElementId aElementId in aElementIds)
                            aResult.Add(aElementId);
                }
            }

            //IncludeSelected
            if (BH.Engine.Adapters.Revit.Query.IncludeSelected(filterRequest) && uIDocument.Selection != null)
            {
                ICollection<ElementId> aElementIdCollection = uIDocument.Selection.GetElementIds();
                if (aElementIdCollection != null)
                    foreach (ElementId aElementId in aElementIdCollection)
                            aResult.Add(aElementId);
            }

            //ElementIds
            IEnumerable<int> aElementIds_Int = BH.Engine.Adapters.Revit.Query.ElementIds(filterRequest);
            if (aElementIds_Int != null)
            {
                foreach (int aId in aElementIds_Int)
                {
                    ElementId aElementId = new ElementId(aId);
                    Element aElement = aDocument.GetElement(aElementId);
                    if (aElement != null)
                        aResult.Add(aElementId);
                }
            }

            //UniqueIds
            IEnumerable<string> aUniqueIds = BH.Engine.Adapters.Revit.Query.UniqueIds(filterRequest);
            if (aUniqueIds != null)
            {
                foreach (string aUniqueId in aUniqueIds)
                {
                    Element aElement = aDocument.GetElement(aUniqueId);
                    if (aElement != null)
                        aResult.Add(aElement.Id);
                }
            }

            //ViewTemplate
            if (aQueryType == RequestType.ViewTemplate)
            {
                List<View> aViewList = new FilteredElementCollector(aDocument).OfClass(typeof(View)).Cast<View>().ToList();
                if (aViewList != null)
                {
                    aViewList.RemoveAll(x => !x.IsTemplate);
                    if (aViewList.Count > 0)
                    {
                        string aViewTemplateName = BH.Engine.Adapters.Revit.Query.ViewTemplateName(filterRequest);

                        if (!string.IsNullOrEmpty(aViewTemplateName))
                        {
                            View aView = aViewList.Find(x => x.Name == aViewTemplateName);
                            aViewList = new List<View>();
                            if (aView != null)
                                aViewList.Add(aView);
                        }

                        foreach (View aView in aViewList)
                                aResult.Add(aView.Id);
                    }
                }
            }

            //View
            if (aQueryType == RequestType.View)
            {
                List<View> aViewList_All = new FilteredElementCollector(aDocument).OfClass(typeof(View)).Cast<View>().ToList();
                if (aViewList_All != null)
                {
                    List<View> aViewList = aViewList_All.FindAll(x => !x.IsTemplate);
                    if (aViewList.Count > 0)
                    {
                        RevitViewType? aRevitViewType = filterRequest.RevitViewType();
                        if(aRevitViewType != null && aRevitViewType.HasValue)
                        {
                            ViewType aViewType = Query.ViewType(aRevitViewType.Value);
                            aViewList.RemoveAll(x => x.ViewType != aViewType);
                        }

                        string aViewTemplateName = filterRequest.ViewTemplateName();
                        if(!string.IsNullOrWhiteSpace(aViewTemplateName))
                        {
                            View aView = aViewList_All.Find(x => x.IsTemplate && x.Name == aViewTemplateName);
                            if(aView != null)
                                aViewList.RemoveAll(x => x.ViewTemplateId != aView.Id);
                        }
                    }
                    if (aViewList != null && aViewList.Count > 0)
                        aViewList.ForEach(x => aResult.Add(x.Id));
                }
            }

            //TypeName
            string aTypeName = BH.Engine.Adapters.Revit.Query.TypeName(filterRequest);
            if (!string.IsNullOrEmpty(aTypeName))
            {
                aElementIds = ElementIds(aDocument, "RevitAPI.dll", aTypeName);
                if (aElementIds != null)
                {
                    foreach (ElementId aElementId in aElementIds)
                            aResult.Add(aElementId);
                }
            }

            //FamilyName and FamilySymbolName
            if (aQueryType == RequestType.Family)
            {
                aElementIds = ElementIds(aDocument, BH.Engine.Adapters.Revit.Query.FamilyName(filterRequest), BH.Engine.Adapters.Revit.Query.FamilyTypeName(filterRequest), true);
                if (aElementIds != null && aElementIds.Count() > 0)
                    foreach (ElementId aElementId in aElementIds)
                            aResult.Add(aElementId);
            }

            //SelectionSet
            if (aQueryType == RequestType.SelectionSet)
            {
                aElementIds = ElementIds(aDocument, BH.Engine.Adapters.Revit.Query.SelectionSetName(filterRequest), true);
                if (aElementIds != null && aElementIds.Count() > 0)
                    foreach (ElementId aElementId in aElementIds)
                            aResult.Add(aElementId);
            }

            //Parameter
            if (aQueryType == RequestType.Parameter)
            {
                FilterRequest aFilterRequest = filterRequest.RelatedFilterRequest();
                if (aFilterRequest != null)
                {
                    string aParameterName = BH.Engine.Adapters.Revit.Query.ParameterName(filterRequest);
                    if (!string.IsNullOrWhiteSpace(aParameterName))
                    {
                        IComparisonRule aComparisonRule = BH.Engine.Adapters.Revit.Query.ComparisonRule(filterRequest);
                        if (aComparisonRule != null)
                        {
                            object aValue = BH.Engine.Adapters.Revit.Query.Value(filterRequest);

                            Dictionary<ElementId, List<FilterRequest>> aDictionary = aFilterRequest.FilterRequestDictionary(uIDocument);
                            if (aDictionary != null && aDictionary.Count > 0)
                            {
                                foreach (ElementId aElementId in aDictionary.Keys)
                                {
                                    Element aElement = aDocument.GetElement(aElementId);
                                    if (Query.IsValid(aElement, aParameterName, aComparisonRule, aValue, true))
                                        aResult.Add(aElementId);
                                }

                            }
                        }
                    }
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}