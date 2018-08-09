using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit;
using BH.oM.Geometry;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static XYZ ToRevit(this Vector vector, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            if (pushSettings.ConvertUnits)
                return new XYZ(UnitUtils.ConvertToInternalUnits(vector.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(vector.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(vector.Z, DisplayUnitType.DUT_METERS));
            else
                return new XYZ(vector.X, vector.Y, vector.Z);
        }

        /***************************************************/
    }
}
