using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Environmental.Elements;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets BuildingElementType from Revit BuiltInCategory. If no match then null will be returned.
        /// </summary>
        /// <param name="builtInCategory">Revit BuiltInCategory</param>
        /// <returns name="BuildingElementType">BHoM BuildingElementType</returns>
        /// <search>
        /// Utilis, BHoM, GetBuildingElementType, Get BuildingElementType, BuiltInCategory, Revit
        /// </search>
        public static BuildingElementType? BuildingElementType(this BuiltInCategory builtInCategory)
        {
            switch (builtInCategory)
            {
                case BuiltInCategory.OST_Ceilings:
                    return BH.oM.Environmental.Elements.BuildingElementType.Ceiling;
                case BuiltInCategory.OST_Floors:
                    return BH.oM.Environmental.Elements.BuildingElementType.Floor;
                case BuiltInCategory.OST_Roofs:
                    return BH.oM.Environmental.Elements.BuildingElementType.Roof;
                case BuiltInCategory.OST_Walls:
                    return BH.oM.Environmental.Elements.BuildingElementType.Wall;
                case BuiltInCategory.OST_Windows:
                    return BH.oM.Environmental.Elements.BuildingElementType.Window;
                case BuiltInCategory.OST_Doors:
                    return BH.oM.Environmental.Elements.BuildingElementType.Door;
            }

            return null;
        }

        /***************************************************/
    }
}
