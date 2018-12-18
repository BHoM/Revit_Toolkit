using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
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

        internal static oM.Geometry.Point ToBHoM(this XYZ xyz, PullSettings pullSettings = null)
        {
            if (xyz == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            if (pullSettings.ConvertUnits)
                return BH.Engine.Geometry.Create.Point(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return BH.Engine.Geometry.Create.Point(xyz.X, xyz.Y, xyz.Z);
        }

        /***************************************************/
    }
}