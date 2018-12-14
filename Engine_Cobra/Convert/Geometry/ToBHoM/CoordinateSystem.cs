using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.CoordinateSystem.Cartesian ToBHoM(this Plane plane, PullSettings pullSettings = null)
        {
            if (plane == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return BH.Engine.Geometry.Create.CartesianCoordinateSystem(ToBHoM(plane.Origin, pullSettings), ToBHoMVector(plane.XVec, pullSettings), ToBHoMVector(plane.YVec, pullSettings));
        }

        /***************************************************/
    }
}