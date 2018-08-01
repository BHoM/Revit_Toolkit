using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.Engine.Environment;

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
    }
}
