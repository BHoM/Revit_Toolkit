using Autodesk.Revit.DB;
using BH.Engine.Geometry;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Point ToBHoM(this XYZ xyz, bool convertUnits = true)
        {
            if (xyz == null)
                return null;

            if (convertUnits)
                return Create.Point(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return Create.Point(xyz.X, xyz.Y, xyz.Z);
        }

        /***************************************************/

        internal static oM.Geometry.Vector ToBHoMVector(this XYZ xyz, bool convertUnits = true)
        {
            if (xyz == null)
                return null;

            if (convertUnits)
                return Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(xyz.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertFromInternalUnits(xyz.Z, DisplayUnitType.DUT_METERS));
            else
                return Geometry.Create.Vector(xyz.X, xyz.Y, xyz.Z);
        }

        /***************************************************/
    }
}