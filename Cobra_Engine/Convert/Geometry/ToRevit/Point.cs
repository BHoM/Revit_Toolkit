using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static XYZ ToRevit(this oM.Geometry.Point point, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            if (pushSettings.ConvertUnits)
                return new XYZ(UnitUtils.ConvertToInternalUnits(point.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(point.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(point.Z, DisplayUnitType.DUT_METERS));
            else
                return new XYZ(point.X, point.Y, point.Z);
        }

        /***************************************************/
    }
}