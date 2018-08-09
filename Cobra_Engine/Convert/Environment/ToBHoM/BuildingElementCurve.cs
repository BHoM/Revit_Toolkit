using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit;
using BH.oM.Environment.Elements;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = (wall.Location as LocationCurve).ToBHoM(pullSettings)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/
    }
}
