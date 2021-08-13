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
using System.ComponentModel;

#if (REVIT2018 || REVIT2019 || REVIT2020)
namespace BH.Revit.Engine.Core
{
    [Description("A special-purpose class mimicking the Revit API class introduced in 2021 version, which contains the properties that return the equivalents of DisplayUnitType.\n" +
                 "It has been implemented to minimise the fallout caused by the API change on the existing code base of Revit_Toolkit.")]
    public static class UnitTypeId
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public static DisplayUnitType Meters { get { return DisplayUnitType.DUT_METERS; } }
        public static DisplayUnitType Centimeters { get { return DisplayUnitType.DUT_CENTIMETERS; } }
        public static DisplayUnitType Millimeters { get { return DisplayUnitType.DUT_MILLIMETERS; } }
        public static DisplayUnitType Feet { get { return DisplayUnitType.DUT_DECIMAL_FEET; } }
        public static DisplayUnitType FeetFractionalInches { get { return DisplayUnitType.DUT_FEET_FRACTIONAL_INCHES; } }
        public static DisplayUnitType FractionalInches { get { return DisplayUnitType.DUT_FRACTIONAL_INCHES; } }
        public static DisplayUnitType Inches { get { return DisplayUnitType.DUT_DECIMAL_INCHES; } }
        public static DisplayUnitType Acres { get { return DisplayUnitType.DUT_ACRES; } }
        public static DisplayUnitType Hectares { get { return DisplayUnitType.DUT_HECTARES; } }
        public static DisplayUnitType MetersCentimeters { get { return DisplayUnitType.DUT_METERS_CENTIMETERS; } }
        public static DisplayUnitType CubicYards { get { return DisplayUnitType.DUT_CUBIC_YARDS; } }
        public static DisplayUnitType SquareFeet { get { return DisplayUnitType.DUT_SQUARE_FEET; } }
        public static DisplayUnitType SquareMeters { get { return DisplayUnitType.DUT_SQUARE_METERS; } }
        public static DisplayUnitType CubicFeet { get { return DisplayUnitType.DUT_CUBIC_FEET; } }
        public static DisplayUnitType CubicMeters { get { return DisplayUnitType.DUT_CUBIC_METERS; } }
        public static DisplayUnitType Degrees { get { return DisplayUnitType.DUT_DECIMAL_DEGREES; } }
        public static DisplayUnitType DegreesMinutes { get { return DisplayUnitType.DUT_DEGREES_AND_MINUTES; } }
        public static DisplayUnitType General { get { return DisplayUnitType.DUT_GENERAL; } }
        public static DisplayUnitType Fixed { get { return DisplayUnitType.DUT_FIXED; } }
        public static DisplayUnitType Percentage { get { return DisplayUnitType.DUT_PERCENTAGE; } }
        public static DisplayUnitType SquareInches { get { return DisplayUnitType.DUT_SQUARE_INCHES; } }
        public static DisplayUnitType SquareCentimeters { get { return DisplayUnitType.DUT_SQUARE_CENTIMETERS; } }
        public static DisplayUnitType SquareMillimeters { get { return DisplayUnitType.DUT_SQUARE_MILLIMETERS; } }
        public static DisplayUnitType CubicInches { get { return DisplayUnitType.DUT_CUBIC_INCHES; } }
        public static DisplayUnitType CubicCentimeters { get { return DisplayUnitType.DUT_CUBIC_CENTIMETERS; } }
        public static DisplayUnitType CubicMillimeters { get { return DisplayUnitType.DUT_CUBIC_MILLIMETERS; } }
        public static DisplayUnitType Liters { get { return DisplayUnitType.DUT_LITERS; } }
        public static DisplayUnitType UsGallons { get { return DisplayUnitType.DUT_GALLONS_US; } }
        public static DisplayUnitType KilogramsPerCubicMeter { get { return DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER; } }
        public static DisplayUnitType PoundsMassPerCubicFoot { get { return DisplayUnitType.DUT_POUNDS_MASS_PER_CUBIC_FOOT; } }
        public static DisplayUnitType PoundsMassPerCubicInch { get { return DisplayUnitType.DUT_POUNDS_MASS_PER_CUBIC_INCH; } }
        public static DisplayUnitType BritishThermalUnits { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS; } }
        public static DisplayUnitType Calories { get { return DisplayUnitType.DUT_CALORIES; } }
        public static DisplayUnitType Kilocalories { get { return DisplayUnitType.DUT_KILOCALORIES; } }
        public static DisplayUnitType Joules { get { return DisplayUnitType.DUT_JOULES; } }
        public static DisplayUnitType KilowattHours { get { return DisplayUnitType.DUT_KILOWATT_HOURS; } }
        public static DisplayUnitType Therms { get { return DisplayUnitType.DUT_THERMS; } }
        public static DisplayUnitType InchesOfWater60DegreesFahrenheitPer100Feet { get { return DisplayUnitType.DUT_INCHES_OF_WATER_PER_100FT; } }
        public static DisplayUnitType PascalsPerMeter { get { return DisplayUnitType.DUT_PASCALS_PER_METER; } }
        public static DisplayUnitType Watts { get { return DisplayUnitType.DUT_WATTS; } }
        public static DisplayUnitType Kilowatts { get { return DisplayUnitType.DUT_KILOWATTS; } }
        public static DisplayUnitType BritishThermalUnitsPerSecond { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_SECOND; } }
        public static DisplayUnitType BritishThermalUnitsPerHour { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_HOUR; } }
        public static DisplayUnitType CaloriesPerSecond { get { return DisplayUnitType.DUT_CALORIES_PER_SECOND; } }
        public static DisplayUnitType KilocaloriesPerSecond { get { return DisplayUnitType.DUT_KILOCALORIES_PER_SECOND; } }
        public static DisplayUnitType WattsPerSquareFoot { get { return DisplayUnitType.DUT_WATTS_PER_SQUARE_FOOT; } }
        public static DisplayUnitType WattsPerSquareMeter { get { return DisplayUnitType.DUT_WATTS_PER_SQUARE_METER; } }
        public static DisplayUnitType InchesOfWater60DegreesFahrenheit { get { return DisplayUnitType.DUT_INCHES_OF_WATER; } }
        public static DisplayUnitType Pascals { get { return DisplayUnitType.DUT_PASCALS; } }
        public static DisplayUnitType Kilopascals { get { return DisplayUnitType.DUT_KILOPASCALS; } }
        public static DisplayUnitType Megapascals { get { return DisplayUnitType.DUT_MEGAPASCALS; } }
        public static DisplayUnitType PoundsForcePerSquareInch { get { return DisplayUnitType.DUT_POUNDS_FORCE_PER_SQUARE_INCH; } }
        public static DisplayUnitType InchesOfMercury32DegreesFahrenheit { get { return DisplayUnitType.DUT_INCHES_OF_MERCURY; } }
        public static DisplayUnitType MillimetersOfMercury { get { return DisplayUnitType.DUT_MILLIMETERS_OF_MERCURY; } }
        public static DisplayUnitType Atmospheres { get { return DisplayUnitType.DUT_ATMOSPHERES; } }
        public static DisplayUnitType Bars { get { return DisplayUnitType.DUT_BARS; } }
        public static DisplayUnitType Fahrenheit { get { return DisplayUnitType.DUT_FAHRENHEIT; } }
        public static DisplayUnitType Celsius { get { return DisplayUnitType.DUT_CELSIUS; } }
        public static DisplayUnitType Kelvin { get { return DisplayUnitType.DUT_KELVIN; } }
        public static DisplayUnitType Rankine { get { return DisplayUnitType.DUT_RANKINE; } }
        public static DisplayUnitType FeetPerMinute { get { return DisplayUnitType.DUT_FEET_PER_MINUTE; } }
        public static DisplayUnitType MetersPerSecond { get { return DisplayUnitType.DUT_METERS_PER_SECOND; } }
        public static DisplayUnitType CentimetersPerMinute { get { return DisplayUnitType.DUT_CENTIMETERS_PER_MINUTE; } }
        public static DisplayUnitType CubicFeetPerMinute { get { return DisplayUnitType.DUT_CUBIC_FEET_PER_MINUTE; } }
        public static DisplayUnitType LitersPerSecond { get { return DisplayUnitType.DUT_LITERS_PER_SECOND; } }
        public static DisplayUnitType CubicMetersPerSecond { get { return DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND; } }
        public static DisplayUnitType CubicMetersPerHour { get { return DisplayUnitType.DUT_CUBIC_METERS_PER_HOUR; } }
        public static DisplayUnitType UsGallonsPerMinute { get { return DisplayUnitType.DUT_GALLONS_US_PER_MINUTE; } }
        public static DisplayUnitType UsGallonsPerHour { get { return DisplayUnitType.DUT_GALLONS_US_PER_HOUR; } }
        public static DisplayUnitType Amperes { get { return DisplayUnitType.DUT_AMPERES; } }
        public static DisplayUnitType Kiloamperes { get { return DisplayUnitType.DUT_KILOAMPERES; } }
        public static DisplayUnitType Milliamperes { get { return DisplayUnitType.DUT_MILLIAMPERES; } }
        public static DisplayUnitType Volts { get { return DisplayUnitType.DUT_VOLTS; } }
        public static DisplayUnitType Kilovolts { get { return DisplayUnitType.DUT_KILOVOLTS; } }
        public static DisplayUnitType Millivolts { get { return DisplayUnitType.DUT_MILLIVOLTS; } }
        public static DisplayUnitType Hertz { get { return DisplayUnitType.DUT_HERTZ; } }
        public static DisplayUnitType CyclesPerSecond { get { return DisplayUnitType.DUT_CYCLES_PER_SECOND; } }
        public static DisplayUnitType Lux { get { return DisplayUnitType.DUT_LUX; } }
        public static DisplayUnitType Footcandles { get { return DisplayUnitType.DUT_FOOTCANDLES; } }
        public static DisplayUnitType Footlamberts { get { return DisplayUnitType.DUT_FOOTLAMBERTS; } }
        public static DisplayUnitType CandelasPerSquareMeter { get { return DisplayUnitType.DUT_CANDELAS_PER_SQUARE_METER; } }
        public static DisplayUnitType Candelas { get { return DisplayUnitType.DUT_CANDELAS; } }
        public static DisplayUnitType Lumens { get { return DisplayUnitType.DUT_LUMENS; } }
        public static DisplayUnitType VoltAmperes { get { return DisplayUnitType.DUT_VOLT_AMPERES; } }
        public static DisplayUnitType KilovoltAmperes { get { return DisplayUnitType.DUT_KILOVOLT_AMPERES; } }
        public static DisplayUnitType Horsepower { get { return DisplayUnitType.DUT_HORSEPOWER; } }
        public static DisplayUnitType Newtons { get { return DisplayUnitType.DUT_NEWTONS; } }
        public static DisplayUnitType Dekanewtons { get { return DisplayUnitType.DUT_DECANEWTONS; } }
        public static DisplayUnitType Kilonewtons { get { return DisplayUnitType.DUT_KILONEWTONS; } }
        public static DisplayUnitType Meganewtons { get { return DisplayUnitType.DUT_MEGANEWTONS; } }
        public static DisplayUnitType Kips { get { return DisplayUnitType.DUT_KIPS; } }
        public static DisplayUnitType KilogramsForce { get { return DisplayUnitType.DUT_KILOGRAMS_FORCE; } }
        public static DisplayUnitType TonnesForce { get { return DisplayUnitType.DUT_TONNES_FORCE; } }
        public static DisplayUnitType PoundsForce { get { return DisplayUnitType.DUT_POUNDS_FORCE; } }
        public static DisplayUnitType NewtonsPerMeter { get { return DisplayUnitType.DUT_NEWTONS_PER_METER; } }
        public static DisplayUnitType DekanewtonsPerMeter { get { return DisplayUnitType.DUT_DECANEWTONS_PER_METER; } }
        public static DisplayUnitType KilonewtonsPerMeter { get { return DisplayUnitType.DUT_KILONEWTONS_PER_METER; } }
        public static DisplayUnitType MeganewtonsPerMeter { get { return DisplayUnitType.DUT_MEGANEWTONS_PER_METER; } }
        public static DisplayUnitType KipsPerFoot { get { return DisplayUnitType.DUT_KIPS_PER_FOOT; } }
        public static DisplayUnitType KilogramsForcePerMeter { get { return DisplayUnitType.DUT_KILOGRAMS_FORCE_PER_METER; } }
        public static DisplayUnitType TonnesForcePerMeter { get { return DisplayUnitType.DUT_TONNES_FORCE_PER_METER; } }
        public static DisplayUnitType PoundsForcePerFoot { get { return DisplayUnitType.DUT_POUNDS_FORCE_PER_FOOT; } }
        public static DisplayUnitType NewtonsPerSquareMeter { get { return DisplayUnitType.DUT_NEWTONS_PER_SQUARE_METER; } }
        public static DisplayUnitType DekanewtonsPerSquareMeter { get { return DisplayUnitType.DUT_DECANEWTONS_PER_SQUARE_METER; } }
        public static DisplayUnitType KilonewtonsPerSquareMeter { get { return DisplayUnitType.DUT_KILONEWTONS_PER_SQUARE_METER; } }
        public static DisplayUnitType MeganewtonsPerSquareMeter { get { return DisplayUnitType.DUT_MEGANEWTONS_PER_SQUARE_METER; } }
        public static DisplayUnitType KipsPerSquareFoot { get { return DisplayUnitType.DUT_KIPS_PER_SQUARE_FOOT; } }
        public static DisplayUnitType KilogramsForcePerSquareMeter { get { return DisplayUnitType.DUT_KILOGRAMS_FORCE_PER_SQUARE_METER; } }
        public static DisplayUnitType TonnesForcePerSquareMeter { get { return DisplayUnitType.DUT_TONNES_FORCE_PER_SQUARE_METER; } }
        public static DisplayUnitType PoundsForcePerSquareFoot { get { return DisplayUnitType.DUT_POUNDS_FORCE_PER_SQUARE_FOOT; } }
        public static DisplayUnitType NewtonMeters { get { return DisplayUnitType.DUT_NEWTON_METERS; } }
        public static DisplayUnitType DekanewtonMeters { get { return DisplayUnitType.DUT_DECANEWTON_METERS; } }
        public static DisplayUnitType KilonewtonMeters { get { return DisplayUnitType.DUT_KILONEWTON_METERS; } }
        public static DisplayUnitType MeganewtonMeters { get { return DisplayUnitType.DUT_MEGANEWTON_METERS; } }
        public static DisplayUnitType KipFeet { get { return DisplayUnitType.DUT_KIP_FEET; } }
        public static DisplayUnitType KilogramForceMeters { get { return DisplayUnitType.DUT_KILOGRAM_FORCE_METERS; } }
        public static DisplayUnitType TonneForceMeters { get { return DisplayUnitType.DUT_TONNE_FORCE_METERS; } }
        public static DisplayUnitType PoundForceFeet { get { return DisplayUnitType.DUT_POUND_FORCE_FEET; } }
        public static DisplayUnitType MetersPerKilonewton { get { return DisplayUnitType.DUT_METERS_PER_KILONEWTON; } }
        public static DisplayUnitType FeetPerKip { get { return DisplayUnitType.DUT_FEET_PER_KIP; } }
        public static DisplayUnitType SquareMetersPerKilonewton { get { return DisplayUnitType.DUT_SQUARE_METERS_PER_KILONEWTON; } }
        public static DisplayUnitType SquareFeetPerKip { get { return DisplayUnitType.DUT_SQUARE_FEET_PER_KIP; } }
        public static DisplayUnitType CubicMetersPerKilonewton { get { return DisplayUnitType.DUT_CUBIC_METERS_PER_KILONEWTON; } }
        public static DisplayUnitType CubicFeetPerKip { get { return DisplayUnitType.DUT_CUBIC_FEET_PER_KIP; } }
        public static DisplayUnitType InverseKilonewtons { get { return DisplayUnitType.DUT_INV_KILONEWTONS; } }
        public static DisplayUnitType InverseKips { get { return DisplayUnitType.DUT_INV_KIPS; } }
        public static DisplayUnitType FeetOfWater39_2DegreesFahrenheitPer100Feet { get { return DisplayUnitType.DUT_FEET_OF_WATER_PER_100FT; } }
        public static DisplayUnitType FeetOfWater39_2DegreesFahrenheit { get { return DisplayUnitType.DUT_FEET_OF_WATER; } }
        public static DisplayUnitType PascalSeconds { get { return DisplayUnitType.DUT_PASCAL_SECONDS; } }
        public static DisplayUnitType PoundsMassPerFootSecond { get { return DisplayUnitType.DUT_POUNDS_MASS_PER_FOOT_SECOND; } }
        public static DisplayUnitType Centipoises { get { return DisplayUnitType.DUT_CENTIPOISES; } }
        public static DisplayUnitType FeetPerSecond { get { return DisplayUnitType.DUT_FEET_PER_SECOND; } }
        public static DisplayUnitType KipsPerSquareInch { get { return DisplayUnitType.DUT_KIPS_PER_SQUARE_INCH; } }
        public static DisplayUnitType KilonewtonsPerCubicMeter { get { return DisplayUnitType.DUT_KILONEWTONS_PER_CUBIC_METER; } }
        public static DisplayUnitType PoundsForcePerCubicFoot { get { return DisplayUnitType.DUT_POUNDS_FORCE_PER_CUBIC_FOOT; } }
        public static DisplayUnitType KipsPerCubicInch { get { return DisplayUnitType.DUT_KIPS_PER_CUBIC_INCH; } }
        public static DisplayUnitType InverseDegreesFahrenheit { get { return DisplayUnitType.DUT_INV_FAHRENHEIT; } }
        public static DisplayUnitType InverseDegreesCelsius { get { return DisplayUnitType.DUT_INV_CELSIUS; } }
        public static DisplayUnitType NewtonMetersPerMeter { get { return DisplayUnitType.DUT_NEWTON_METERS_PER_METER; } }
        public static DisplayUnitType DekanewtonMetersPerMeter { get { return DisplayUnitType.DUT_DECANEWTON_METERS_PER_METER; } }
        public static DisplayUnitType KilonewtonMetersPerMeter { get { return DisplayUnitType.DUT_KILONEWTON_METERS_PER_METER; } }
        public static DisplayUnitType MeganewtonMetersPerMeter { get { return DisplayUnitType.DUT_MEGANEWTON_METERS_PER_METER; } }
        public static DisplayUnitType KipFeetPerFoot { get { return DisplayUnitType.DUT_KIP_FEET_PER_FOOT; } }
        public static DisplayUnitType KilogramForceMetersPerMeter { get { return DisplayUnitType.DUT_KILOGRAM_FORCE_METERS_PER_METER; } }
        public static DisplayUnitType TonneForceMetersPerMeter { get { return DisplayUnitType.DUT_TONNE_FORCE_METERS_PER_METER; } }
        public static DisplayUnitType PoundForceFeetPerFoot { get { return DisplayUnitType.DUT_POUND_FORCE_FEET_PER_FOOT; } }
        public static DisplayUnitType PoundsMassPerFootHour { get { return DisplayUnitType.DUT_POUNDS_MASS_PER_FOOT_HOUR; } }
        public static DisplayUnitType KipsPerInch { get { return DisplayUnitType.DUT_KIPS_PER_INCH; } }
        public static DisplayUnitType KipsPerCubicFoot { get { return DisplayUnitType.DUT_KIPS_PER_CUBIC_FOOT; } }
        public static DisplayUnitType KipFeetPerDegree { get { return DisplayUnitType.DUT_KIP_FEET_PER_DEGREE; } }
        public static DisplayUnitType KilonewtonMetersPerDegree { get { return DisplayUnitType.DUT_KILONEWTON_METERS_PER_DEGREE; } }
        public static DisplayUnitType KipFeetPerDegreePerFoot { get { return DisplayUnitType.DUT_KIP_FEET_PER_DEGREE_PER_FOOT; } }
        public static DisplayUnitType KilonewtonMetersPerDegreePerMeter { get { return DisplayUnitType.DUT_KILONEWTON_METERS_PER_DEGREE_PER_METER; } }
        public static DisplayUnitType WattsPerSquareMeterKelvin { get { return DisplayUnitType.DUT_WATTS_PER_SQUARE_METER_KELVIN; } }
        public static DisplayUnitType BritishThermalUnitsPerHourSquareFootDegreeFahrenheit { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_HOUR_SQUARE_FOOT_FAHRENHEIT; } }
        public static DisplayUnitType CubicFeetPerMinuteSquareFoot { get { return DisplayUnitType.DUT_CUBIC_FEET_PER_MINUTE_SQUARE_FOOT; } }
        public static DisplayUnitType LitersPerSecondSquareMeter { get { return DisplayUnitType.DUT_LITERS_PER_SECOND_SQUARE_METER; } }
        public static DisplayUnitType RatioTo10 { get { return DisplayUnitType.DUT_RATIO_10; } }
        public static DisplayUnitType RatioTo12 { get { return DisplayUnitType.DUT_RATIO_12; } }
        public static DisplayUnitType SlopeDegrees { get { return DisplayUnitType.DUT_SLOPE_DEGREES; } }
        public static DisplayUnitType RiseDividedBy12Inches { get { return DisplayUnitType.DUT_RISE_OVER_INCHES; } }
        public static DisplayUnitType RiseDividedBy1Foot { get { return DisplayUnitType.DUT_RISE_OVER_FOOT; } }
        public static DisplayUnitType RiseDividedBy1000Millimeters { get { return DisplayUnitType.DUT_RISE_OVER_MMS; } }
        public static DisplayUnitType WattsPerCubicFoot { get { return DisplayUnitType.DUT_WATTS_PER_CUBIC_FOOT; } }
        public static DisplayUnitType WattsPerCubicMeter { get { return DisplayUnitType.DUT_WATTS_PER_CUBIC_METER; } }
        public static DisplayUnitType BritishThermalUnitsPerHourSquareFoot { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_HOUR_SQUARE_FOOT; } }
        public static DisplayUnitType BritishThermalUnitsPerHourCubicFoot { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_HOUR_CUBIC_FOOT; } }
        public static DisplayUnitType TonsOfRefrigeration { get { return DisplayUnitType.DUT_TON_OF_REFRIGERATION; } }
        public static DisplayUnitType CubicFeetPerMinuteCubicFoot { get { return DisplayUnitType.DUT_CUBIC_FEET_PER_MINUTE_CUBIC_FOOT; } }
        public static DisplayUnitType LitersPerSecondCubicMeter { get { return DisplayUnitType.DUT_LITERS_PER_SECOND_CUBIC_METER; } }
        public static DisplayUnitType CubicFeetPerMinuteTonOfRefrigeration { get { return DisplayUnitType.DUT_CUBIC_FEET_PER_MINUTE_TON_OF_REFRIGERATION; } }
        public static DisplayUnitType LitersPerSecondKilowatt { get { return DisplayUnitType.DUT_LITERS_PER_SECOND_KILOWATTS; } }
        public static DisplayUnitType SquareFeetPerTonOfRefrigeration { get { return DisplayUnitType.DUT_SQUARE_FEET_PER_TON_OF_REFRIGERATION; } }
        public static DisplayUnitType SquareMetersPerKilowatt { get { return DisplayUnitType.DUT_SQUARE_METERS_PER_KILOWATTS; } }
        public static DisplayUnitType Currency { get { return DisplayUnitType.DUT_CURRENCY; } }
        public static DisplayUnitType LumensPerWatt { get { return DisplayUnitType.DUT_LUMENS_PER_WATT; } }
        public static DisplayUnitType SquareFeetPer1000BritishThermalUnitsPerHour { get { return DisplayUnitType.DUT_SQUARE_FEET_PER_THOUSAND_BRITISH_THERMAL_UNITS_PER_HOUR; } }
        public static DisplayUnitType KilonewtonsPerSquareCentimeter { get { return DisplayUnitType.DUT_KILONEWTONS_PER_SQUARE_CENTIMETER; } }
        public static DisplayUnitType NewtonsPerSquareMillimeter { get { return DisplayUnitType.DUT_NEWTONS_PER_SQUARE_MILLIMETER; } }
        public static DisplayUnitType KilonewtonsPerSquareMillimeter { get { return DisplayUnitType.DUT_KILONEWTONS_PER_SQUARE_MILLIMETER; } }
        public static DisplayUnitType RiseDividedBy120Inches { get { return DisplayUnitType.DUT_RISE_OVER_120_INCHES; } }
        public static DisplayUnitType OneToRatio { get { return DisplayUnitType.DUT_1_RATIO; } }
        public static DisplayUnitType RiseDividedBy10Feet { get { return DisplayUnitType.DUT_RISE_OVER_10_FEET; } }
        public static DisplayUnitType HourSquareFootDegreesFahrenheitPerBritishThermalUnit { get { return DisplayUnitType.DUT_HOUR_SQUARE_FOOT_FAHRENHEIT_PER_BRITISH_THERMAL_UNIT; } }
        public static DisplayUnitType SquareMeterKelvinsPerWatt { get { return DisplayUnitType.DUT_SQUARE_METER_KELVIN_PER_WATT; } }
        public static DisplayUnitType BritishThermalUnitsPerDegreeFahrenheit { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNIT_PER_FAHRENHEIT; } }
        public static DisplayUnitType JoulesPerKelvin { get { return DisplayUnitType.DUT_JOULES_PER_KELVIN; } }
        public static DisplayUnitType KilojoulesPerKelvin { get { return DisplayUnitType.DUT_KILOJOULES_PER_KELVIN; } }
        public static DisplayUnitType Kilograms { get { return DisplayUnitType.DUT_KILOGRAMS_MASS; } }
        public static DisplayUnitType Tonnes { get { return DisplayUnitType.DUT_TONNES_MASS; } }
        public static DisplayUnitType PoundsMass { get { return DisplayUnitType.DUT_POUNDS_MASS; } }
        public static DisplayUnitType MetersPerSecondSquared { get { return DisplayUnitType.DUT_METERS_PER_SECOND_SQUARED; } }
        public static DisplayUnitType KilometersPerSecondSquared { get { return DisplayUnitType.DUT_KILOMETERS_PER_SECOND_SQUARED; } }
        public static DisplayUnitType InchesPerSecondSquared { get { return DisplayUnitType.DUT_INCHES_PER_SECOND_SQUARED; } }
        public static DisplayUnitType FeetPerSecondSquared { get { return DisplayUnitType.DUT_FEET_PER_SECOND_SQUARED; } }
        public static DisplayUnitType MilesPerSecondSquared { get { return DisplayUnitType.DUT_MILES_PER_SECOND_SQUARED; } }
        public static DisplayUnitType FeetToTheFourthPower { get { return DisplayUnitType.DUT_FEET_TO_THE_FOURTH_POWER; } }
        public static DisplayUnitType InchesToTheFourthPower { get { return DisplayUnitType.DUT_INCHES_TO_THE_FOURTH_POWER; } }
        public static DisplayUnitType MillimetersToTheFourthPower { get { return DisplayUnitType.DUT_MILLIMETERS_TO_THE_FOURTH_POWER; } }
        public static DisplayUnitType CentimetersToTheFourthPower { get { return DisplayUnitType.DUT_CENTIMETERS_TO_THE_FOURTH_POWER; } }
        public static DisplayUnitType MetersToTheFourthPower { get { return DisplayUnitType.DUT_METERS_TO_THE_FOURTH_POWER; } }
        public static DisplayUnitType FeetToTheSixthPower { get { return DisplayUnitType.DUT_FEET_TO_THE_SIXTH_POWER; } }
        public static DisplayUnitType InchesToTheSixthPower { get { return DisplayUnitType.DUT_INCHES_TO_THE_SIXTH_POWER; } }
        public static DisplayUnitType MillimetersToTheSixthPower { get { return DisplayUnitType.DUT_MILLIMETERS_TO_THE_SIXTH_POWER; } }
        public static DisplayUnitType CentimetersToTheSixthPower { get { return DisplayUnitType.DUT_CENTIMETERS_TO_THE_SIXTH_POWER; } }
        public static DisplayUnitType MetersToTheSixthPower { get { return DisplayUnitType.DUT_METERS_TO_THE_SIXTH_POWER; } }
        public static DisplayUnitType SquareFeetPerFoot { get { return DisplayUnitType.DUT_SQUARE_FEET_PER_FOOT; } }
        public static DisplayUnitType SquareInchesPerFoot { get { return DisplayUnitType.DUT_SQUARE_INCHES_PER_FOOT; } }
        public static DisplayUnitType SquareMillimetersPerMeter { get { return DisplayUnitType.DUT_SQUARE_MILLIMETERS_PER_METER; } }
        public static DisplayUnitType SquareCentimetersPerMeter { get { return DisplayUnitType.DUT_SQUARE_CENTIMETERS_PER_METER; } }
        public static DisplayUnitType SquareMetersPerMeter { get { return DisplayUnitType.DUT_SQUARE_METERS_PER_METER; } }
        public static DisplayUnitType KilogramsPerMeter { get { return DisplayUnitType.DUT_KILOGRAMS_MASS_PER_METER; } }
        public static DisplayUnitType PoundsMassPerFoot { get { return DisplayUnitType.DUT_POUNDS_MASS_PER_FOOT; } }
        public static DisplayUnitType Radians { get { return DisplayUnitType.DUT_RADIANS; } }
        public static DisplayUnitType Gradians { get { return DisplayUnitType.DUT_GRADS; } }
        public static DisplayUnitType RadiansPerSecond { get { return DisplayUnitType.DUT_RADIANS_PER_SECOND; } }
        public static DisplayUnitType Milliseconds { get { return DisplayUnitType.DUT_MILISECONDS; } }
        public static DisplayUnitType Seconds { get { return DisplayUnitType.DUT_SECONDS; } }
        public static DisplayUnitType Minutes { get { return DisplayUnitType.DUT_MINUTES; } }
        public static DisplayUnitType Hours { get { return DisplayUnitType.DUT_HOURS; } }
        public static DisplayUnitType KilometersPerHour { get { return DisplayUnitType.DUT_KILOMETERS_PER_HOUR; } }
        public static DisplayUnitType MilesPerHour { get { return DisplayUnitType.DUT_MILES_PER_HOUR; } }
        public static DisplayUnitType Kilojoules { get { return DisplayUnitType.DUT_KILOJOULES; } }
        public static DisplayUnitType KilogramsPerSquareMeter { get { return DisplayUnitType.DUT_KILOGRAMS_MASS_PER_SQUARE_METER; } }
        public static DisplayUnitType PoundsMassPerSquareFoot { get { return DisplayUnitType.DUT_POUNDS_MASS_PER_SQUARE_FOOT; } }
        public static DisplayUnitType WattsPerMeterKelvin { get { return DisplayUnitType.DUT_WATTS_PER_METER_KELVIN; } }
        public static DisplayUnitType JoulesPerGramDegreeCelsius { get { return DisplayUnitType.DUT_JOULES_PER_GRAM_CELSIUS; } }
        public static DisplayUnitType JoulesPerGram { get { return DisplayUnitType.DUT_JOULES_PER_GRAM; } }
        public static DisplayUnitType NanogramsPerPascalSecondSquareMeter { get { return DisplayUnitType.DUT_NANOGRAMS_PER_PASCAL_SECOND_SQUARE_METER; } }
        public static DisplayUnitType OhmMeters { get { return DisplayUnitType.DUT_OHM_METERS; } }
        public static DisplayUnitType BritishThermalUnitsPerHourFootDegreeFahrenheit { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_HOUR_FOOT_FAHRENHEIT; } }
        public static DisplayUnitType BritishThermalUnitsPerPoundDegreeFahrenheit { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_POUND_FAHRENHEIT; } }
        public static DisplayUnitType BritishThermalUnitsPerPound { get { return DisplayUnitType.DUT_BRITISH_THERMAL_UNITS_PER_POUND; } }
        public static DisplayUnitType GrainsPerHourSquareFootInchMercury { get { return DisplayUnitType.DUT_GRAINS_PER_HOUR_SQUARE_FOOT_INCH_MERCURY; } }
        public static DisplayUnitType PerMille { get { return DisplayUnitType.DUT_PER_MILLE; } }
        public static DisplayUnitType Decimeters { get { return DisplayUnitType.DUT_DECIMETERS; } }
        public static DisplayUnitType JoulesPerKilogramDegreeCelsius { get { return DisplayUnitType.DUT_JOULES_PER_KILOGRAM_CELSIUS; } }
        public static DisplayUnitType MicrometersPerMeterDegreeCelsius { get { return DisplayUnitType.DUT_MICROMETERS_PER_METER_CELSIUS; } }
        public static DisplayUnitType MicroinchesPerInchDegreeFahrenheit { get { return DisplayUnitType.DUT_MICROINCHES_PER_INCH_FAHRENHEIT; } }
        public static DisplayUnitType UsTonnesMass { get { return DisplayUnitType.DUT_USTONNES_MASS; } }
        public static DisplayUnitType UsTonnesForce { get { return DisplayUnitType.DUT_USTONNES_FORCE; } }
        public static DisplayUnitType LitersPerMinute { get { return DisplayUnitType.DUT_LITERS_PER_MINUTE; } }
        public static DisplayUnitType FahrenheitInterval { get { return DisplayUnitType.DUT_FAHRENHEIT_DIFFERENCE; } }
        public static DisplayUnitType CelsiusInterval { get { return DisplayUnitType.DUT_CELSIUS_DIFFERENCE; } }
        public static DisplayUnitType KelvinInterval { get { return DisplayUnitType.DUT_KELVIN_DIFFERENCE; } }
        public static DisplayUnitType RankineInterval { get { return DisplayUnitType.DUT_RANKINE_DIFFERENCE; } }
        public static DisplayUnitType Custom { get { return DisplayUnitType.DUT_CUSTOM; } }
        
        public static DisplayUnitType StationingMeters { get { return NonExistent(nameof(StationingMeters), 2021); } }
        public static DisplayUnitType StationingFeet { get { return NonExistent(nameof(StationingFeet), 2021); } }
        public static DisplayUnitType CubicFeetPerHour { get { return NonExistent(nameof(CubicFeetPerHour), 2021); } }
        public static DisplayUnitType LitersPerHour { get { return NonExistent(nameof(LitersPerHour), 2021); } }
        public static DisplayUnitType RatioTo1 { get { return NonExistent(nameof(RatioTo1), 2021); } }
        public static DisplayUnitType UsSurveyFeet { get { return NonExistent(nameof(UsSurveyFeet), 2021); } }
        public static DisplayUnitType WattsPerMeter { get { return NonExistent(nameof(WattsPerMeter), 2021); } }
        public static DisplayUnitType WattsPerFoot { get { return NonExistent(nameof(WattsPerFoot), 2021); } }
        public static DisplayUnitType WattsPerCubicMeterPerSecond { get { return NonExistent(nameof(WattsPerCubicMeterPerSecond), 2021); } }
        public static DisplayUnitType WattsPerCubicFootPerMinute { get { return NonExistent(nameof(WattsPerCubicFootPerMinute), 2021); } }
        public static DisplayUnitType ThousandBritishThermalUnitsPerHour { get { return NonExistent(nameof(ThousandBritishThermalUnitsPerHour), 2021); } }
        public static DisplayUnitType SquareMetersPerSecond { get { return NonExistent(nameof(SquareMetersPerSecond), 2021); } }
        public static DisplayUnitType SquareFeetPerSecond { get { return NonExistent(nameof(SquareFeetPerSecond), 2021); } }
        public static DisplayUnitType RevolutionsPerSecond { get { return NonExistent(nameof(RevolutionsPerSecond), 2021); } }
        public static DisplayUnitType RevolutionsPerMinute { get { return NonExistent(nameof(RevolutionsPerMinute), 2021); } }
        public static DisplayUnitType PoundsMassPerSecond { get { return NonExistent(nameof(PoundsMassPerSecond), 2021); } }
        public static DisplayUnitType PoundsMassPerPoundDegreeFahrenheit { get { return NonExistent(nameof(PoundsMassPerPoundDegreeFahrenheit), 2021); } }
        public static DisplayUnitType PoundsMassPerMinute { get { return NonExistent(nameof(PoundsMassPerMinute), 2021); } }
        public static DisplayUnitType PoundsMassPerHour { get { return NonExistent(nameof(PoundsMassPerHour), 2021); } }
        public static DisplayUnitType PoundForceSecondsPerSquareFoot { get { return NonExistent(nameof(PoundForceSecondsPerSquareFoot), 2021); } }
        public static DisplayUnitType Pi { get { return NonExistent(nameof(Pi), 2021); } }
        public static DisplayUnitType NewtonSecondsPerSquareMeter { get { return NonExistent(nameof(NewtonSecondsPerSquareMeter), 2021); } }
        public static DisplayUnitType MillimetersOfWaterColumnPerMeter { get { return NonExistent(nameof(MillimetersOfWaterColumnPerMeter), 2021); } }
        public static DisplayUnitType MillimetersOfWaterColumn { get { return NonExistent(nameof(MillimetersOfWaterColumn), 2021); } }
        public static DisplayUnitType MetersOfWaterColumnPerMeter { get { return NonExistent(nameof(MetersOfWaterColumnPerMeter), 2021); } }
        public static DisplayUnitType MetersOfWaterColumn { get { return NonExistent(nameof(MetersOfWaterColumn), 2021); } }
        public static DisplayUnitType KilogramsPerSecond { get { return NonExistent(nameof(KilogramsPerSecond), 2021); } }
        public static DisplayUnitType KilogramsPerMinute { get { return NonExistent(nameof(KilogramsPerMinute), 2021); } }
        public static DisplayUnitType KilogramsPerMeterSecond { get { return NonExistent(nameof(KilogramsPerMeterSecond), 2021); } }
        public static DisplayUnitType KilogramsPerMeterHour { get { return NonExistent(nameof(KilogramsPerMeterHour), 2021); } }
        public static DisplayUnitType KilogramsPerKilogramKelvin { get { return NonExistent(nameof(KilogramsPerKilogramKelvin), 2021); } }
        public static DisplayUnitType KilogramsPerHour { get { return NonExistent(nameof(KilogramsPerHour), 2021); } }
        public static DisplayUnitType CurrencyPerWattHour { get { return NonExistent(nameof(CurrencyPerWattHour), 2021); } }
        public static DisplayUnitType CurrencyPerWatt { get { return NonExistent(nameof(CurrencyPerWatt), 2021); } }
        public static DisplayUnitType CurrencyPerSquareMeter { get { return NonExistent(nameof(CurrencyPerSquareMeter), 2021); } }
        public static DisplayUnitType CurrencyPerSquareFoot { get { return NonExistent(nameof(CurrencyPerSquareFoot), 2021); } }
        public static DisplayUnitType CurrencyPerBritishThermalUnitPerHour { get { return NonExistent(nameof(CurrencyPerBritishThermalUnitPerHour), 2021); } }
        public static DisplayUnitType CurrencyPerBritishThermalUnit { get { return NonExistent(nameof(CurrencyPerBritishThermalUnit), 2021); } }
        public static DisplayUnitType CubicMetersPerWattSecond { get { return NonExistent(nameof(CubicMetersPerWattSecond), 2021); } }
        public static DisplayUnitType CubicMetersPerKilogram { get { return NonExistent(nameof(CubicMetersPerKilogram), 2021); } }
        public static DisplayUnitType CubicMetersPerHourSquareMeter { get { return NonExistent(nameof(CubicMetersPerHourSquareMeter), 2021); } }
        public static DisplayUnitType CubicMetersPerHourCubicMeter { get { return NonExistent(nameof(CubicMetersPerHourCubicMeter), 2021); } }
        public static DisplayUnitType CubicFeetPerPoundMass { get { return NonExistent(nameof(CubicFeetPerPoundMass), 2021); } }
        public static DisplayUnitType CubicFeetPerMinutePerBritishThermalUnitPerHour { get { return NonExistent(nameof(CubicFeetPerMinutePerBritishThermalUnitPerHour), 2021); } }
        public static DisplayUnitType CandelasPerSquareFoot { get { return NonExistent(nameof(CandelasPerSquareFoot), 2021); } }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static DisplayUnitType NonExistent(string name, int version)
        {
            BH.Engine.Reflection.Compute.RecordWarning($"UnitTypeId.{name} does not have a DisplayUnitType equivalent in Revit versions older than {version}. UnitType.UT_Undefined has been used which may cause unit conversion issues.");
            return DisplayUnitType.DUT_UNDEFINED;
        }

        /***************************************************/
    }
}
#endif