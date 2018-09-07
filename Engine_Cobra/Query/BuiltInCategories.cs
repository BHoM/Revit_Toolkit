using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
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

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static IEnumerable<BuiltInCategory> BuiltInCategories(this RevitSettings revitSettings, Document document)
        {
            if (document == null || revitSettings == null)
                return null;

            List<BuiltInCategory> aResult = new List<BuiltInCategory>();

            if (revitSettings.SelectionSettings == null || revitSettings.SelectionSettings.CategoryNames == null)
                return aResult;

            Categories aCategories = document.Settings.Categories;
            foreach (string aCategoryName in revitSettings.SelectionSettings.CategoryNames)
            {
                foreach (Category aCategory in aCategories)
                    if (aCategory.Name == aCategoryName)
                    {
                        BuiltInCategory aBuiltInCategory = (BuiltInCategory)aCategory.Id.IntegerValue;
                        aResult.Add(aBuiltInCategory);
                        break;
                    }
            }
            return aResult;
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
    }
}