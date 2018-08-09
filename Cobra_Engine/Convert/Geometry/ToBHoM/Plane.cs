using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.CoordinateSystem ToBHoM(this Plane plane, PullSettings pullSettings = null)
        {
            if (plane == null)
                return null;

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            return Geometry.Create.CoordinateSystem(ToBHoM(plane.Origin, pullSettings), ToBHoMVector(plane.XVec, pullSettings), ToBHoMVector(plane.YVec, pullSettings));
        }

        /***************************************************/
    }
}