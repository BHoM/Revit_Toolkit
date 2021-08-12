/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
#if (REVIT2018 || REVIT2019 || REVIT2020)

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static DisplayUnitType BHoMUnitType(this UnitType unitType)
        {
            // Check if any display unit type applicable to given unit type is acceptable BHoM unit type.
            IEnumerable<DisplayUnitType> duts = UnitUtils.GetValidDisplayUnits(unitType);
            foreach (DisplayUnitType dut in duts)
            {
                if (BHoMUnits.Contains(dut))
                    return dut;
            }

            // Check if any display unit type applicable to given unit type has acceptable BHoM equivalent unit type.
            foreach (DisplayUnitType dut in duts)
            {
                if (BHoMEquivalents.ContainsKey(dut))
                    return BHoMEquivalents[dut];
            }

            // Find any SI display unit types.
            List<DisplayUnitType> acceptable = duts.Where(x =>
            {
                string lower = x.ToString().ToLower();
                return !NonSIUnitNames.Any(y => lower.Contains(y));
            }).ToList();

            // Return first SI display unit type or record error.
            if (acceptable.Count != 0)
            {
                DisplayUnitType dut = acceptable.First();
                BH.Engine.Reflection.Compute.RecordWarning(String.Format("Unit type {0} does not have a unitless SI convert - instead, it has been converted to {1}.", LabelUtils.GetLabelFor(unitType), LabelUtils.GetLabelFor(dut)));
                return dut;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Unit type {0} has not been recognized and has not been converted - as a result, the output value can be wrong.", LabelUtils.GetLabelFor(unitType)));
                return DisplayUnitType.DUT_GENERAL;
            }
        }


        /***************************************************/
        /****            Private collections            ****/
        /***************************************************/

        private static readonly string[] NonSIUnitNames = new string[]
        {
            "inch",
            "feet",
            "foot",
            "yard",
            "fahrenheit",
            "pound",
            "gallon",
            "kip",
            "ustonne",
            "mile",
            "rankine",
            "horsepower"
        };

        /***************************************************/

        private static readonly DisplayUnitType[] BHoMUnits = new DisplayUnitType[]
        {
            DisplayUnitType.DUT_METERS,
            DisplayUnitType.DUT_SQUARE_METERS,
            DisplayUnitType.DUT_CUBIC_METERS,
            DisplayUnitType.DUT_JOULES,
            DisplayUnitType.DUT_PASCALS_PER_METER,
            DisplayUnitType.DUT_WATTS,
            DisplayUnitType.DUT_WATTS_PER_SQUARE_METER,
            DisplayUnitType.DUT_PASCALS,
            DisplayUnitType.DUT_CELSIUS,
            DisplayUnitType.DUT_METERS_PER_SECOND,
            DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND,
            DisplayUnitType.DUT_AMPERES,
            DisplayUnitType.DUT_KILOGRAMS_MASS,
            DisplayUnitType.DUT_VOLTS,
            DisplayUnitType.DUT_HERTZ,
            DisplayUnitType.DUT_LUX,
            DisplayUnitType.DUT_CANDELAS_PER_SQUARE_METER,
            DisplayUnitType.DUT_CANDELAS,
            DisplayUnitType.DUT_LUMENS,
            DisplayUnitType.DUT_NEWTONS,
            DisplayUnitType.DUT_NEWTONS_PER_METER,
            DisplayUnitType.DUT_NEWTON_METERS,
            DisplayUnitType.DUT_PASCAL_SECONDS,
            DisplayUnitType.DUT_INV_CELSIUS,
            DisplayUnitType.DUT_NEWTON_METERS_PER_METER,
            DisplayUnitType.DUT_WATTS_PER_SQUARE_METER_KELVIN,
            DisplayUnitType.DUT_WATTS_PER_CUBIC_METER,
            DisplayUnitType.DUT_LUMENS_PER_WATT,
            DisplayUnitType.DUT_SQUARE_METER_KELVIN_PER_WATT,
            DisplayUnitType.DUT_JOULES_PER_KELVIN,
            DisplayUnitType.DUT_METERS_PER_SECOND_SQUARED,
            DisplayUnitType.DUT_METERS_TO_THE_FOURTH_POWER,
            DisplayUnitType.DUT_METERS_TO_THE_SIXTH_POWER,
            DisplayUnitType.DUT_RADIANS,
            DisplayUnitType.DUT_RADIANS_PER_SECOND,
            DisplayUnitType.DUT_SECONDS,
            DisplayUnitType.DUT_WATTS_PER_METER_KELVIN,
            DisplayUnitType.DUT_OHM_METERS,
            DisplayUnitType.DUT_KELVIN_DIFFERENCE,
            DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER,
            DisplayUnitType.DUT_KILOGRAMS_MASS_PER_METER,
            DisplayUnitType.DUT_KILOGRAMS_MASS_PER_SQUARE_METER,
            DisplayUnitType.DUT_JOULES_PER_KILOGRAM_CELSIUS,
            DisplayUnitType.DUT_GENERAL
        };

        /***************************************************/

        private static readonly Dictionary<DisplayUnitType, DisplayUnitType> BHoMEquivalents = new Dictionary<DisplayUnitType, DisplayUnitType>
        {
            { DisplayUnitType.DUT_CURRENCY,  DisplayUnitType.DUT_GENERAL },
            { DisplayUnitType.DUT_KELVIN,  DisplayUnitType.DUT_CELSIUS },
            { DisplayUnitType.DUT_PERCENTAGE,  DisplayUnitType.DUT_FIXED },
            { DisplayUnitType.DUT_SQUARE_METERS_PER_METER,  DisplayUnitType.DUT_METERS },
            { DisplayUnitType.DUT_MILLIMETERS,  DisplayUnitType.DUT_METERS }
        };

        /***************************************************/

