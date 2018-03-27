using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Structural.Elements;

using BH.oM.Environmental.Elements;
using BH.oM.Environmental.Properties;
using BH.oM.Environmental.Interface;
using BH.oM.Geometry;

using BH.Engine.Environment;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{

    /// <summary>
    /// BHoM Revit Engine Convert Methods
    /// </summary>
    public static partial class Convert
    {
        public static double ToSI(this double Value, UnitType UnitType)
        {
            switch(UnitType)
            {
                case UnitType.UT_Length:
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
                default:
                    return Value;
            }
        }
    }
}