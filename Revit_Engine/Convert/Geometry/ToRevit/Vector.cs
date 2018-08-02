using Autodesk.Revit.DB;
using BH.oM.Geometry;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static XYZ ToRevit(this Vector vector, bool convertUnits = true)
        {
            if (convertUnits)
                return new XYZ(UnitUtils.ConvertToInternalUnits(vector.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(vector.Y, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(vector.Z, DisplayUnitType.DUT_METERS));
            else
                return new XYZ(vector.X, vector.Y, vector.Z);
        }

        /***************************************************/
    }
}