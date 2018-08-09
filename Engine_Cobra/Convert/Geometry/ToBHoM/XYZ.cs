using Autodesk.Revit.DB;
using BH.Engine.Geometry;
using BH.oM.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Point ToBHoM(this XYZ xyz, PullSettings pullSettings = null)
        {
            if (xyz == null)
                return null;

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            if (pullSettings.ConvertUnits)
                return Create.Point(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return Create.Point(xyz.X, xyz.Y, xyz.Z);
        }

        /***************************************************/

        internal static oM.Geometry.Vector ToBHoMVector(this XYZ xyz, PullSettings pullSettings = null)
        {
            if (xyz == null)
                return null;

            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            if (pullSettings.ConvertUnits)
                return Create.Vector(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return Create.Vector(xyz.X, xyz.Y, xyz.Z);
        }

        /***************************************************/
    }
}