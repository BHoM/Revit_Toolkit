/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
using System.ComponentModel;
using BH.oM.Revit.Attributes;

namespace BH.oM.Revit.Enums
{
    [Description("A collection of Revit categories supported by BHoM (UI, filtering etc.).")]
    public enum Category
    {
        [DisplayText("Abutment Foundation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AbutmentFoundationTags,
        [DisplayText("Abutment Pile Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AbutmentPileTags,
        [DisplayText("Abutment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeAbutmentTags,
        [DisplayText("Abutment Wall Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AbutmentWallTags,
        [DisplayText("Abutments")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeAbutments,
        [DisplayText("Adaptive Points")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_AdaptivePoints,
        [DisplayText("Air Terminal Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctTerminalTags,
        [DisplayText("Air Terminals")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctTerminal,
        [DisplayText("Alignment Station Label Sets")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AlignmentStationLabelSets,
        [DisplayText("Alignment Station Labels")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AlignmentStationLabels,
        [DisplayText("Alignment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AlignmentsTags,
        [DisplayText("Alignments")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_Alignments,
        [DisplayText("Analytical Link Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_LinkAnalyticalTags,
        [DisplayText("Analytical Links")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_LinksAnalytical,
        [DisplayText("Analytical Member Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalMemberTags,
        [DisplayText("Analytical Members")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalMember,
        [DisplayText("Analytical Node Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_NodeAnalyticalTags,
        [DisplayText("Analytical Nodes")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalNodes,
        [DisplayText("Analytical Opening Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalOpeningTags,
        [DisplayText("Analytical Openings")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalOpening,
        [DisplayText("Analytical Panel Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalPanelTags,
        [DisplayText("Analytical Panels")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalPanel,
        [DisplayText("Analytical Pipe Connections")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Piping")]
        OST_AnalyticalPipeConnections,
        [DisplayText("Analytical Spaces")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Architecture", "Mechanical")]
        OST_AnalyticSpaces,
        [DisplayText("Analytical Surfaces")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Architecture", "Mechanical")]
        OST_AnalyticSurfaces,
        [DisplayText("Anchor Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionAnchorTags,
        [DisplayText("Approach Slab Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_ApproachSlabTags,
        [DisplayText("Area Based Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_ELECTRICAL_AreaBasedLoads_Tags,
        [DisplayText("Area Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AreaLoadTags,
        [DisplayText("Area Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_AreaTags,
        [DisplayText("Areas")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_Areas,
        [DisplayText("Assemblies")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Assemblies,
        [DisplayText("Assembly Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_AssemblyTags,
        [DisplayText("Audio Visual Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_AudioVisualDeviceTags,
        [DisplayText("Audio Visual Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_AudioVisualDevices,
        [DisplayText("Bearing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeBearingTags,
        [DisplayText("Bearings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeBearings,
        [DisplayText("Bolt Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionBoltTags,
        [DisplayText("Boundary Conditions")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_BoundaryConditions,
        [DisplayText("Brace in Plan View Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructuralBracePlanReps,
        [DisplayText("Bridge Cable Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeCableTags,
        [DisplayText("Bridge Cables")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeCables,
        [DisplayText("Bridge Cross Bracing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingCrossBracingTags,
        [DisplayText("Bridge Deck Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeDeckTags,
        [DisplayText("Bridge Decks")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeDecks,
        [DisplayText("Bridge Diaphragm Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingDiaphragmTags,
        [DisplayText("Bridge Framing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingTags,
        [DisplayText("Bridge Framing")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFraming,
        [DisplayText("Bridge Truss Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingTrussTags,
        [DisplayText("Cable Tray Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_CableTrayFittingTags,
        [DisplayText("Cable Tray Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_CableTrayFitting,
        [DisplayText("Cable Tray Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_CableTrayTags,
        [DisplayText("Cable Trays")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_CableTray,
        [DisplayText("Callouts")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Callouts,
        [DisplayText("Casework Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CaseworkTags,
        [DisplayText("Casework")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Casework,
        [DisplayText("Ceiling Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CeilingTags,
        [DisplayText("Ceilings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Ceilings,
        [DisplayText("Color Fill Legends")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_ColorFillLegends,
        [DisplayText("Column Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_ColumnTags,
        [DisplayText("Columns")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Columns,
        [DisplayText("Communication Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_CommunicationDeviceTags,
        [DisplayText("Communication Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_CommunicationDevices,
        [DisplayText("Conduit Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_ConduitFittingTags,
        [DisplayText("Conduit Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_ConduitFitting,
        [DisplayText("Conduit Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_ConduitTags,
        [DisplayText("Conduits")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_Conduit,
        [DisplayText("Connection Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructConnectionSymbols,
        [DisplayText("Contour Labels")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_ContourLabels,
        [DisplayText("Curtain Panel Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallPanelTags,
        [DisplayText("Curtain Panels")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallPanels,
        [DisplayText("Curtain System Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CurtaSystemTags,
        [DisplayText("Curtain Systems")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_CurtaSystem,
        [DisplayText("Curtain Wall Mullion Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallMullionTags,
        [DisplayText("Curtain Wall Mullions")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallMullions,
        [DisplayText("Data Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_DataDeviceTags,
        [DisplayText("Data Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_DataDevices,
        [DisplayText("Detail Item Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_DetailComponentTags,
        [DisplayText("Detail Items")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_DetailComponents,
        [DisplayText("Dimensions")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Dimensions,
        [DisplayText("Displacement Path")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_DisplacementPath,
        [DisplayText("Door Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_DoorTags,
        [DisplayText("Doors")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Doors,
        [DisplayText("Duct Accessories")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctAccessory,
        [DisplayText("Duct Accessory Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctAccessoryTags,
        [DisplayText("Duct Color Fill Legends")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctColorFillLegends,
        [DisplayText("Duct Color Fill")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctColorFills,
        [DisplayText("Duct Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctFittingTags,
        [DisplayText("Duct Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctFitting,
        [DisplayText("Duct Insulation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctInsulationsTags,
        [DisplayText("Duct Insulations")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctInsulations,
        [DisplayText("Duct Lining Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctLiningsTags,
        [DisplayText("Duct Linings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctLinings,
        [DisplayText("Duct Placeholders")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_PlaceHolderDucts,
        [DisplayText("Duct Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctTags,
        [DisplayText("Ducts")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctCurves,
        [DisplayText("Electrical Analytical Loads")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Electrical")]
        OST_ElectricalLoadZoneInstance,
        [DisplayText("Electrical Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalEquipmentTags,
        [DisplayText("Electrical Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalEquipment,
        [DisplayText("Electrical Fixture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalFixtureTags,
        [DisplayText("Electrical Fixtures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalFixtures,
        [DisplayText("Elevations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Elev,
        [DisplayText("Entourage Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_EntourageTags,
        [DisplayText("Entourage")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Entourage,
        [DisplayText("Expansion Joint Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_ExpansionJointTags,
        [DisplayText("Expansion Joints")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_ExpansionJoints,
        [DisplayText("Fascia Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_FasciaTags,
        [DisplayText("Fire Alarm Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_FireAlarmDeviceTags,
        [DisplayText("Fire Alarm Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_FireAlarmDevices,
        [DisplayText("Fire Protection Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FireProtectionTags,
        [DisplayText("Fire Protection")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FireProtection,
        [DisplayText("Flex Duct Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_FlexDuctTags,
        [DisplayText("Flex Ducts")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_FlexDuctCurves,
        [DisplayText("Flex Pipe Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_FlexPipeTags,
        [DisplayText("Flex Pipes")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_FlexPipeCurves,
        [DisplayText("Floor Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_FloorTags,
        [DisplayText("Floors")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Floors,
        [DisplayText("Food Service Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FoodServiceEquipmentTags,
        [DisplayText("Food Service Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FoodServiceEquipment,
        [DisplayText("Foundation Span Direction Symbol")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_FootingSpanDirectionSymbol,
        [DisplayText("Furniture System Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_FurnitureSystemTags,
        [DisplayText("Furniture Systems")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_FurnitureSystems,
        [DisplayText("Furniture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_FurnitureTags,
        [DisplayText("Furniture")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Furniture,
        [DisplayText("Generic Annotations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_GenericAnnotation,
        [DisplayText("Generic Model Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_GenericModelTags,
        [DisplayText("Generic Models")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_GenericModel,
        [DisplayText("Grids")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Grids,
        [DisplayText("Guide Grid")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_GuideGrid,
        [DisplayText("Gutter Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_GutterTags,
        [DisplayText("Handrail Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_HandrailTags,
        [DisplayText("Hardscape Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_HardscapeTags,
        [DisplayText("Hardscape")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Hardscape,
        [DisplayText("Hole Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionHoleTags,
        [DisplayText("HVAC Zones")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_HVAC_Zones,
        [DisplayText("Internal Area Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_InternalAreaLoadTags,
        [DisplayText("Internal Line Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_InternalLineLoadTags,
        [DisplayText("Internal Point Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_InternalPointLoadTags,
        [DisplayText("Keynote Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_KeynoteTags,
        [DisplayText("Levels")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Levels,
        [DisplayText("Lighting Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_LightingDeviceTags,
        [DisplayText("Lighting Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_LightingDevices,
        [DisplayText("Lighting Fixture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Electrical", "Infrastructure")]
        OST_LightingFixtureTags,
        [DisplayText("Lighting Fixtures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Electrical", "Infrastructure")]
        OST_LightingFixtures,
        [DisplayText("Line Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_LineLoadTags,
        [DisplayText("Lines")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Lines,
        [DisplayText("Mass Floor Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_MassAreaFaceTags,
        [DisplayText("Mass Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_MassTags,
        [DisplayText("Mass")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Mass,
        [DisplayText("Matchline")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Matchline,
        [DisplayText("Material Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_MaterialTags,
        [DisplayText("Mechanical Control Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical")]
        OST_MechanicalControlDeviceTags,
        [DisplayText("Mechanical Control Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical")]
        OST_MechanicalControlDevices,
        [DisplayText("Mechanical Equipment Set Boundary Lines")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Piping")]
        OST_MechanicalEquipmentSetBoundaryLines,
        [DisplayText("Mechanical Equipment Set Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Piping")]
        OST_MechanicalEquipmentSetTags,
        [DisplayText("Mechanical Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_MechanicalEquipmentTags,
        [DisplayText("Mechanical Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_MechanicalEquipment,
        [DisplayText("Medical Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_MedicalEquipmentTags,
        [DisplayText("Medical Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_MedicalEquipment,
        [DisplayText("MEP Fabrication Containment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_FabricationContainmentTags,
        [DisplayText("MEP Fabrication Containment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_FabricationContainment,
        [DisplayText("MEP Fabrication Ductwork Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_FabricationDuctworkTags,
        [DisplayText("MEP Fabrication Ductwork")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_FabricationDuctwork,
        [DisplayText("MEP Fabrication Hanger Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_FabricationHangerTags,
        [DisplayText("MEP Fabrication Hangers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_FabricationHangers,
        [DisplayText("MEP Fabrication Pipework Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_FabricationPipeworkTags,
        [DisplayText("MEP Fabrication Pipework")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_FabricationPipework,
        [DisplayText("Model Group Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_ModelGroupTags,
        [DisplayText("Model Groups")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_IOSModelGroups,
        [DisplayText("Multi Leader Tag")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_MultiLeaderTag,
        [DisplayText("Multi-Category Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_MultiCategoryTags,
        [DisplayText("Nurse Call Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_NurseCallDeviceTags,
        [DisplayText("Nurse Call Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_NurseCallDevices,
        [DisplayText("Pad Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Infrastructure")]
        OST_PadTags,
        [DisplayText("Panel Schedule Graphics")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_PanelScheduleGraphics,
        [DisplayText("Parking Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_ParkingTags,
        [DisplayText("Parking")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Parking,
        [DisplayText("Part Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_PartTags,
        [DisplayText("Parts")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Parts,
        [DisplayText("Path of Travel Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_PathOfTravelTags,
        [DisplayText("Pier Cap Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierCapTags,
        [DisplayText("Pier Column Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierColumnTags,
        [DisplayText("Pier Foundation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFoundationTags,
        [DisplayText("Pier Pile Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierPileTags,
        [DisplayText("Pier Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgePierTags,
        [DisplayText("Pier Tower Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeTowerTags,
        [DisplayText("Pier Wall Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierWallTags,
        [DisplayText("Piers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgePiers,
        [DisplayText("Pipe Accessories")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeAccessory,
        [DisplayText("Pipe Accessory Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeAccessoryTags,
        [DisplayText("Pipe Color Fill Legends")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeColorFillLegends,
        [DisplayText("Pipe Color Fill")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeColorFills,
        [DisplayText("Pipe Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeFittingTags,
        [DisplayText("Pipe Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeFitting,
        [DisplayText("Pipe Insulation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeInsulationsTags,
        [DisplayText("Pipe Insulations")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeInsulations,
        [DisplayText("Pipe Placeholders")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PlaceHolderPipes,
        [DisplayText("Pipe Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeTags,
        [DisplayText("Pipes")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeCurves,
        [DisplayText("Plan Region")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_PlanRegion,
        [DisplayText("Planting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_PlantingTags,
        [DisplayText("Planting")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Planting,
        [DisplayText("Plate Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionPlateTags,
        [DisplayText("Plumbing Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_PlumbingEquipmentTags,
        [DisplayText("Plumbing Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_PlumbingEquipment,
        [DisplayText("Plumbing Fixture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Piping")]
        OST_PlumbingFixtureTags,
        [DisplayText("Plumbing Fixtures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Piping")]
        OST_PlumbingFixtures,
        [DisplayText("Point Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_PointLoadTags,
        [DisplayText("Profile Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionProfilesTags,
        [DisplayText("Property Line Segment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SitePropertyLineSegmentTags,
        [DisplayText("Property Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SitePropertyTags,
        [DisplayText("Railing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StairsRailingTags,
        [DisplayText("Railings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Infrastructure")]
        OST_StairsRailing,
        [DisplayText("Ramp Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_RampTags,
        [DisplayText("Ramps")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Ramps,
        [DisplayText("Raster Images")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_RasterImages,
        [DisplayText("Reference Lines")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ReferenceLines,
        [DisplayText("Reference Planes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_CLines,
        [DisplayText("Reference Points")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ReferencePoints,
        [DisplayText("Revision Cloud Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_RevisionCloudTags,
        [DisplayText("Revision Clouds")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_RevisionClouds,
        [DisplayText("Revision")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Revisions,
        [DisplayText("Road Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_RoadTags,
        [DisplayText("Roads")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Roads,
        [DisplayText("Roof Soffit Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_RoofSoffitTags,
        [DisplayText("Roof Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_RoofTags,
        [DisplayText("Roofs")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Roofs,
        [DisplayText("Room Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_RoomTags,
        [DisplayText("Rooms")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_Rooms,
        [DisplayText("RVT Link Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_RvtLinksTags,
        [DisplayText("Schedule Graphics")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ScheduleGraphics,
        [DisplayText("Schedules")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Schedules,
        [DisplayText("Scope Boxes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_VolumeOfInterest,
        [DisplayText("Section Boxes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_SectionBox,
        [DisplayText("Sections")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Sections,
        [DisplayText("Security Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_SecurityDeviceTags,
        [DisplayText("Security Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_SecurityDevices,
        [DisplayText("Shaft Openings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_ShaftOpening,
        [DisplayText("Shear Stud Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionShearStudTags,
        [DisplayText("Signage Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_SignageTags,
        [DisplayText("Signage")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Signage,
        [DisplayText("Site Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SiteTags,
        [DisplayText("Site")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Infrastructure")]
        OST_Site,
        [DisplayText("Slab Edge Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SlabEdgeTags,
        [DisplayText("Space Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_MEPSpaceTags,
        [DisplayText("Spaces")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_MEPSpaces,
        [DisplayText("Span Direction Symbol")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_SpanDirectionSymbol,
        [DisplayText("Specialty Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SpecialityEquipmentTags,
        [DisplayText("Specialty Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_SpecialityEquipment,
        [DisplayText("Spot Coordinates")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SpotCoordinates,
        [DisplayText("Spot Elevations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SpotElevations,
        [DisplayText("Spot Slopes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SpotSlopes,
        [DisplayText("Sprinkler Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_SprinklerTags,
        [DisplayText("Sprinklers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_Sprinklers,
        [DisplayText("Stair Landing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsLandingTags,
        [DisplayText("Stair Paths")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsPaths,
        [DisplayText("Stair Run Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsRunTags,
        [DisplayText("Stair Support Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsSupportTags,
        [DisplayText("Stair Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsTags,
        [DisplayText("Stair Tread/Riser Numbers")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsTriserNumbers,
        [DisplayText("Stairs")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Stairs,
        [DisplayText("Structural Annotations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructuralAnnotations,
        [DisplayText("Structural Area Reinforcement Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_AreaReinSpanSymbol,
        [DisplayText("Structural Area Reinforcement Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_AreaReinTags,
        [DisplayText("Structural Area Reinforcement")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_AreaRein,
        [DisplayText("Structural Beam System Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_BeamSystemTags,
        [DisplayText("Structural Beam Systems")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructuralFramingSystem,
        [DisplayText("Structural Column Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralColumnTags,
        [DisplayText("Structural Columns")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralColumns,
        [DisplayText("Structural Connection Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_StructConnectionTags,
        [DisplayText("Structural Connections")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructConnections,
        [DisplayText("Structural Fabric Areas")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricAreas,
        [DisplayText("Structural Fabric Reinforcement Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricReinSpanSymbol,
        [DisplayText("Structural Fabric Reinforcement Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricReinforcementTags,
        [DisplayText("Structural Fabric Reinforcement")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricReinforcement,
        [DisplayText("Structural Foundation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFoundationTags,
        [DisplayText("Structural Foundations")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFoundation,
        [DisplayText("Structural Framing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFramingTags,
        [DisplayText("Structural Framing")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFraming,
        [DisplayText("Structural Internal Loads")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_InternalLoads,
        [DisplayText("Structural Load Cases")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_LoadCases,
        [DisplayText("Structural Loads")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_Loads,
        [DisplayText("Structural Path Reinforcement Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_PathReinSpanSymbol,
        [DisplayText("Structural Path Reinforcement Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_PathReinTags,
        [DisplayText("Structural Path Reinforcement")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_PathRein,
        [DisplayText("Structural Rebar Coupler Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_CouplerTags,
        [DisplayText("Structural Rebar Couplers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_Coupler,
        [DisplayText("Structural Rebar Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_RebarTags,
        [DisplayText("Structural Rebar")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_Rebar,
        [DisplayText("Structural Stiffener Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_StructuralStiffenerTags,
        [DisplayText("Structural Stiffeners")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralStiffener,
        [DisplayText("Structural Tendon Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_StructuralTendonTags,
        [DisplayText("Structural Tendons")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_StructuralTendons,
        [DisplayText("Structural Truss Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_TrussTags,
        [DisplayText("Structural Trusses")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure")]
        OST_StructuralTruss,
        [DisplayText("System-Zone Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Piping")]
        OST_MEPSystemZoneTags,
        [DisplayText("System-Zones")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Mechanical")]
        OST_MEPSystemZone,
        [DisplayText("Telephone Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_TelephoneDeviceTags,
        [DisplayText("Telephone Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_TelephoneDevices,
        [DisplayText("Temporary Structure Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_TemporaryStructureTags,
        [DisplayText("Temporary Structures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_TemporaryStructure,
        [DisplayText("Text Notes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_TextNotes,
        [DisplayText("Title Blocks")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_TitleBlocks,
        [DisplayText("Top Rail Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_TopRailTags,
        [DisplayText("Topography")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Topography,
        [DisplayText("Vertical Circulation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_VerticalCirculationTags,
        [DisplayText("Vertical Circulation")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_VerticalCirculation,
        [DisplayText("Vibration Damper Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationDamperTags,
        [DisplayText("Vibration Isolator Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationIsolatorTags,
        [DisplayText("Vibration Management Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationManagementTags,
        [DisplayText("Vibration Management")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationManagement,
        [DisplayText("View Reference")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ReferenceViewer,
        [DisplayText("View Titles")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ViewportLabel,
        [DisplayText("Views")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Views,
        [DisplayText("Wall Sweep Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_WallSweepTags,
        [DisplayText("Wall Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_WallTags,
        [DisplayText("Walls")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_Walls,
        [DisplayText("Weld Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionWeldTags,
        [DisplayText("Window Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_WindowTags,
        [DisplayText("Windows")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Windows,
        [DisplayText("Wire Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_WireTags,
        [DisplayText("Wires")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_Wire,
        [DisplayText("Zone Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_ZoneTags,
    }
}
