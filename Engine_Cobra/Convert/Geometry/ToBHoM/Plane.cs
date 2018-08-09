using Autodesk.Revit.DB;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
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

            return BH.Engine.Geometry.Create.CoordinateSystem(ToBHoM(plane.Origin, pullSettings), ToBHoMVector(plane.XVec, pullSettings), ToBHoMVector(plane.YVec, pullSettings));
        }

        /***************************************************/
    }
}