/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

#if REVIT2022 || REVIT2023 || REVIT2024
using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static class GroupTypeId
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        public static BuiltInParameterGroup AdskModelProperties { get { return BuiltInParameterGroup.PG_ADSK_MODEL_PROPERTIES; } }
        public static BuiltInParameterGroup AnalysisResults { get { return BuiltInParameterGroup.PG_ANALYSIS_RESULTS; } }
        public static BuiltInParameterGroup AnalyticalAlignment { get { return BuiltInParameterGroup.PG_ANALYTICAL_ALIGNMENT; } }
        public static BuiltInParameterGroup AnalyticalModel { get { return BuiltInParameterGroup.PG_ANALYTICAL_MODEL; } }
        public static BuiltInParameterGroup AnalyticalProperties { get { return BuiltInParameterGroup.PG_ANALYTICAL_PROPERTIES; } }
        public static BuiltInParameterGroup Area { get { return BuiltInParameterGroup.PG_AREA; } }
        public static BuiltInParameterGroup ConceptualEnergyData { get { return BuiltInParameterGroup.PG_CONCEPTUAL_ENERGY_DATA; } }
        public static BuiltInParameterGroup ConceptualEnergyDataBuildingServices { get { return BuiltInParameterGroup.PG_CONCEPTUAL_ENERGY_DATA_BUILDING_SERVICES; } }
        public static BuiltInParameterGroup Constraints { get { return BuiltInParameterGroup.PG_CONSTRAINTS; } }
        public static BuiltInParameterGroup Construction { get { return BuiltInParameterGroup.PG_CONSTRUCTION; } }
        public static BuiltInParameterGroup ContinuousrailBeginBottomExtension { get { return BuiltInParameterGroup.PG_CONTINUOUSRAIL_BEGIN_BOTTOM_EXTENSION; } }
        public static BuiltInParameterGroup ContinuousrailEndTopExtension { get { return BuiltInParameterGroup.PG_CONTINUOUSRAIL_END_TOP_EXTENSION; } }
        public static BuiltInParameterGroup CouplerArray { get { return BuiltInParameterGroup.PG_COUPLER_ARRAY; } }
        public static BuiltInParameterGroup CurtainGrid { get { return BuiltInParameterGroup.PG_CURTAIN_GRID; } }
        public static BuiltInParameterGroup CurtainGridHoriz { get { return BuiltInParameterGroup.PG_CURTAIN_GRID_HORIZ; } }
        public static BuiltInParameterGroup CurtainGridn1 { get { return BuiltInParameterGroup.PG_CURTAIN_GRID_1; } }
        public static BuiltInParameterGroup CurtainGridn2 { get { return BuiltInParameterGroup.PG_CURTAIN_GRID_2; } }
        public static BuiltInParameterGroup CurtainGridU { get { return BuiltInParameterGroup.PG_CURTAIN_GRID_U; } }
        public static BuiltInParameterGroup CurtainGridV { get { return BuiltInParameterGroup.PG_CURTAIN_GRID_V; } }
        public static BuiltInParameterGroup CurtainGridVert { get { return BuiltInParameterGroup.PG_CURTAIN_GRID_VERT; } }
        public static BuiltInParameterGroup CurtainMullionHoriz { get { return BuiltInParameterGroup.PG_CURTAIN_MULLION_HORIZ; } }
        public static BuiltInParameterGroup CurtainMullionn1 { get { return BuiltInParameterGroup.PG_CURTAIN_MULLION_1; } }
        public static BuiltInParameterGroup CurtainMullionn2 { get { return BuiltInParameterGroup.PG_CURTAIN_MULLION_2; } }
        public static BuiltInParameterGroup CurtainMullionVert { get { return BuiltInParameterGroup.PG_CURTAIN_MULLION_VERT; } }
        public static BuiltInParameterGroup Data { get { return BuiltInParameterGroup.PG_DATA; } }
        public static BuiltInParameterGroup Display { get { return BuiltInParameterGroup.PG_DISPLAY; } }
        public static BuiltInParameterGroup DivisionGeometry { get { return BuiltInParameterGroup.PG_DIVISION_GEOMETRY; } }
        public static BuiltInParameterGroup Electrical { get { return BuiltInParameterGroup.PG_ELECTRICAL; } }
        public static BuiltInParameterGroup ElectricalCircuiting { get { return BuiltInParameterGroup.PG_ELECTRICAL_CIRCUITING; } }
        public static BuiltInParameterGroup ElectricalLighting { get { return BuiltInParameterGroup.PG_ELECTRICAL_LIGHTING; } }
        public static BuiltInParameterGroup ElectricalLoads { get { return BuiltInParameterGroup.PG_ELECTRICAL_LOADS; } }
        public static BuiltInParameterGroup EnergyAnalysis { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS; } }
        public static BuiltInParameterGroup EnergyAnalysisAdvanced { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_ADVANCED; } }
        public static BuiltInParameterGroup EnergyAnalysisBldgConsMtlThermalProps { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_BLDG_CONS_MTL_THERMAL_PROPS; } }
        public static BuiltInParameterGroup EnergyAnalysisBuildingData { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_BUILDING_DATA; } }
        public static BuiltInParameterGroup EnergyAnalysisConceptualModel { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_CONCEPTUAL_MODEL; } }
        public static BuiltInParameterGroup EnergyAnalysisDetailedAndConceptualModels { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_DETAILED_AND_CONCEPTUAL_MODELS; } }
        public static BuiltInParameterGroup EnergyAnalysisDetailedModel { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_DETAILED_MODEL; } }
        public static BuiltInParameterGroup EnergyAnalysisRoomSpaceData { get { return BuiltInParameterGroup.PG_ENERGY_ANALYSIS_ROOM_SPACE_DATA; } }
        public static BuiltInParameterGroup FabricationProductData { get { return BuiltInParameterGroup.PG_FABRICATION_PRODUCT_DATA; } }
        public static BuiltInParameterGroup FireProtection { get { return BuiltInParameterGroup.PG_FIRE_PROTECTION; } }
        public static BuiltInParameterGroup Fitting { get { return BuiltInParameterGroup.PG_FITTING; } }
        public static BuiltInParameterGroup Flexible { get { return BuiltInParameterGroup.PG_FLEXIBLE; } }
        public static BuiltInParameterGroup Forces { get { return BuiltInParameterGroup.PG_FORCES; } }
        public static BuiltInParameterGroup General { get { return BuiltInParameterGroup.PG_GENERAL; } }
        public static BuiltInParameterGroup GeoLocation { get { return BuiltInParameterGroup.PG_GEO_LOCATION; } }
        public static BuiltInParameterGroup Geometry { get { return BuiltInParameterGroup.PG_GEOMETRY; } }
        public static BuiltInParameterGroup GeometryPositioning { get { return BuiltInParameterGroup.PG_GEOMETRY_POSITIONING; } }
        public static BuiltInParameterGroup Graphics { get { return BuiltInParameterGroup.PG_GRAPHICS; } }
        public static BuiltInParameterGroup GreenBuilding { get { return BuiltInParameterGroup.PG_GREEN_BUILDING; } }
        public static BuiltInParameterGroup IdentityData { get { return BuiltInParameterGroup.PG_IDENTITY_DATA; } }
        public static BuiltInParameterGroup Ifc { get { return BuiltInParameterGroup.PG_IFC; } }
        public static BuiltInParameterGroup Insulation { get { return BuiltInParameterGroup.PG_INSULATION; } }
        public static BuiltInParameterGroup Length { get { return BuiltInParameterGroup.PG_LENGTH; } }
        public static BuiltInParameterGroup LightPhotometrics { get { return BuiltInParameterGroup.PG_LIGHT_PHOTOMETRICS; } }
        public static BuiltInParameterGroup Lining { get { return BuiltInParameterGroup.PG_LINING; } }
        public static BuiltInParameterGroup Materials { get { return BuiltInParameterGroup.PG_MATERIALS; } }
        public static BuiltInParameterGroup Mechanical { get { return BuiltInParameterGroup.PG_MECHANICAL; } }
        public static BuiltInParameterGroup MechanicalAirflow { get { return BuiltInParameterGroup.PG_MECHANICAL_AIRFLOW; } }
        public static BuiltInParameterGroup MechanicalLoads { get { return BuiltInParameterGroup.PG_MECHANICAL_LOADS; } }
        public static BuiltInParameterGroup Moments { get { return BuiltInParameterGroup.PG_MOMENTS; } }
        public static BuiltInParameterGroup Nodes { get { return BuiltInParameterGroup.PG_NODES; } }
        public static BuiltInParameterGroup OverallLegend { get { return BuiltInParameterGroup.PG_OVERALL_LEGEND; } }
        public static BuiltInParameterGroup Pattern { get { return BuiltInParameterGroup.PG_PATTERN; } }
        public static BuiltInParameterGroup PatternApplication { get { return BuiltInParameterGroup.PG_PATTERN_APPLICATION; } }
        public static BuiltInParameterGroup Phasing { get { return BuiltInParameterGroup.PG_PHASING; } }
        public static BuiltInParameterGroup Plumbing { get { return BuiltInParameterGroup.PG_PLUMBING; } }
        public static BuiltInParameterGroup PrimaryEnd { get { return BuiltInParameterGroup.PG_PRIMARY_END; } }
        public static BuiltInParameterGroup Profile { get { return BuiltInParameterGroup.PG_PROFILE; } }
        public static BuiltInParameterGroup Profilen1 { get { return BuiltInParameterGroup.PG_PROFILE_1; } }
        public static BuiltInParameterGroup Profilen2 { get { return BuiltInParameterGroup.PG_PROFILE_2; } }
        public static BuiltInParameterGroup RailingSystemFamilyHandrails { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_FAMILY_HANDRAILS; } }
        public static BuiltInParameterGroup RailingSystemFamilySegmentPattern { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_FAMILY_SEGMENT_PATTERN; } }
        public static BuiltInParameterGroup RailingSystemFamilyTopRail { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_FAMILY_TOP_RAIL; } }
        public static BuiltInParameterGroup RailingSystemSecondaryFamilyHandrails { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_SECONDARY_FAMILY_HANDRAILS; } }
        public static BuiltInParameterGroup RailingSystemSegmentPatternRemainder { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_PATTERN_REMAINDER; } }
        public static BuiltInParameterGroup RailingSystemSegmentPatternRepeat { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_PATTERN_REPEAT; } }
        public static BuiltInParameterGroup RailingSystemSegmentPosts { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_POSTS; } }
        public static BuiltInParameterGroup RailingSystemSegmentUGrid { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_U_GRID; } }
        public static BuiltInParameterGroup RailingSystemSegmentVGrid { get { return BuiltInParameterGroup.PG_RAILING_SYSTEM_SEGMENT_V_GRID; } }
        public static BuiltInParameterGroup RebarArray { get { return BuiltInParameterGroup.PG_REBAR_ARRAY; } }
        public static BuiltInParameterGroup RebarSystemLayers { get { return BuiltInParameterGroup.PG_REBAR_SYSTEM_LAYERS; } }
        public static BuiltInParameterGroup Reference { get { return BuiltInParameterGroup.PG_REFERENCE; } }
        public static BuiltInParameterGroup ReleasesMemberForces { get { return BuiltInParameterGroup.PG_RELEASES_MEMBER_FORCES; } }
        public static BuiltInParameterGroup RotationAbout { get { return BuiltInParameterGroup.PG_ROTATION_ABOUT; } }
        public static BuiltInParameterGroup RouteAnalysis { get { return BuiltInParameterGroup.PG_ROUTE_ANALYSIS; } }
        public static BuiltInParameterGroup SecondaryEnd { get { return BuiltInParameterGroup.PG_SECONDARY_END; } }
        public static BuiltInParameterGroup SegmentsFittings { get { return BuiltInParameterGroup.PG_SEGMENTS_FITTINGS; } }
        public static BuiltInParameterGroup SlabShapeEdit { get { return BuiltInParameterGroup.PG_SLAB_SHAPE_EDIT; } }
        public static BuiltInParameterGroup SplitProfileDimensions { get { return BuiltInParameterGroup.PG_SPLIT_PROFILE_DIMENSIONS; } }
        public static BuiltInParameterGroup StairRisers { get { return BuiltInParameterGroup.PG_STAIR_RISERS; } }
        public static BuiltInParameterGroup StairsCalculatorRules { get { return BuiltInParameterGroup.PG_STAIRS_CALCULATOR_RULES; } }
        public static BuiltInParameterGroup StairsOpenEndConnection { get { return BuiltInParameterGroup.PG_STAIRS_OPEN_END_CONNECTION; } }
        public static BuiltInParameterGroup StairsSupports { get { return BuiltInParameterGroup.PG_STAIRS_SUPPORTS; } }
        public static BuiltInParameterGroup StairsTreadsRisers { get { return BuiltInParameterGroup.PG_STAIRS_TREADS_RISERS; } }
        public static BuiltInParameterGroup StairStringers { get { return BuiltInParameterGroup.PG_STAIR_STRINGERS; } }
        public static BuiltInParameterGroup StairsWinders { get { return BuiltInParameterGroup.PG_STAIRS_WINDERS; } }
        public static BuiltInParameterGroup StairTreads { get { return BuiltInParameterGroup.PG_STAIR_TREADS; } }
        public static BuiltInParameterGroup Structural { get { return BuiltInParameterGroup.PG_STRUCTURAL; } }
        public static BuiltInParameterGroup StructuralAnalysis { get { return BuiltInParameterGroup.PG_STRUCTURAL_ANALYSIS; } }
        public static BuiltInParameterGroup StructuralSectionGeometry { get { return BuiltInParameterGroup.PG_STRUCTURAL_SECTION_GEOMETRY; } }
        public static BuiltInParameterGroup Support { get { return BuiltInParameterGroup.PG_SUPPORT; } }
        public static BuiltInParameterGroup SystemtypeRisedrop { get { return BuiltInParameterGroup.PG_SYSTEMTYPE_RISEDROP; } }
        public static BuiltInParameterGroup Text { get { return BuiltInParameterGroup.PG_TEXT; } }
        public static BuiltInParameterGroup Title { get { return BuiltInParameterGroup.PG_TITLE; } }
        public static BuiltInParameterGroup TranslationIn { get { return BuiltInParameterGroup.PG_TRANSLATION_IN; } }
        public static BuiltInParameterGroup TrussFamilyBottomChord { get { return BuiltInParameterGroup.PG_TRUSS_FAMILY_BOTTOM_CHORD; } }
        public static BuiltInParameterGroup TrussFamilyDiagWeb { get { return BuiltInParameterGroup.PG_TRUSS_FAMILY_DIAG_WEB; } }
        public static BuiltInParameterGroup TrussFamilyTopChord { get { return BuiltInParameterGroup.PG_TRUSS_FAMILY_TOP_CHORD; } }
        public static BuiltInParameterGroup TrussFamilyVertWeb { get { return BuiltInParameterGroup.PG_TRUSS_FAMILY_VERT_WEB; } }
        public static BuiltInParameterGroup Underlay { get { return BuiltInParameterGroup.PG_UNDERLAY; } }
        public static BuiltInParameterGroup ViewCamera { get { return BuiltInParameterGroup.PG_VIEW_CAMERA; } }
        public static BuiltInParameterGroup ViewExtents { get { return BuiltInParameterGroup.PG_VIEW_EXTENTS; } }
        public static BuiltInParameterGroup Visibility { get { return BuiltInParameterGroup.PG_VISIBILITY; } }

        public static BuiltInParameterGroup LifeSafety { [NotImplemented] get { return NonExistent(nameof(LifeSafety), 2022); } }
        public static BuiltInParameterGroup StructuralSectionDimensions { [NotImplemented] get { return NonExistent(nameof(StructuralSectionDimensions), 2022); } }
        public static BuiltInParameterGroup ToposolidSubdivision { [NotImplemented] get { return NonExistent(nameof(ToposolidSubdivision), 2022); } }
        public static BuiltInParameterGroup WallCrossSectionDefinition { [NotImplemented] get { return NonExistent(nameof(WallCrossSectionDefinition), 2022); } }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static BuiltInParameterGroup NonExistent(string name, int version)
        {
            BH.Engine.Base.Compute.RecordWarning($"GroupTypeId.{name} does not have a BuiltInParameterGroup equivalent in Revit versions older than {version}. UnitType.UT_Undefined has been used which may cause unit conversion issues.");
            return BuiltInParameterGroup.INVALID;
        }

        /***************************************************/
    }
}
#endif



