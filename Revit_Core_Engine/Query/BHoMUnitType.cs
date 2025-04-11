/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds a BHoM-specific Revit unit type for a given quantity.")]
        [Input("quantity", "Quantity to find a BHoM-specific Revit unit type for.")]
        [Output("unitType", "BHoM-specific Revit unit type for the input quantity.")]
        public static ForgeTypeId BHoMUnitType(this ForgeTypeId quantity)
        {
#if (!REVIT2021)
            if (quantity == SpecTypeId.Currency || !UnitUtils.IsMeasurableSpec(quantity))
                return null;
#endif

            // Check if any display unit type applicable to given unit type is acceptable BHoM unit type.
            IEnumerable<ForgeTypeId> duts = UnitUtils.GetValidUnits(quantity);
            foreach (ForgeTypeId dut in duts)
            {
                if (BHoMUnitsNew.Contains(dut))
                    return dut;
            }

            // Check if any display unit type applicable to given unit type has acceptable BHoM equivalent unit type.
            foreach (ForgeTypeId dut in duts)
            {
                if (BHoMEquivalentsNew.ContainsKey(dut))
                    return BHoMEquivalentsNew[dut];
            }

            // Find any SI display unit types.
            List<ForgeTypeId> acceptable = duts.Where(x =>
            {
                string lower = LabelUtils.GetLabelForUnit(x).ToLower();
                return !NonSIUnitNames.Any(y => lower.Contains(y));
            }).ToList();

            // Return first SI display unit type or record error.
            if (acceptable.Count != 0)
            {
                ForgeTypeId dut = acceptable.First();
                BH.Engine.Base.Compute.RecordWarning($"Unit type {LabelUtils.GetLabelForSpec(quantity)} does not have a predefined SI equivalent in BHoM - the unit type used on the BHoM side of convert is {LabelUtils.GetLabelForUnit(dut)}. Please make sure the converted values are correct.");
                return dut;
            }
            else
            {
                BH.Engine.Base.Compute.RecordError($"Unit type {LabelUtils.GetLabelForSpec(quantity)} has not been recognized and has not been converted - as a result, the output value can be wrong.");
                return UnitTypeId.General;
            }
        }


        /***************************************************/
        /****            Private collections            ****/
        /***************************************************/

        private static readonly ForgeTypeId[] BHoMUnitsNew = new ForgeTypeId[]
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
            UnitTypeId.NewtonsPerSquareMeter,
            UnitTypeId.General
        };

        /***************************************************/

        private static readonly Dictionary<ForgeTypeId, ForgeTypeId> BHoMEquivalentsNew = new Dictionary<ForgeTypeId, ForgeTypeId>
        {
            { UnitTypeId.Currency,  UnitTypeId.General },
            { UnitTypeId.Kelvin,  UnitTypeId.Celsius },
            { UnitTypeId.Percentage,  UnitTypeId.Fixed },
            { UnitTypeId.SquareMetersPerMeter,  UnitTypeId.Meters },
            { UnitTypeId.Millimeters,  UnitTypeId.Meters }
        };

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
            "horsepower",
            "british"
        };

        /***************************************************/
    }
}






