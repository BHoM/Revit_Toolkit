using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using BH.oM.DataManipulation.Queries;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(this Type type)
        {
            if (type == null)
                return null;

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>();

            if (type == typeof(FramingElement))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFraming);
                //aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFoundation);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralColumns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Columns);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_VerticalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Truss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralTruss);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_HorizontalBracing);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Purlin);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Joist);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Girder);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralStiffener);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_StructuralFramingOther);
            }
            if (type == typeof(BuildingElement))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Doors);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Windows);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Walls);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Ceilings);
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
            }
            if(type == typeof(oM.Adapters.Revit.Elements.Sheet))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Sheets);
            }
            if (type == typeof(oM.Adapters.Revit.Elements.Viewport))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Viewports);
            }
            if (type == typeof(oM.Adapters.Revit.Elements.ViewPlan))
            {
                aBuiltInCategoryList.Add(Autodesk.Revit.DB.BuiltInCategory.OST_Views);
            }

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(IEnumerable<Element> elements)
        {
            if (elements == null)
                return null;

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>();
            foreach (Element aElement in elements)
                if (aElement.Category != null)
                {
                    BuiltInCategory aBuiltInCategory = (BuiltInCategory)aElement.Category.Id.IntegerValue;
                    if (!aBuiltInCategoryList.Contains(aBuiltInCategory))
                        aBuiltInCategoryList.Add(aBuiltInCategory);
                }

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(IEnumerable<FilterQuery> filterQueries, Document document)
        {
            if (filterQueries == null || document == null)
                return null;

            List<BuiltInCategory> aBuiltInCategories = new List<BuiltInCategory>();
            foreach (FilterQuery aFilterQuery in filterQueries)
            {
                BuiltInCategory aBuiltInCategory = Query.BuiltInCategory(aFilterQuery, document);
                if (aBuiltInCategory != Autodesk.Revit.DB.BuiltInCategory.INVALID)
                    aBuiltInCategories.Add(aBuiltInCategory);
            }
            return aBuiltInCategories;
        }
    }
}