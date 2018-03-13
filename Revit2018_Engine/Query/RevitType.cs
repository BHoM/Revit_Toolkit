using System;

using BH.oM.Environmental.Elements;

using Autodesk.Revit.DB;

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
        /// Get Revit class type from BHoM BuildingElementType.
        /// </summary>
        /// <param name="buildingElementType">BHoM BuildingElementType</param>
        /// <returns name="Type">Revit class Type</returns>
        /// <search>
        /// Query, GetType, Revit, Get Type, BuildingElementType
        /// </search>
        public static Type RevitType(BuildingElementType buildingElementType)
        {
            switch (buildingElementType)
            {
                case BH.oM.Environmental.Elements.BuildingElementType.Ceiling:
                    return typeof(CeilingType);
                case BH.oM.Environmental.Elements.BuildingElementType.Floor:
                    return typeof(FloorType);
                case BH.oM.Environmental.Elements.BuildingElementType.Roof:
                    return typeof(RoofType);
                case BH.oM.Environmental.Elements.BuildingElementType.Wall:
                    return typeof(WallType);
                case BH.oM.Environmental.Elements.BuildingElementType.Door:
                    return typeof(FamilyInstance);
                case BH.oM.Environmental.Elements.BuildingElementType.Window:
                    return typeof(FamilyInstance);
            }

            return null;
        }

        /***************************************************/
    }
}
