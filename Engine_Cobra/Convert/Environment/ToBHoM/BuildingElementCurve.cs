using Autodesk.Revit.DB;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = (wall.Location as LocationCurve).ToBHoM(pullSettings)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/
    }
}