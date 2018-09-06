using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Point ToBHoM(this LocationPoint locationPoint, PullSettings pullSettings = null)
        {
            if (locationPoint == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoM(locationPoint.Point, pullSettings);
        }

        /***************************************************/
    }
}