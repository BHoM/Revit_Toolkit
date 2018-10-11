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
                        return null;

                    foreach (ElementId aElementId in new FilteredElementCollector(document).OfClass(aType).ToElementIds())
                        aResult.Add(aElementId);
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

            oM.Adapters.Revit.Enums.QueryType aQueryType = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery);

            //Type
            if (aQueryType == oM.Adapters.Revit.Enums.QueryType.Undefined && filterQuery.Type != null)
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
            bool aActiveWorkset = aQueryType == oM.Adapters.Revit.Enums.QueryType.ActiveWorkset;
            bool aOpenWorksets = aQueryType == oM.Adapters.Revit.Enums.QueryType.OpenWorksets;
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
            if (aQueryType == oM.Adapters.Revit.Enums.QueryType.ViewTemplate)
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

            //FamilyName and FamilySymbolName
            if (aQueryType == oM.Adapters.Revit.Enums.QueryType.Family)
            {
                aElementIds = ElementIds(aDocument, BH.Engine.Adapters.Revit.Query.FamilyName(filterQuery), BH.Engine.Adapters.Revit.Query.FamilySymbolName(filterQuery), true);
                if(aElementIds != null && aElementIds.Count() > 0)
                    foreach(ElementId aElementId in aElementIds)
                        if(!aResult.Contains(aElementId))
                            aResult.Add(aElementId);
            }

            //SelectionSet
            if (aQueryType == oM.Adapters.Revit.Enums.QueryType.SelectionSet)
            {
                aElementIds = ElementIds(aDocument, BH.Engine.Adapters.Revit.Query.SelectionSetName(filterQuery), true);
                if (aElementIds != null && aElementIds.Count() > 0)
                    foreach (ElementId aElementId in aElementIds)
                        if (!aResult.Contains(aElementId))
                            aResult.Add(aElementId);
            }

            return aResult;
        }

        /***************************************************/
    }
}