#else
    
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static ForgeTypeId BHoMUnitType(this ForgeTypeId unitType)
        {
            // Check if any display unit type applicable to given unit type is acceptable BHoM unit type.
            IEnumerable<ForgeTypeId> duts = UnitUtils.GetValidUnits(unitType);
            foreach (ForgeTypeId dut in duts)
            {
                if (BHoMUnits.Contains(dut))
                    return dut;
            }

            // Check if any display unit type applicable to given unit type has acceptable BHoM equivalent unit type.
            foreach (ForgeTypeId dut in duts)
            {
                if (BHoMEquivalents.ContainsKey(dut))
                    return BHoMEquivalents[dut];
            }

            // Find any SI display unit types.
            List<ForgeTypeId> acceptable = duts.Where(x =>
            {
                string lower = x.ToString().ToLower();
                return !NonSIUnitNames.Any(y => lower.Contains(y));
            }).ToList();

            // Return first SI display unit type or record error.
            if (acceptable.Count != 0)
            {
                ForgeTypeId dut = acceptable.First();
                BH.Engine.Reflection.Compute.RecordWarning($"Unit type {LabelUtils.GetLabelForSpec(unitType)} does not have a unitless SI convert - instead, it has been converted to {LabelUtils.GetLabelForUnit(dut)}.");
                return dut;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError($"Unit type {LabelUtils.GetLabelForSpec(unitType)} has not been recognized and has not been converted - as a result, the output value can be wrong.");
                return UnitTypeId.General;
            }
        }


        /***************************************************/
        /****            Private collections            ****/
        /***************************************************/

        private static readonly string[] NonSIUnitNames = new string[]
        {
            "inch",
            "feet",
            "foot",
            "yard",
            "fahrenheit",
            "pound",
            "gallon",
            "kip",
            "ustonne",
            "mile",
            "rankine",
            "horsepower"
        };

        /***************************************************/

        private static readonly ForgeTypeId[] BHoMUnits = new ForgeTypeId[]
        {
            UnitTypeId.Meters,
            UnitTypeId.SquareMeters,
            UnitTypeId.CubicMeters,
            UnitTypeId.Joules,
            UnitTypeId.PascalsPerMeter,
            UnitTypeId.Watts,
            UnitTypeId.WattsPerSquareMeter,
            UnitTypeId.Pascals,
            UnitTypeId.Celsius,
            UnitTypeId.MetersPerSecond,
            UnitTypeId.CubicMetersPerSecond,
            UnitTypeId.Amperes,
            UnitTypeId.Kilograms,
            UnitTypeId.Volts,
            UnitTypeId.Hertz,
            UnitTypeId.Lux,
            UnitTypeId.CandelasPerSquareMeter,
            UnitTypeId.Candelas,
            UnitTypeId.Lumens,
            UnitTypeId.Newtons,
            UnitTypeId.NewtonsPerMeter,
            UnitTypeId.NewtonMeters,
            UnitTypeId.PascalSeconds,
            UnitTypeId.InverseDegreesCelsius,
            UnitTypeId.NewtonMetersPerMeter,
            UnitTypeId.WattsPerSquareMeterKelvin,
            UnitTypeId.WattsPerCubicMeter,
            UnitTypeId.LumensPerWatt,
            UnitTypeId.SquareMeterKelvinsPerWatt,
            UnitTypeId.JoulesPerKelvin,
            UnitTypeId.MetersPerSecondSquared,
            UnitTypeId.MetersToTheFourthPower,
            UnitTypeId.MetersToTheSixthPower,
            UnitTypeId.Radians,
            UnitTypeId.RadiansPerSecond,
            UnitTypeId.Seconds,
            UnitTypeId.WattsPerMeterKelvin,
            UnitTypeId.OhmMeters,
            UnitTypeId.KelvinInterval,
            UnitTypeId.KilogramsPerCubicMeter,
            UnitTypeId.KilogramsPerMeter,
            UnitTypeId.KilogramsPerSquareMeter,
            UnitTypeId.JoulesPerKilogramDegreeCelsius,
            UnitTypeId.General,
        };

        /***************************************************/

        private static readonly Dictionary<ForgeTypeId, ForgeTypeId> BHoMEquivalents = new Dictionary<ForgeTypeId, ForgeTypeId>
        {
            { UnitTypeId.Currency,  UnitTypeId.General },
            { UnitTypeId.Kelvin,  UnitTypeId.Celsius },
            { UnitTypeId.Percentage,  UnitTypeId.Fixed },
            { UnitTypeId.SquareMetersPerMeter,  UnitTypeId.Meters },
            { UnitTypeId.Millimeters,  UnitTypeId.Meters }
        };

        /***************************************************/

#endif
    }
}


