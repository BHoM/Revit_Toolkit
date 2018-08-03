using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

using Autodesk.Revit.DB;
using BH.oM.Structural.Elements;
using BH.oM.Environment.Elements;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Returs discipline of BHoM type. Default value: Revit.Discipline.Environmental
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns name="BuiltInCategories">Revit BuiltInCategory List</returns>
        /// <search>
        /// Query, BHoM, BuiltInCategories, BuiltInCategory, BHoMObject
        /// </search>
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
