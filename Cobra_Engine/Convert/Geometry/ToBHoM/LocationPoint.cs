using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
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

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            return ToBHoM(locationPoint.Point, pullSettings);
        }

        /***************************************************/
    }
}