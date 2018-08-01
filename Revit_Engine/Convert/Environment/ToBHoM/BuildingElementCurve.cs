using Autodesk.Revit.DB;

using BH.oM.Environment.Elements;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall, bool convertUnits = true)
        {
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = (wall.Location as LocationCurve).ToBHoM(convertUnits)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/
    }
}
