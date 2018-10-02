using System.Collections.Generic;

using Autodesk.Revit.DB;
using System;
using System.Reflection;
using BH.oM.DataManipulation.Queries;
using System.Linq;
using BH.oM.Base;
using Autodesk.Revit.UI;

namespace BH.UI.Cobra.Engine
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
            else if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(BHoMObject)))
            {
                aTypes = RevitTypes(type);
                aBuiltInCategories = BuiltInCategories(type);
            }

            if (aTypes == null || aTypes.Count() == 0)
                return null;

            aTypes = aTypes.ToList().FindAll(x => BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(x, typeof(Element)));
            if (aTypes.Count() == 0)
                return null;

            if (aTypes.Count() == 1)
            {
                if (aBuiltInCategories == null || aBuiltInCategories.Count() == 0)
                    return new FilteredElementCollector(document).OfClass(aTypes.First()).ToElementIds();

                return new FilteredElementCollector(document).OfClass(aTypes.First()).WherePasses(new LogicalOrFilter(aBuiltInCategories.ToList().ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).ToElementIds();
            }
            else
            {
                if (aBuiltInCategories == null || aBuiltInCategories.Count() == 0)
                    return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(aTypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter))).ToElementIds();

                return new FilteredElementCollector(document).WherePasses(new LogicalAndFilter(new LogicalOrFilter(aTypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter)), new LogicalOrFilter(aTypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter)))).ToElementIds();
            }
                
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
                return null;


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

        public static IEnumerable<ElementId> ElementIds(this FilterQuery filterQuery, UIDocument uIDocument)
        {
            if (uIDocument == null || filterQuery == null)
                return null;

            List<ElementId> aResult = new List<ElementId>();
            Document aDocument = uIDocument.Document;

            IEnumerable<ElementId> aElementIds = null;

            //Type
            if (BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.Undefined && filterQuery.Type != null)
            {
                aElementIds = ElementIds(uIDocument.Document, filterQuery.Type);
                if (aElementIds != null)
                {
                    foreach (ElementId aElementId in aElementIds)
                        if (!aResult.Contains(aElementId))
                            aResult.Add(aElementId);
                }
            }

            //Workset
            string aWorksetName = BH.Engine.Adapters.Revit.Query.WorksetName(filterQuery);
            bool aActiveWorkset = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.ActiveWorkset;
            bool aOpenWorksets = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.OpenWorksets;
            aElementIds = ElementIds(aDocument, aActiveWorkset, aOpenWorksets, aWorksetName);
            if (aElementIds != null)
            {
                foreach (ElementId aElementId in aElementIds)
                    if (!aResult.Contains(aElementId))
                        aResult.Add(aElementId);
            }

            //Category
            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(filterQuery);
            if (!string.IsNullOrEmpty(aCategoryName))
            {
                aElementIds = ElementIds(aDocument, aCategoryName);
                if (aElementIds != null)
                {
                    foreach (ElementId aElementId in aElementIds)
                        if (!aResult.Contains(aElementId))
                            aResult.Add(aElementId);
                }
            }

            //IncludeSelected
            if (BH.Engine.Adapters.Revit.Query.IncludeSelected(filterQuery) && uIDocument.Selection != null)
            {
                ICollection<ElementId> aElementIdCollection = uIDocument.Selection.GetElementIds();
                if (aElementIdCollection != null)
                    foreach (ElementId aElementId in aElementIdCollection)
                            if (!aResult.Contains(aElementId))
                                aResult.Add(aElementId);
            }

            //ElementIds
            IEnumerable<int> aElementIds_Int = BH.Engine.Adapters.Revit.Query.ElementIds(filterQuery);
            if (aElementIds_Int != null)
            {
                foreach (int aId in aElementIds_Int)
                {
                    ElementId aElementId = new ElementId(aId);
                    Element aElement = aDocument.GetElement(aElementId);
                    if (aElement != null && !aResult.Contains(aElementId))
                        aResult.Add(aElementId);
                }
            }

            //UniqueIds
            IEnumerable<string> aUniqueIds = BH.Engine.Adapters.Revit.Query.UniqueIds(filterQuery);
            if (aUniqueIds != null)
            {
                foreach (string aUniqueId in aUniqueIds)
                {
                    Element aElement = aDocument.GetElement(aUniqueId);
                    if (aElement != null && !aResult.Contains(aElement.Id))
                        aResult.Add(aElement.Id);
                }
            }

            //ViewTemplate
            if (BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.ViewTemplate)
            {
                List<View> aViewList = new FilteredElementCollector(aDocument).OfClass(typeof(View)).Cast<View>().ToList();
                if (aViewList != null)
                {
                    aViewList.RemoveAll(x => !x.IsTemplate);
                    if (aViewList.Count > 0)
                    {
                        string aViewTemplateName = BH.Engine.Adapters.Revit.Query.ViewTemplateName(filterQuery);

                        if (!string.IsNullOrEmpty(aViewTemplateName))
                        {
                            View aView = aViewList.Find(x => x.Name == aViewTemplateName);
                            aViewList = new List<View>();
                            if (aView != null)
                                aViewList.Add(aView);
                        }

                        foreach (View aView in aViewList)
                            if (aView != null && !aResult.Contains(aView.Id))
                                aResult.Add(aView.Id);
                    }
                }

            }

            //TypeName
            string aTypeName = BH.Engine.Adapters.Revit.Query.TypeName(filterQuery);
            if (!string.IsNullOrEmpty(aTypeName))
            {
                aElementIds = ElementIds(aDocument, "RevitAPI.dll", aTypeName);
                if (aElementIds != null)
                {
                    foreach (ElementId aElementId in aElementIds)
                        if (!aResult.Contains(aElementId))
                            aResult.Add(aElementId);
                }
            }


            return aResult;
        }

        /***************************************************/
    }
}