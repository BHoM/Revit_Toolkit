/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static DisplayUnitType BHoMUnitType(this UnitType unitType)
        {
            switch (unitType)
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
                case UnitType.UT_Reinforcement_Spacing:
                case UnitType.UT_Reinforcement_Length:
                    return DisplayUnitType.DUT_METERS;
                case UnitType.UT_Electrical_Current:
                    return DisplayUnitType.DUT_AMPERES;
                case UnitType.UT_HVAC_Temperature:
                    return DisplayUnitType.DUT_KELVIN;
                case UnitType.UT_Mass:
                case UnitType.UT_Weight:
                    return DisplayUnitType.DUT_KILOGRAMS_MASS;
                case UnitType.UT_HVAC_Pressure:
                case UnitType.UT_Piping_Pressure:
                case UnitType.UT_Stress:
                    return DisplayUnitType.DUT_PASCALS;
                case UnitType.UT_HVAC_Velocity:
                    return DisplayUnitType.DUT_METERS_PER_SECOND;
                case UnitType.UT_Area:
                    return DisplayUnitType.DUT_SQUARE_METERS;
                case UnitType.UT_Volume:
                    return DisplayUnitType.DUT_CUBIC_METERS;
                case UnitType.UT_HVAC_Airflow:
                    return DisplayUnitType.DUT_CUBIC_METERS_PER_SECOND;
                case UnitType.UT_HVAC_Cooling_Load:
                case UnitType.UT_HVAC_Heating_Load:
                    return DisplayUnitType.DUT_WATTS;
                case UnitType.UT_MassDensity:
                    return DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER;
                case UnitType.UT_ThermalExpansion:
                    return DisplayUnitType.DUT_INV_CELSIUS;
                default:
                    return DisplayUnitType.DUT_GENERAL;
            }
        }

        /***************************************************/
    }
}
