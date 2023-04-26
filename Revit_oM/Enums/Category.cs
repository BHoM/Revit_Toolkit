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
        OST_AbutmentFoundationTags = -2006208,
        [DisplayText("Abutment Pile Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AbutmentPileTags = -2006209,
        [DisplayText("Abutment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeAbutmentTags = -2006170,
        [DisplayText("Abutment Wall Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AbutmentWallTags = -2006210,
        [DisplayText("Abutments")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeAbutments = -2006130,
        [DisplayText("Adaptive Points")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_AdaptivePoints = -2000900,
        [DisplayText("Air Terminal Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctTerminalTags = -2008014,
        [DisplayText("Air Terminals")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctTerminal = -2008013,
        [DisplayText("Alignment Station Label Sets")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AlignmentStationLabelSets = -2001016,
        [DisplayText("Alignment Station Labels")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AlignmentStationLabels = -2001017,
        [DisplayText("Alignment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_AlignmentsTags = -2001015,
        [DisplayText("Alignments")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_Alignments = -2001012,
        [DisplayText("Analytical Link Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_LinkAnalyticalTags = -2000955,
        [DisplayText("Analytical Links")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_LinksAnalytical = -2009657,
        [DisplayText("Analytical Member Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalMemberTags = -2009663,
        [DisplayText("Analytical Members")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalMember = -2009662,
        [DisplayText("Analytical Node Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_NodeAnalyticalTags = -2000956,
        [DisplayText("Analytical Nodes")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalNodes = -2009645,
        [DisplayText("Analytical Opening Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalOpeningTags = -2000958,
        [DisplayText("Analytical Openings")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalOpening = -2009665,
        [DisplayText("Analytical Panel Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalPanelTags = -2000957,
        [DisplayText("Analytical Panels")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_AnalyticalPanel = -2009664,
        [DisplayText("Analytical Pipe Connections")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Piping")]
        OST_AnalyticalPipeConnections = -2000983,
        [DisplayText("Analytical Spaces")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Architecture", "Mechanical")]
        OST_AnalyticSpaces = -2008185,
        [DisplayText("Analytical Surfaces")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Architecture", "Mechanical")]
        OST_AnalyticSurfaces = -2008186,
        [DisplayText("Anchor Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionAnchorTags = -2009057,
        [DisplayText("Approach Slab Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_ApproachSlabTags = -2006211,
        [DisplayText("Area Based Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_ELECTRICAL_AreaBasedLoads_Tags = -2001078,
        [DisplayText("Area Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_AreaLoadTags = -2005252,
        [DisplayText("Area Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_AreaTags = -2005020,
        [DisplayText("Areas")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_Areas = -2003200,
        [DisplayText("Assemblies")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Assemblies = -2000267,
        [DisplayText("Assembly Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_AssemblyTags = -2000268,
        [DisplayText("Audio Visual Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_AudioVisualDeviceTags = -2001057,
        [DisplayText("Audio Visual Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_AudioVisualDevices = -2001055,
        [DisplayText("Bearing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeBearingTags = -2006178,
        [DisplayText("Bearings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeBearings = -2006138,
        [DisplayText("Bolt Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionBoltTags = -2009056,
        [DisplayText("Boundary Conditions")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_BoundaryConditions = -2005301,
        [DisplayText("Brace in Plan View Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructuralBracePlanReps = -2006110,
        [DisplayText("Bridge Cable Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeCableTags = -2006173,
        [DisplayText("Bridge Cables")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeCables = -2006133,
        [DisplayText("Bridge Cross Bracing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingCrossBracingTags = -2006278,
        [DisplayText("Bridge Deck Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeDeckTags = -2006175,
        [DisplayText("Bridge Decks")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeDecks = -2006135,
        [DisplayText("Bridge Diaphragm Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingDiaphragmTags = -2006279,
        [DisplayText("Bridge Framing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingTags = -2006243,
        [DisplayText("Bridge Framing")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFraming = -2006241,
        [DisplayText("Bridge Truss Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFramingTrussTags = -2006281,
        [DisplayText("Cable Tray Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_CableTrayFittingTags = -2008127,
        [DisplayText("Cable Tray Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_CableTrayFitting = -2008126,
        [DisplayText("Cable Tray Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_CableTrayTags = -2008131,
        [DisplayText("Cable Trays")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_CableTray = -2008130,
        [DisplayText("Callouts")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Callouts = -2000537,
        [DisplayText("Casework Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CaseworkTags = -2005001,
        [DisplayText("Casework")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Casework = -2001000,
        [DisplayText("Ceiling Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CeilingTags = -2005002,
        [DisplayText("Ceilings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Ceilings = -2000038,
        [DisplayText("Color Fill Legends")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_ColorFillLegends = -2000550,
        [DisplayText("Column Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_ColumnTags = -2001063,
        [DisplayText("Columns")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Columns = -2000100,
        [DisplayText("Communication Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_CommunicationDeviceTags = -2008082,
        [DisplayText("Communication Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_CommunicationDevices = -2008081,
        [DisplayText("Conduit Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_ConduitFittingTags = -2008129,
        [DisplayText("Conduit Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_ConduitFitting = -2008128,
        [DisplayText("Conduit Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_ConduitTags = -2008133,
        [DisplayText("Conduits")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_Conduit = -2008132,
        [DisplayText("Connection Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructConnectionSymbols = -2006100,
        [DisplayText("Contour Labels")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_ContourLabels = -2000350,
        [DisplayText("Curtain Panel Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallPanelTags = -2005012,
        [DisplayText("Curtain Panels")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallPanels = -2000170,
        [DisplayText("Curtain System Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CurtaSystemTags = -2005025,
        [DisplayText("Curtain Systems")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_CurtaSystem = -2000340,
        [DisplayText("Curtain Wall Mullion Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallMullionTags = -2005032,
        [DisplayText("Curtain Wall Mullions")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_CurtainWallMullions = -2000171,
        [DisplayText("Data Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_DataDeviceTags = -2008084,
        [DisplayText("Data Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_DataDevices = -2008083,
        [DisplayText("Detail Item Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_DetailComponentTags = -2005028,
        [DisplayText("Detail Items")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_DetailComponents = -2002000,
        [DisplayText("Dimensions")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Dimensions = -2000260,
        [DisplayText("Displacement Path")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_DisplacementPath = -2000223,
        [DisplayText("Door Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_DoorTags = -2000460,
        [DisplayText("Doors")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Doors = -2000023,
        [DisplayText("Duct Accessories")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctAccessory = -2008016,
        [DisplayText("Duct Accessory Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctAccessoryTags = -2008017,
        [DisplayText("Duct Color Fill Legends")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctColorFillLegends = -2007004,
        [DisplayText("Duct Color Fill")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctColorFills = -2008005,
        [DisplayText("Duct Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctFittingTags = -2008061,
        [DisplayText("Duct Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctFitting = -2008010,
        [DisplayText("Duct Insulation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctInsulationsTags = -2008153,
        [DisplayText("Duct Insulations")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctInsulations = -2008123,
        [DisplayText("Duct Lining Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctLiningsTags = -2008154,
        [DisplayText("Duct Linings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctLinings = -2008124,
        [DisplayText("Duct Placeholders")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_PlaceHolderDucts = -2008160,
        [DisplayText("Duct Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctTags = -2008003,
        [DisplayText("Ducts")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_DuctCurves = -2008000,
        [DisplayText("Electrical Analytical Loads")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Electrical")]
        OST_ElectricalLoadZoneInstance = -2001020,
        [DisplayText("Electrical Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalEquipmentTags = -2005003,
        [DisplayText("Electrical Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalEquipment = -2001040,
        [DisplayText("Electrical Fixture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalFixtureTags = -2005004,
        [DisplayText("Electrical Fixtures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Electrical")]
        OST_ElectricalFixtures = -2001060,
        [DisplayText("Elevations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Elev = -2000535,
        [DisplayText("Entourage Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_EntourageTags = -2001064,
        [DisplayText("Entourage")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Entourage = -2001370,
        [DisplayText("Expansion Joint Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_ExpansionJointTags = -2006273,
        [DisplayText("Expansion Joints")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_ExpansionJoints = -2006271,
        [DisplayText("Fascia Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_FasciaTags = -2001062,
        [DisplayText("Fire Alarm Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_FireAlarmDeviceTags = -2008086,
        [DisplayText("Fire Alarm Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_FireAlarmDevices = -2008085,
        [DisplayText("Fire Protection Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FireProtectionTags = -2001051,
        [DisplayText("Fire Protection")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FireProtection = -2001049,
        [DisplayText("Flex Duct Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_FlexDuctTags = -2008004,
        [DisplayText("Flex Ducts")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_FlexDuctCurves = -2008020,
        [DisplayText("Flex Pipe Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_FlexPipeTags = -2008048,
        [DisplayText("Flex Pipes")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_FlexPipeCurves = -2008050,
        [DisplayText("Floor Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_FloorTags = -2005026,
        [DisplayText("Floors")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Floors = -2000032,
        [DisplayText("Food Service Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FoodServiceEquipmentTags = -2001045,
        [DisplayText("Food Service Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_FoodServiceEquipment = -2001043,
        [DisplayText("Foundation Span Direction Symbol")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_FootingSpanDirectionSymbol = -2005111,
        [DisplayText("Furniture System Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_FurnitureSystemTags = -2005007,
        [DisplayText("Furniture Systems")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_FurnitureSystems = -2001100,
        [DisplayText("Furniture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_FurnitureTags = -2005006,
        [DisplayText("Furniture")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Furniture = -2000080,
        [DisplayText("Generic Annotations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_GenericAnnotation = -2000150,
        [DisplayText("Generic Model Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_GenericModelTags = -2005013,
        [DisplayText("Generic Models")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_GenericModel = -2000151,
        [DisplayText("Grids")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Grids = -2000220,
        [DisplayText("Guide Grid")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_GuideGrid = -2000107,
        [DisplayText("Gutter Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_GutterTags = -2001065,
        [DisplayText("Handrail Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_HandrailTags = -2001066,
        [DisplayText("Hardscape Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_HardscapeTags = -2001038,
        [DisplayText("Hardscape")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Hardscape = -2001036,
        [DisplayText("Hole Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionHoleTags = -2009063,
        [DisplayText("HVAC Zones")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_HVAC_Zones = -2008107,
        [DisplayText("Internal Area Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_InternalAreaLoadTags = -2005255,
        [DisplayText("Internal Line Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_InternalLineLoadTags = -2005254,
        [DisplayText("Internal Point Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_InternalPointLoadTags = -2005253,
        [DisplayText("Keynote Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_KeynoteTags = -2005029,
        [DisplayText("Levels")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Levels = -2000240,
        [DisplayText("Lighting Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_LightingDeviceTags = -2008088,
        [DisplayText("Lighting Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_LightingDevices = -2008087,
        [DisplayText("Lighting Fixture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Electrical", "Infrastructure")]
        OST_LightingFixtureTags = -2005008,
        [DisplayText("Lighting Fixtures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Electrical", "Infrastructure")]
        OST_LightingFixtures = -2001120,
        [DisplayText("Line Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_LineLoadTags = -2005251,
        [DisplayText("Lines")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Lines = -2000051,
        [DisplayText("Mass Floor Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_MassAreaFaceTags = -2003410,
        [DisplayText("Mass Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_MassTags = -2003405,
        [DisplayText("Mass")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Mass = -2003400,
        [DisplayText("Matchline")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Matchline = -2000193,
        [DisplayText("Material Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_MaterialTags = -2005027,
        [DisplayText("Mechanical Control Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical")]
        OST_MechanicalControlDeviceTags = -2008233,
        [DisplayText("Mechanical Control Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical")]
        OST_MechanicalControlDevices = -2008232,
        [DisplayText("Mechanical Equipment Set Boundary Lines")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Piping")]
        OST_MechanicalEquipmentSetBoundaryLines = -2000987,
        [DisplayText("Mechanical Equipment Set Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Piping")]
        OST_MechanicalEquipmentSetTags = -2000986,
        [DisplayText("Mechanical Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_MechanicalEquipmentTags = -2005009,
        [DisplayText("Mechanical Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_MechanicalEquipment = -2001140,
        [DisplayText("Medical Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_MedicalEquipmentTags = -2001048,
        [DisplayText("Medical Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_MedicalEquipment = -2001046,
        [DisplayText("MEP Fabrication Containment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_FabricationContainmentTags = -2008213,
        [DisplayText("MEP Fabrication Containment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_FabricationContainment = -2008212,
        [DisplayText("MEP Fabrication Ductwork Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_FabricationDuctworkTags = -2008194,
        [DisplayText("MEP Fabrication Ductwork")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical")]
        OST_FabricationDuctwork = -2008193,
        [DisplayText("MEP Fabrication Hanger Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_FabricationHangerTags = -2008204,
        [DisplayText("MEP Fabrication Hangers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_FabricationHangers = -2008203,
        [DisplayText("MEP Fabrication Pipework Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_FabricationPipeworkTags = -2008209,
        [DisplayText("MEP Fabrication Pipework")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_FabricationPipework = -2008208,
        [DisplayText("Model Group Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_ModelGroupTags = -2001073,
        [DisplayText("Model Groups")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_IOSModelGroups = -2000095,
        [DisplayText("Multi Leader Tag")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_MultiLeaderTag = -2005033,
        [DisplayText("Multi-Category Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_MultiCategoryTags = -2005022,
        [DisplayText("Nurse Call Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_NurseCallDeviceTags = -2008078,
        [DisplayText("Nurse Call Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_NurseCallDevices = -2008077,
        [DisplayText("Pad Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Infrastructure")]
        OST_PadTags = -2001067,
        [DisplayText("Panel Schedule Graphics")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_PanelScheduleGraphics = -2008151,
        [DisplayText("Parking Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_ParkingTags = -2005017,
        [DisplayText("Parking")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Parking = -2001180,
        [DisplayText("Part Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_PartTags = -2000270,
        [DisplayText("Parts")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Parts = -2000269,
        [DisplayText("Path of Travel Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_PathOfTravelTags = -2000834,
        [DisplayText("Pier Cap Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierCapTags = -2006220,
        [DisplayText("Pier Column Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierColumnTags = -2006222,
        [DisplayText("Pier Foundation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeFoundationTags = -2006176,
        [DisplayText("Pier Pile Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierPileTags = -2006226,
        [DisplayText("Pier Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgePierTags = -2006171,
        [DisplayText("Pier Tower Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgeTowerTags = -2006172,
        [DisplayText("Pier Wall Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_PierWallTags = -2006230,
        [DisplayText("Piers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_BridgePiers = -2006131,
        [DisplayText("Pipe Accessories")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeAccessory = -2008055,
        [DisplayText("Pipe Accessory Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeAccessoryTags = -2008056,
        [DisplayText("Pipe Color Fill Legends")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeColorFillLegends = -2008058,
        [DisplayText("Pipe Color Fill")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeColorFills = -2008059,
        [DisplayText("Pipe Fitting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeFittingTags = -2008060,
        [DisplayText("Pipe Fittings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeFitting = -2008049,
        [DisplayText("Pipe Insulation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeInsulationsTags = -2008155,
        [DisplayText("Pipe Insulations")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeInsulations = -2008122,
        [DisplayText("Pipe Placeholders")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PlaceHolderPipes = -2008161,
        [DisplayText("Pipe Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_PipeTags = -2008047,
        [DisplayText("Pipes")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_PipeCurves = -2008044,
        [DisplayText("Plan Region")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_PlanRegion = -2000191,
        [DisplayText("Planting Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_PlantingTags = -2005021,
        [DisplayText("Planting")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Planting = -2001360,
        [DisplayText("Plate Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionPlateTags = -2009055,
        [DisplayText("Plumbing Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_PlumbingEquipmentTags = -2008235,
        [DisplayText("Plumbing Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Piping")]
        OST_PlumbingEquipment = -2008234,
        [DisplayText("Plumbing Fixture Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Piping")]
        OST_PlumbingFixtureTags = -2005010,
        [DisplayText("Plumbing Fixtures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Piping")]
        OST_PlumbingFixtures = -2001160,
        [DisplayText("Point Load Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_PointLoadTags = -2005250,
        [DisplayText("Profile Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionProfilesTags = -2009064,
        [DisplayText("Property Line Segment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SitePropertyLineSegmentTags = -2001269,
        [DisplayText("Property Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SitePropertyTags = -2001267,
        [DisplayText("Railing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StairsRailingTags = -2000133,
        [DisplayText("Railings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Infrastructure")]
        OST_StairsRailing = -2000126,
        [DisplayText("Ramp Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_RampTags = -2001068,
        [DisplayText("Ramps")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Ramps = -2000180,
        [DisplayText("Raster Images")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_RasterImages = -2000560,
        [DisplayText("Reference Lines")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ReferenceLines = -2000083,
        [DisplayText("Reference Planes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_CLines = -2000530,
        [DisplayText("Reference Points")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ReferencePoints = -2000710,
        [DisplayText("Revision Cloud Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_RevisionCloudTags = -2006080,
        [DisplayText("Revision Clouds")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_RevisionClouds = -2006060,
        [DisplayText("Revision")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Revisions = -2006070,
        [DisplayText("Road Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_RoadTags = -2001221,
        [DisplayText("Roads")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Roads = -2001220,
        [DisplayText("Roof Soffit Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_RoofSoffitTags = -2001069,
        [DisplayText("Roof Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_RoofTags = -2000266,
        [DisplayText("Roofs")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Roofs = -2000035,
        [DisplayText("Room Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_RoomTags = -2000480,
        [DisplayText("Rooms")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Mechanical", "Electrical", "Piping")]
        OST_Rooms = -2000160,
        [DisplayText("RVT Link Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_RvtLinksTags = -2001074,
        [DisplayText("Schedule Graphics")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ScheduleGraphics = -2000570,
        [DisplayText("Schedules")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Schedules = -2000573,
        [DisplayText("Scope Boxes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_VolumeOfInterest = -2006000,
        [DisplayText("Section Boxes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_SectionBox = -2000301,
        [DisplayText("Sections")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_Sections = -2000200,
        [DisplayText("Security Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_SecurityDeviceTags = -2008080,
        [DisplayText("Security Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_SecurityDevices = -2008079,
        [DisplayText("Shaft Openings")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_ShaftOpening = -2000996,
        [DisplayText("Shear Stud Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionShearStudTags = -2009058,
        [DisplayText("Signage Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_SignageTags = -2001061,
        [DisplayText("Signage")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Signage = -2001058,
        [DisplayText("Site Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SiteTags = -2005016,
        [DisplayText("Site")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Infrastructure")]
        OST_Site = -2001260,
        [DisplayText("Slab Edge Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SlabEdgeTags = -2001070,
        [DisplayText("Space Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_MEPSpaceTags = -2000485,
        [DisplayText("Spaces")]
        [CategoryType("Model")]
        [CategoryDiscipline("Mechanical", "Electrical", "Piping")]
        OST_MEPSpaces = -2003600,
        [DisplayText("Span Direction Symbol")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_SpanDirectionSymbol = -2005110,
        [DisplayText("Specialty Equipment Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_SpecialityEquipmentTags = -2005014,
        [DisplayText("Specialty Equipment")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_SpecialityEquipment = -2001350,
        [DisplayText("Spot Coordinates")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SpotCoordinates = -2000264,
        [DisplayText("Spot Elevations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SpotElevations = -2000263,
        [DisplayText("Spot Slopes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_SpotSlopes = -2000265,
        [DisplayText("Sprinkler Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Piping")]
        OST_SprinklerTags = -2008100,
        [DisplayText("Sprinklers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Piping")]
        OST_Sprinklers = -2008099,
        [DisplayText("Stair Landing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsLandingTags = -2000941,
        [DisplayText("Stair Paths")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsPaths = -2000938,
        [DisplayText("Stair Run Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsRunTags = -2000940,
        [DisplayText("Stair Support Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsSupportTags = -2000942,
        [DisplayText("Stair Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsTags = -2005023,
        [DisplayText("Stair Tread/Riser Numbers")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StairsTriserNumbers = -2000944,
        [DisplayText("Stairs")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_Stairs = -2000120,
        [DisplayText("Structural Annotations")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructuralAnnotations = -2006090,
        [DisplayText("Structural Area Reinforcement Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_AreaReinSpanSymbol = -2009005,
        [DisplayText("Structural Area Reinforcement Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_AreaReinTags = -2009021,
        [DisplayText("Structural Area Reinforcement")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_AreaRein = -2009003,
        [DisplayText("Structural Beam System Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_BeamSystemTags = -2005130,
        [DisplayText("Structural Beam Systems")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure")]
        OST_StructuralFramingSystem = -2001327,
        [DisplayText("Structural Column Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralColumnTags = -2005018,
        [DisplayText("Structural Columns")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralColumns = -2001330,
        [DisplayText("Structural Connection Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_StructConnectionTags = -2009040,
        [DisplayText("Structural Connections")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructConnections = -2009030,
        [DisplayText("Structural Fabric Areas")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricAreas = -2009017,
        [DisplayText("Structural Fabric Reinforcement Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricReinSpanSymbol = -2009028,
        [DisplayText("Structural Fabric Reinforcement Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricReinforcementTags = -2009022,
        [DisplayText("Structural Fabric Reinforcement")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_FabricReinforcement = -2009016,
        [DisplayText("Structural Foundation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFoundationTags = -2005019,
        [DisplayText("Structural Foundations")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFoundation = -2001300,
        [DisplayText("Structural Framing Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFramingTags = -2005015,
        [DisplayText("Structural Framing")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralFraming = -2001320,
        [DisplayText("Structural Internal Loads")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_InternalLoads = -2005204,
        [DisplayText("Structural Load Cases")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_LoadCases = -2005210,
        [DisplayText("Structural Loads")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Structure")]
        OST_Loads = -2005200,
        [DisplayText("Structural Path Reinforcement Symbols")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_PathReinSpanSymbol = -2009010,
        [DisplayText("Structural Path Reinforcement Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_PathReinTags = -2009011,
        [DisplayText("Structural Path Reinforcement")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_PathRein = -2009009,
        [DisplayText("Structural Rebar Coupler Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_CouplerTags = -2009061,
        [DisplayText("Structural Rebar Couplers")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_Coupler = -2009060,
        [DisplayText("Structural Rebar Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_RebarTags = -2009020,
        [DisplayText("Structural Rebar")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_Rebar = -2009000,
        [DisplayText("Structural Stiffener Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure", "Infrastructure")]
        OST_StructuralStiffenerTags = -2001355,
        [DisplayText("Structural Stiffeners")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_StructuralStiffener = -2001354,
        [DisplayText("Structural Tendon Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_StructuralTendonTags = -2006276,
        [DisplayText("Structural Tendons")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_StructuralTendons = -2006274,
        [DisplayText("Structural Truss Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_TrussTags = -2005030,
        [DisplayText("Structural Trusses")]
        [CategoryType("Model")]
        [CategoryDiscipline("Structure")]
        OST_StructuralTruss = -2001336,
        [DisplayText("System-Zone Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical", "Piping")]
        OST_MEPSystemZoneTags = -2001007,
        [DisplayText("System-Zones")]
        [CategoryType("Analytical")]
        [CategoryDiscipline("Mechanical")]
        OST_MEPSystemZone = -2001001,
        [DisplayText("Telephone Device Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_TelephoneDeviceTags = -2008076,
        [DisplayText("Telephone Devices")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_TelephoneDevices = -2008075,
        [DisplayText("Temporary Structure Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_TemporaryStructureTags = -2001042,
        [DisplayText("Temporary Structures")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_TemporaryStructure = -2001039,
        [DisplayText("Text Notes")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_TextNotes = -2000300,
        [DisplayText("Title Blocks")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_TitleBlocks = -2000280,
        [DisplayText("Top Rail Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_TopRailTags = -2001071,
        [DisplayText("Topography")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Topography = -2001340,
        [DisplayText("Vertical Circulation Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_VerticalCirculationTags = -2001054,
        [DisplayText("Vertical Circulation")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_VerticalCirculation = -2001052,
        [DisplayText("Vibration Damper Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationDamperTags = -2006264,
        [DisplayText("Vibration Isolator Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationIsolatorTags = -2006266,
        [DisplayText("Vibration Management Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationManagementTags = -2006282,
        [DisplayText("Vibration Management")]
        [CategoryType("Model")]
        [CategoryDiscipline("Infrastructure")]
        OST_VibrationManagement = -2006261,
        [DisplayText("View Reference")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ReferenceViewer = -2000198,
        [DisplayText("View Titles")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping")]
        OST_ViewportLabel = -2000515,
        [DisplayText("Views")]
        [CategoryType("Internal")]
        [CategoryDiscipline("Architecture", "Structure", "Mechanical", "Electrical", "Piping", "Infrastructure")]
        OST_Views = -2000279,
        [DisplayText("Wall Sweep Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_WallSweepTags = -2001072,
        [DisplayText("Wall Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_WallTags = -2005011,
        [DisplayText("Walls")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture", "Structure", "Infrastructure")]
        OST_Walls = -2000011,
        [DisplayText("Weld Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Structure")]
        OST_StructConnectionWeldTags = -2009059,
        [DisplayText("Window Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Architecture")]
        OST_WindowTags = -2000450,
        [DisplayText("Windows")]
        [CategoryType("Model")]
        [CategoryDiscipline("Architecture")]
        OST_Windows = -2000014,
        [DisplayText("Wire Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Electrical")]
        OST_WireTags = -2008057,
        [DisplayText("Wires")]
        [CategoryType("Model")]
        [CategoryDiscipline("Electrical")]
        OST_Wire = -2008039,
        [DisplayText("Zone Tags")]
        [CategoryType("Annotation")]
        [CategoryDiscipline("Mechanical")]
        OST_ZoneTags = -2008115
    }
}
