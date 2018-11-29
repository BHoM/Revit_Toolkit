using Autodesk.Revit.DB;

using BH.Engine.Geometry;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Vector ToBHoMVector(this XYZ xyz, PullSettings pullSettings = null)
        {
            if (xyz == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            if (pullSettings.ConvertUnits)
                return Create.Vector(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return Create.Vector(xyz.X, xyz.Y, xyz.Z);
        }

        /***************************************************/
    }
}