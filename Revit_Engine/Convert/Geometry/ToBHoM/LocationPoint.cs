using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static oM.Geometry.Point ToBHoM(this LocationPoint locationPoint, bool convertUnits = true)
        {
            if (locationPoint == null)
                return null;

            return ToBHoM(locationPoint.Point, convertUnits);
        }

        /***************************************************/
    }
}