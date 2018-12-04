using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static double FromSI(this double Value, UnitType UnitType)
        {
            switch(UnitType)
            {
                case UnitType.UT_Length:
                case UnitType.UT_Bar_Diameter:
                case UnitType.UT_Section_Dimension:
                case UnitType.UT_Section_Property:
                case UnitType.UT_PipeSize:
                case UnitType.UT_HVAC_DuctSize:
                case UnitType.UT_HVAC_DuctLiningThickness:
                case UnitType.UT_HVAC_DuctInsulationThickness:
                case UnitType.UT_PipeInsulationThickness:
                case UnitType.UT_SheetLength:
                case UnitType.UT_WireSize:
                case UnitType.UT_Crack_Width:
                case UnitType.UT_DecSheetLength:
                case UnitType.UT_Electrical_CableTraySize:
                case UnitType.UT_Electrical_ConduitSize:
                case UnitType.UT_Reinforcement_Length:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_METERS);
                case UnitType.UT_Mass:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_KILOGRAMS_MASS);
                case UnitType.UT_Electrical_Current:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_AMPERES);
                case UnitType.UT_HVAC_Temperature:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_KELVIN);
                case UnitType.UT_Weight:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_KILOGRAMS_MASS);
                case UnitType.UT_HVAC_Pressure:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_PASCALS);
                case UnitType.UT_Piping_Pressure:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_PASCALS);
                case UnitType.UT_HVAC_Velocity:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_METERS_PER_SECOND);
                case UnitType.UT_Area:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_SQUARE_METERS);
                case UnitType.UT_Volume:
                    return UnitUtils.ConvertToInternalUnits(Value, DisplayUnitType.DUT_CUBIC_METERS);
                default:
                    return Value;
            }
        }

        /***************************************************/
    }
}