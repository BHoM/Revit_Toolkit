using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using BH.oM.Structural.Elements;
using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<BuiltInCategory> BuiltInCategories(this Type type)
        {
            if (type == null)
                return null;

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>();

            if (type == typeof(FramingElement))
            {
                aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFraming);
                //aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFoundation);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralColumns);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Columns);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_VerticalBracing);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Truss);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralTruss);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_HorizontalBracing);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Purlin);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Joist);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Girder);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralStiffener);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_StructuralFramingOther);
            }
            if(type == typeof(BuildingElement))
            {
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Doors);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Windows);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Walls);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Floors);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Ceilings);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Roofs);
            }

            return aBuiltInCategoryList;
        }

        /***************************************************/
    }
}