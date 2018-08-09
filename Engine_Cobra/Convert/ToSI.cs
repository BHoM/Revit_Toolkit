using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static double ToSI(this double Value, UnitType UnitType)
        {
            switch(UnitType)
            {
                case UnitType.UT_Length:
                case UnitType.UT_Bar_Diameter:
                case UnitType.UT_Section_Dimension:
                case UnitType.UT_Section_Property:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_METERS);
                case UnitType.UT_Mass:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_KILOGRAMS_MASS);
                case UnitType.UT_Electrical_Current:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_AMPERES);
                case UnitType.UT_HVAC_Temperature:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_KELVIN);
                case UnitType.UT_Weight:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_KILOGRAMS_MASS);
                case UnitType.UT_HVAC_Pressure:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_PASCALS);
                case UnitType.UT_Piping_Pressure:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_PASCALS);
                case UnitType.UT_HVAC_Velocity:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_METERS_PER_SECOND);
                case UnitType.UT_Area:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_SQUARE_METERS);
                case UnitType.UT_Volume:
                    return UnitUtils.ConvertFromInternalUnits(Value, DisplayUnitType.DUT_CUBIC_METERS);
                default:
                    return Value;
            }
        }

        /***************************************************/
    }
}