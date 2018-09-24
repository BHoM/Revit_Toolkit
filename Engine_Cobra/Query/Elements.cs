using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using System.Reflection;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<Element> Elements(this Document document, Type type)
        {
            if (document == null || type == null)
                return null;

            IEnumerable<Type> aTypes = null;
            if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(Element)))
                aTypes = new List<Type>() { type };
            else if (BH.Engine.Adapters.Revit.Query.IsAssignableFromByFullName(type, typeof(BHoMObject)))
                aTypes = RevitTypes(type);

            if (aTypes == null || aTypes.Count() == 0)
                return null;

            if (aTypes.Count() == 1)
                return new FilteredElementCollector(document).OfClass(aTypes.First());
            else
                return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(aTypes.ToList().ConvertAll(x => new ElementClassFilter(x) as ElementFilter)));
        }

        /***************************************************/

        public static IEnumerable<Element> Elements(this Document document, string categoryName)
        {
            if (document == null || string.IsNullOrEmpty(categoryName))
                return null;

            BuiltInCategory aBuiltInCategory = Query.BuiltInCategory(document, categoryName);
            if (aBuiltInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                return null;

            return new FilteredElementCollector(document).OfCategory(aBuiltInCategory);
        }

        /***************************************************/

        public static IEnumerable<Element> Elements(this Document document, string currentDomainAssembly, string typeName)
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


            return new FilteredElementCollector(document).OfClass(aType);
        }

        /***************************************************/

        public static IEnumerable<Element> Elements(this Document document, bool activeWorkset, bool openWorksets, string worksetName = null)
        {
            if (document == null)
                return null;

            List<WorksetId> aWorksetIdList = new List<WorksetId>();
            if(!string.IsNullOrEmpty(worksetName))
            {
                WorksetId aWorksetId = Query.WorksetId(document, worksetName);
                if (aWorksetId != null && aWorksetIdList.Find(x => x == aWorksetId) == null)
                    aWorksetIdList.Add(aWorksetId);
            }

            if(activeWorkset)
            {
                WorksetId aWorksetId = Query.ActiveWorksetId(document);
                if (aWorksetId != null && aWorksetIdList.Find(x => x == aWorksetId) == null)
                    aWorksetIdList.Add(aWorksetId);
            }

            if(openWorksets)
            {
                IEnumerable<WorksetId> aWorksetIds = Query.OpenWorksetIds(document);
                if (aWorksetIds != null && aWorksetIds.Count() > 0)
                {
                    foreach(WorksetId aWorksetId in aWorksetIds)
                        if (aWorksetId != null && aWorksetIdList.Find(x => x == aWorksetId) == null)
                            aWorksetIdList.Add(aWorksetId);
                }
            }

            if (aWorksetIdList == null || aWorksetIdList.Count == 0)
                return null;

            if(aWorksetIdList.Count == 1)
                return new FilteredElementCollector(document).WherePasses(new ElementWorksetFilter(aWorksetIdList.First(), false)).ToElements();
            else
                return new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(aWorksetIdList.ConvertAll(x => new ElementWorksetFilter(x, false) as ElementFilter))).ToElements();
        }

        /***************************************************/

        public static IEnumerable<Element> Elements(this FilterQuery filterQuery, UIDocument uIDocument)
        {
            if (uIDocument == null || filterQuery == null)
                return null;

            Dictionary<int, Element> aDictionary_Elements = new Dictionary<int, Element>();

            IEnumerable<Element> aElements = null;

            Document aDocument = uIDocument.Document;

            //Type
            if (BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.Undefined && filterQuery.Type != null)
            {
                aElements = Elements(uIDocument.Document, filterQuery.Type);
                if (aElements != null)
                {
                    foreach (Element aElement in aElements)
                        if (!aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue))
                            aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }

            //Workset
            string aWorksetName = BH.Engine.Adapters.Revit.Query.WorksetName(filterQuery);
            bool aActiveWorkset = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.ActiveWorkset;
            bool aOpenWorksets = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.OpenWorksets;
            aElements = Elements(aDocument, aActiveWorkset, aOpenWorksets, aWorksetName);
            if(aElements != null)
            {
                foreach (Element aElement in aElements)
                    if (!aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue))
                        aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
            }

            //Category
            string aCategoryName = BH.Engine.Adapters.Revit.Query.CategoryName(filterQuery);
            if (!string.IsNullOrEmpty(aCategoryName))
            {
                aElements = Elements(aDocument, aCategoryName);
                if(aElements != null)
                {
                    foreach(Element aElement in aElements)
                        if (!aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue))
                            aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }

            //IncludeSelected
            if (BH.Engine.Adapters.Revit.Query.IncludeSelected(filterQuery) && uIDocument.Selection != null)
            {
                ICollection<ElementId> aElementIdCollection = uIDocument.Selection.GetElementIds();
                if (aElementIdCollection != null)
                    foreach (ElementId aElementId in aElementIdCollection)
                        if(!aDictionary_Elements.ContainsKey(aElementId.IntegerValue))
                        {
                            Element aElement = aDocument.GetElement(aElementId);
                            if(aElement != null)
                                aDictionary_Elements.Add(aElementId.IntegerValue, aElement);
                        }
            }

            //ElementIds
            IEnumerable<int> aElementIds = BH.Engine.Adapters.Revit.Query.ElementIds(filterQuery);
            if(aElementIds != null)
            {
                foreach (int aId in aElementIds)
                {
                    ElementId aElementId = new ElementId(aId);
                    Element aElement = aDocument.GetElement(aElementId);
                    if (aElement != null && !aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue))
                        aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }

            //UniqueIds
            IEnumerable<string> aUniqueIds = BH.Engine.Adapters.Revit.Query.UniqueIds(filterQuery);
            if(aUniqueIds != null)
            {
                foreach (string aUniqueId in aUniqueIds)
                {
                    Element aElement = aDocument.GetElement(aUniqueId);
                    if (aElement != null && !aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue))
                        aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }

            //ViewTemplate
            if(BH.Engine.Adapters.Revit.Query.QueryType(filterQuery) == oM.Adapters.Revit.Enums.QueryType.ViewTemplate)
            {
                List<View> aViewList = new FilteredElementCollector(aDocument).OfClass(typeof(View)).Cast<View>().ToList();
                if(aViewList != null)
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
                            if (aView != null && !aDictionary_Elements.ContainsKey(aView.Id.IntegerValue))
                                aDictionary_Elements.Add(aView.Id.IntegerValue, aView);
                    }
                }

            }

            //TypeName
            string aTypeName = BH.Engine.Adapters.Revit.Query.TypeName(filterQuery);
            if (!string.IsNullOrEmpty(aTypeName))
            {
                aElements = Elements(aDocument, "RevitAPI.dll", aTypeName);
                if (aElements != null)
                {
                    foreach (Element aElement in aElements)
                        if (!aDictionary_Elements.ContainsKey(aElement.Id.IntegerValue))
                            aDictionary_Elements.Add(aElement.Id.IntegerValue, aElement);
                }
            }


            return aDictionary_Elements.Values;
        }

        /***************************************************/
    }
}