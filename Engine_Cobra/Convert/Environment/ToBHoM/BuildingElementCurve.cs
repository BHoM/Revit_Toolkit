using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
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