using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Base;

using Autodesk.Revit.DB;
using BH.oM.Structural.Elements;
using BH.oM.Environmental.Elements;

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
            }
            if(type == typeof(BuildingElement))
            {
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Doors);
                aBuiltInCategoryList.Add(BuiltInCategory.OST_Windows);
            }

            return aBuiltInCategoryList;
        }

        /***************************************************/
    }
}
