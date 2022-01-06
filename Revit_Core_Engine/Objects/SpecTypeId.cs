/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
    [Description("This class is defined by BHoM only for Revit Versions < 2021. Revit versions from 2021 onwards define an equivalent class with the same name as part of their API." +
                 "This BHoM implementation eliminates breaking changes between different Revit API versions. It contains static properties that return the equivalents of UnitType.")]
    public static class SpecTypeId
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public static UnitType Length { get { return UnitType.UT_Length; } }
        public static UnitType Area { get { return UnitType.UT_Area; } }
        public static UnitType Volume { get { return UnitType.UT_Volume; } }
        public static UnitType Angle { get { return UnitType.UT_Angle; } }
        public static UnitType Number { get { return UnitType.UT_Number; } }
        public static UnitType SheetLength { get { return UnitType.UT_SheetLength; } }
        public static UnitType SiteAngle { get { return UnitType.UT_SiteAngle; } }
        public static UnitType HvacDensity { get { return UnitType.UT_HVAC_Density; } }
        public static UnitType HvacEnergy { get { return UnitType.UT_HVAC_Energy; } }
        public static UnitType HvacFriction { get { return UnitType.UT_HVAC_Friction; } }
        public static UnitType HvacPower { get { return UnitType.UT_HVAC_Power; } }
        public static UnitType HvacPowerDensity { get { return UnitType.UT_HVAC_Power_Density; } }
        public static UnitType HvacPressure { get { return UnitType.UT_HVAC_Pressure; } }
        public static UnitType HvacTemperature { get { return UnitType.UT_HVAC_Temperature; } }
        public static UnitType HvacVelocity { get { return UnitType.UT_HVAC_Velocity; } }
        public static UnitType AirFlow { get { return UnitType.UT_HVAC_Airflow; } }
        public static UnitType DuctSize { get { return UnitType.UT_HVAC_DuctSize; } }
        public static UnitType CrossSection { get { return UnitType.UT_HVAC_CrossSection; } }
        public static UnitType HeatGain { get { return UnitType.UT_HVAC_HeatGain; } }
        public static UnitType Current { get { return UnitType.UT_Electrical_Current; } }
        public static UnitType ElectricalPotential { get { return UnitType.UT_Electrical_Potential; } }
        public static UnitType ElectricalFrequency { get { return UnitType.UT_Electrical_Frequency; } }
        public static UnitType Illuminance { get { return UnitType.UT_Electrical_Illuminance; } }
        public static UnitType LuminousFlux { get { return UnitType.UT_Electrical_Luminous_Flux; } }
        public static UnitType ElectricalPower { get { return UnitType.UT_Electrical_Power; } }
        public static UnitType HvacRoughness { get { return UnitType.UT_HVAC_Roughness; } }
        public static UnitType Force { get { return UnitType.UT_Force; } }
        public static UnitType LinearForce { get { return UnitType.UT_LinearForce; } }
        public static UnitType AreaForce { get { return UnitType.UT_AreaForce; } }
        public static UnitType Moment { get { return UnitType.UT_Moment; } }
        public static UnitType ForceScale { get { return UnitType.UT_ForceScale; } }
        public static UnitType LinearForceScale { get { return UnitType.UT_LinearForceScale; } }
        public static UnitType AreaForceScale { get { return UnitType.UT_AreaForceScale; } }
        public static UnitType MomentScale { get { return UnitType.UT_MomentScale; } }
        public static UnitType ApparentPower { get { return UnitType.UT_Electrical_Apparent_Power; } }
        public static UnitType ElectricalPowerDensity { get { return UnitType.UT_Electrical_Power_Density; } }
        public static UnitType PipingDensity { get { return UnitType.UT_Piping_Density; } }
        public static UnitType Flow { get { return UnitType.UT_Piping_Flow; } }
        public static UnitType PipingFriction { get { return UnitType.UT_Piping_Friction; } }
        public static UnitType PipingPressure { get { return UnitType.UT_Piping_Pressure; } }
        public static UnitType PipingTemperature { get { return UnitType.UT_Piping_Temperature; } }
        public static UnitType PipingVelocity { get { return UnitType.UT_Piping_Velocity; } }
        public static UnitType PipingViscosity { get { return UnitType.UT_Piping_Viscosity; } }
        public static UnitType PipeSize { get { return UnitType.UT_PipeSize; } }
        public static UnitType PipingRoughness { get { return UnitType.UT_Piping_Roughness; } }
        public static UnitType Stress { get { return UnitType.UT_Stress; } }
        public static UnitType UnitWeight { get { return UnitType.UT_UnitWeight; } }
        public static UnitType ThermalExpansionCoefficient { get { return UnitType.UT_ThermalExpansion; } }
        public static UnitType LinearMoment { get { return UnitType.UT_LinearMoment; } }
        public static UnitType LinearMomentScale { get { return UnitType.UT_LinearMomentScale; } }
        public static UnitType PointSpringCoefficient { get { return UnitType.UT_ForcePerLength; } }
        public static UnitType RotationalPointSpringCoefficient { get { return UnitType.UT_ForceLengthPerAngle; } }
        public static UnitType LineSpringCoefficient { get { return UnitType.UT_LinearForcePerLength; } }
        public static UnitType RotationalLineSpringCoefficient { get { return UnitType.UT_LinearForceLengthPerAngle; } }
        public static UnitType AreaSpringCoefficient { get { return UnitType.UT_AreaForcePerLength; } }
        public static UnitType PipingVolume { get { return UnitType.UT_Piping_Volume; } }
        public static UnitType HvacViscosity { get { return UnitType.UT_HVAC_Viscosity; } }
        public static UnitType HeatTransferCoefficient { get { return UnitType.UT_HVAC_CoefficientOfHeatTransfer; } }
        public static UnitType AirFlowDensity { get { return UnitType.UT_HVAC_Airflow_Density; } }
        public static UnitType Slope { get { return UnitType.UT_Slope; } }
        public static UnitType CoolingLoad { get { return UnitType.UT_HVAC_Cooling_Load; } }
        public static UnitType CoolingLoadDividedByArea { get { return UnitType.UT_HVAC_Cooling_Load_Divided_By_Area; } }
        public static UnitType CoolingLoadDividedByVolume { get { return UnitType.UT_HVAC_Cooling_Load_Divided_By_Volume; } }
        public static UnitType HeatingLoad { get { return UnitType.UT_HVAC_Heating_Load; } }
        public static UnitType HeatingLoadDividedByArea { get { return UnitType.UT_HVAC_Heating_Load_Divided_By_Area; } }
        public static UnitType HeatingLoadDividedByVolume { get { return UnitType.UT_HVAC_Heating_Load_Divided_By_Volume; } }
        public static UnitType AirFlowDividedByVolume { get { return UnitType.UT_HVAC_Airflow_Divided_By_Volume; } }
        public static UnitType AirFlowDividedByCoolingLoad { get { return UnitType.UT_HVAC_Airflow_Divided_By_Cooling_Load; } }
        public static UnitType AreaDividedByCoolingLoad { get { return UnitType.UT_HVAC_Area_Divided_By_Cooling_Load; } }
        public static UnitType WireDiameter { get { return UnitType.UT_WireSize; } }
        public static UnitType HvacSlope { get { return UnitType.UT_HVAC_Slope; } }
        public static UnitType PipingSlope { get { return UnitType.UT_Piping_Slope; } }
        public static UnitType Currency { get { return UnitType.UT_Currency; } }
        public static UnitType Efficacy { get { return UnitType.UT_Electrical_Efficacy; } }
        public static UnitType Wattage { get { return UnitType.UT_Electrical_Wattage; } }
        public static UnitType ColorTemperature { get { return UnitType.UT_Color_Temperature; } }
        public static UnitType DecimalSheetLength { get { return UnitType.UT_DecSheetLength; } }
        public static UnitType LuminousIntensity { get { return UnitType.UT_Electrical_Luminous_Intensity; } }
        public static UnitType Luminance { get { return UnitType.UT_Electrical_Luminance; } }
        public static UnitType AreaDividedByHeatingLoad { get { return UnitType.UT_HVAC_Area_Divided_By_Heating_Load; } }
        public static UnitType Factor { get { return UnitType.UT_HVAC_Factor; } }
        public static UnitType ElectricalTemperature { get { return UnitType.UT_Electrical_Temperature; } }
        public static UnitType CableTraySize { get { return UnitType.UT_Electrical_CableTraySize; } }
        public static UnitType ConduitSize { get { return UnitType.UT_Electrical_ConduitSize; } }
        public static UnitType ReinforcementVolume { get { return UnitType.UT_Reinforcement_Volume; } }
        public static UnitType ReinforcementLength { get { return UnitType.UT_Reinforcement_Length; } }
        public static UnitType DemandFactor { get { return UnitType.UT_Electrical_Demand_Factor; } }
        public static UnitType DuctInsulationThickness { get { return UnitType.UT_HVAC_DuctInsulationThickness; } }
        public static UnitType DuctLiningThickness { get { return UnitType.UT_HVAC_DuctLiningThickness; } }
        public static UnitType PipeInsulationThickness { get { return UnitType.UT_PipeInsulationThickness; } }
        public static UnitType ThermalResistance { get { return UnitType.UT_HVAC_ThermalResistance; } }
        public static UnitType ThermalMass { get { return UnitType.UT_HVAC_ThermalMass; } }
        public static UnitType Acceleration { get { return UnitType.UT_Acceleration; } }
        public static UnitType BarDiameter { get { return UnitType.UT_Bar_Diameter; } }
        public static UnitType CrackWidth { get { return UnitType.UT_Crack_Width; } }
        public static UnitType Displacement { get { return UnitType.UT_Displacement_Deflection; } }
        public static UnitType Energy { get { return UnitType.UT_Energy; } }
        public static UnitType StructuralFrequency { get { return UnitType.UT_Structural_Frequency; } }
        public static UnitType Mass { get { return UnitType.UT_Mass; } }
        public static UnitType MassPerUnitLength { get { return UnitType.UT_Mass_per_Unit_Length; } }
        public static UnitType MomentOfInertia { get { return UnitType.UT_Moment_of_Inertia; } }
        public static UnitType SurfaceAreaPerUnitLength { get { return UnitType.UT_Surface_Area; } }
        public static UnitType Period { get { return UnitType.UT_Period; } }
        public static UnitType Pulsation { get { return UnitType.UT_Pulsation; } }
        public static UnitType ReinforcementArea { get { return UnitType.UT_Reinforcement_Area; } }
        public static UnitType ReinforcementAreaPerUnitLength { get { return UnitType.UT_Reinforcement_Area_per_Unit_Length; } }
        public static UnitType ReinforcementCover { get { return UnitType.UT_Reinforcement_Cover; } }
        public static UnitType ReinforcementSpacing { get { return UnitType.UT_Reinforcement_Spacing; } }
        public static UnitType Rotation { get { return UnitType.UT_Rotation; } }
        public static UnitType SectionArea { get { return UnitType.UT_Section_Area; } }
        public static UnitType SectionDimension { get { return UnitType.UT_Section_Dimension; } }
        public static UnitType SectionModulus { get { return UnitType.UT_Section_Modulus; } }
        public static UnitType SectionProperty { get { return UnitType.UT_Section_Property; } }
        public static UnitType StructuralVelocity { get { return UnitType.UT_Structural_Velocity; } }
        public static UnitType WarpingConstant { get { return UnitType.UT_Warping_Constant; } }
        public static UnitType Weight { get { return UnitType.UT_Weight; } }
        public static UnitType WeightPerUnitLength { get { return UnitType.UT_Weight_per_Unit_Length; } }
        public static UnitType ThermalConductivity { get { return UnitType.UT_HVAC_ThermalConductivity; } }
        public static UnitType SpecificHeat { get { return UnitType.UT_HVAC_SpecificHeat; } }
        public static UnitType SpecificHeatOfVaporization { get { return UnitType.UT_HVAC_SpecificHeatOfVaporization; } }
        public static UnitType Permeability { get { return UnitType.UT_HVAC_Permeability; } }
        public static UnitType ElectricalResistivity { get { return UnitType.UT_Electrical_Resistivity; } }
        public static UnitType MassDensity { get { return UnitType.UT_MassDensity; } }
        public static UnitType MassPerUnitArea { get { return UnitType.UT_MassPerUnitArea; } }
        public static UnitType PipeDimension { get { return UnitType.UT_Pipe_Dimension; } }
        public static UnitType PipingMass { get { return UnitType.UT_PipeMass; } }
        public static UnitType PipeMassPerUnitLength { get { return UnitType.UT_PipeMassPerUnitLength; } }
        public static UnitType HvacTemperatureDifference { get { return UnitType.UT_HVAC_TemperatureDifference; } }
        public static UnitType PipingTemperatureDifference { get { return UnitType.UT_Piping_TemperatureDifference; } }
        public static UnitType ElectricalTemperatureDifference { get { return UnitType.UT_Electrical_TemperatureDifference; } }
        public static UnitType Custom { get { return UnitType.UT_Custom; } }

        public static UnitType Stationing { get { return NonExistent(nameof(Stationing), 2021); } }
        public static UnitType ThermalGradientCoefficientForMoistureCapacity { get { return NonExistent(nameof(ThermalGradientCoefficientForMoistureCapacity), 2021); } }
        public static UnitType StationingInterval { get { return NonExistent(nameof(StationingInterval), 2021); } }
        public static UnitType RotationAngle { get { return NonExistent(nameof(RotationAngle), 2021); } }
        public static UnitType PowerPerLength { get { return NonExistent(nameof(PowerPerLength), 2021); } }
        public static UnitType PowerPerFlow { get { return NonExistent(nameof(PowerPerFlow), 2021); } }
        public static UnitType PipingMassPerTime { get { return NonExistent(nameof(PipingMassPerTime), 2021); } }
        public static UnitType IsothermalMoistureCapacity { get { return NonExistent(nameof(IsothermalMoistureCapacity), 2021); } }
        public static UnitType HvacMassPerTime { get { return NonExistent(nameof(HvacMassPerTime), 2021); } }
        public static UnitType FlowPerPower { get { return NonExistent(nameof(FlowPerPower), 2021); } }
        public static UnitType Distance { get { return NonExistent(nameof(Distance), 2021); } }
        public static UnitType Diffusivity { get { return NonExistent(nameof(Diffusivity), 2021); } }
        public static UnitType CostRatePower { get { return NonExistent(nameof(CostRatePower), 2021); } }
        public static UnitType CostRateEnergy { get { return NonExistent(nameof(CostRateEnergy), 2021); } }
        public static UnitType CostPerArea { get { return NonExistent(nameof(CostPerArea), 2021); } }
        public static UnitType AngularSpeed { get { return NonExistent(nameof(AngularSpeed), 2021); } }
        
#if (REVIT2018 || REVIT2019)
        public static UnitType Time { get { return NonExistent(nameof(Time), 2020); } }
        public static UnitType Speed { get { return NonExistent(nameof(Speed), 2020); } }
#else
        public static UnitType Time { get { return UnitType.UT_TimeInterval; } }
        public static UnitType Speed { get { return UnitType.UT_Speed; } }
#endif

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static UnitType NonExistent(string name, int version)
        {
            BH.Engine.Reflection.Compute.RecordWarning($"SpecTypeId.{name} does not have a UnitType equivalent in Revit versions older than {version}. UnitType.UT_Undefined has been used which may cause unit conversion issues.");
            return UnitType.UT_Undefined;
        }

        /***************************************************/
    }
}
#endif
