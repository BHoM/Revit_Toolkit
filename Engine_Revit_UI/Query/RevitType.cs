using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static Type RevitType(BuildingElementType buildingElementType)
        {
            switch (buildingElementType)
            {
                case oM.Environment.Elements.BuildingElementType.Ceiling:
                    return typeof(CeilingType);
                case oM.Environment.Elements.BuildingElementType.Floor:
                    return typeof(FloorType);
                case oM.Environment.Elements.BuildingElementType.Roof:
                    return typeof(RoofType);
                case oM.Environment.Elements.BuildingElementType.Wall:
                    return typeof(WallType);
                case oM.Environment.Elements.BuildingElementType.Door:
                    return typeof(FamilyInstance);
                case oM.Environment.Elements.BuildingElementType.Window:
                    return typeof(FamilyInstance);
            }

            return null;
        }

        /***************************************************/
    }
}
