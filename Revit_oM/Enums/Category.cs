using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.Revit.Enums
{
    public enum Category
    {
        [DisplayText("Point Clouds")]
        OST_PointClouds = -2010001,
        [DisplayText("Analytical Links")]
        OST_LinksAnalytical = -2009657,
        [DisplayText("Analytical Slab Foundation Tags")]
        OST_FoundationSlabAnalyticalTags = -2009656,
        [DisplayText("Analytical Wall Foundation Tags")]
        OST_WallFoundationAnalyticalTags = -2009655,
        [DisplayText("Analytical Isolated Foundation Tags")]
        OST_IsolatedFoundationAnalyticalTags = -2009654,
        [DisplayText("Analytical Wall Tags")]
        OST_WallAnalyticalTags = -2009653,
        [DisplayText("Analytical Floor Tags")]
        OST_FloorAnalyticalTags = -2009652,
        [DisplayText("Analytical Column Tags")]
        OST_ColumnAnalyticalTags = -2009651,
        [DisplayText("Analytical Brace Tags")]
        OST_BraceAnalyticalTags = -2009650,
        [DisplayText("Analytical Beam Tags")]
        OST_BeamAnalyticalTags = -2009649,
        [DisplayText("Analytical Nodes")]
        OST_AnalyticalNodes = -2009645,
        [DisplayText("Analytical Foundation Slabs")]
        OST_FoundationSlabAnalytical = -2009643,
        [DisplayText("Analytical Wall Foundations")]
        OST_WallFoundationAnalytical = -2009642,
        [DisplayText("Analytical Isolated Foundations")]
        OST_IsolatedFoundationAnalytical = -2009641,
        [DisplayText("Analytical Walls")]
        OST_WallAnalytical = -2009640,
        [DisplayText("Analytical Floors")]
        OST_FloorAnalytical = -2009639,
        [DisplayText("Analytical Columns")]
        OST_ColumnAnalytical = -2009636,
        [DisplayText("Analytical Braces")]
        OST_BraceAnalytical = -2009633,
        [DisplayText("Analytical Beams")]
        OST_BeamAnalytical = -2009630,
        [DisplayText("Profile Tags")]
        OST_StructConnectionProfilesTags = -2009064,
        [DisplayText("Hole Tags")]
        OST_StructConnectionHoleTags = -2009063,
        [DisplayText("Structural Rebar Coupler Tags")]
        OST_CouplerTags = -2009061,
        [DisplayText("Structural Rebar Couplers")]
        OST_Coupler = -2009060,
        [DisplayText("Weld Tags")]
        OST_StructConnectionWeldTags = -2009059,
        [DisplayText("Shear Stud Tags")]
        OST_StructConnectionShearStudTags = -2009058,
        [DisplayText("Anchor Tags")]
        OST_StructConnectionAnchorTags = -2009057,
        [DisplayText("Bolt Tags")]
        OST_StructConnectionBoltTags = -2009056,
        [DisplayText("Plate Tags")]
        OST_StructConnectionPlateTags = -2009055,
        [DisplayText("Structural Connection Tags")]
        OST_StructConnectionTags = -2009040,
        [DisplayText("Structural Connections")]
        OST_StructConnections = -2009030,
        [DisplayText("Structural Fabric Reinforcement Symbols")]
        OST_FabricReinSpanSymbol = -2009028,
        [DisplayText("Rebar Set Toggle")]
        OST_RebarSetToggle = -2009025,
        [DisplayText("Structural Fabric Reinforcement Tags")]
        OST_FabricReinforcementTags = -2009022,
        [DisplayText("Structural Area Reinforcement Tags")]
        OST_AreaReinTags = -2009021,
        [DisplayText("Structural Rebar Tags")]
        OST_RebarTags = -2009020,
        [DisplayText("Structural Fabric Areas")]
        OST_FabricAreas = -2009017,
        [DisplayText("Structural Fabric Reinforcement")]
        OST_FabricReinforcement = -2009016,
        [DisplayText("Rebar Cover References")]
        OST_RebarCover = -2009015,
        [DisplayText("Rebar Shape")]
        OST_RebarShape = -2009013,
        [DisplayText("Structural Path Reinforcement Tags")]
        OST_PathReinTags = -2009011,
        [DisplayText("Structural Path Reinforcement Symbols")]
        OST_PathReinSpanSymbol = -2009010,
        [DisplayText("Structural Path Reinforcement")]
        OST_PathRein = -2009009,
        [DisplayText("Structural Area Reinforcement Symbols")]
        OST_AreaReinSpanSymbol = -2009005,
        [DisplayText("Structural Area Reinforcement")]
        OST_AreaRein = -2009003,
        [DisplayText("Structural Rebar")]
        OST_Rebar = -2009000,
        [DisplayText("MEP Fabrication Containment Tags")]
        OST_FabricationContainmentTags = -2008213,
        [DisplayText("MEP Fabrication Containment")]
        OST_FabricationContainment = -2008212,
        [DisplayText("MEP Fabrication Pipework Tags")]
        OST_FabricationPipeworkTags = -2008209,
        [DisplayText("MEP Fabrication Pipework")]
        OST_FabricationPipework = -2008208,
        [DisplayText("MEP Fabrication Hanger Tags")]
        OST_FabricationHangerTags = -2008204,
        [DisplayText("MEP Fabrication Hangers")]
        OST_FabricationHangers = -2008203,
        [DisplayText("MEP Fabrication Ductwork Tags")]
        OST_FabricationDuctworkTags = -2008194,
        [DisplayText("MEP Fabrication Ductwork")]
        OST_FabricationDuctwork = -2008193,
        [DisplayText("Analytical Surfaces")]
        OST_AnalyticSurfaces = -2008186,
        [DisplayText("Analytical Spaces")]
        OST_AnalyticSpaces = -2008185,
        [DisplayText("Pipe Segments")]
        OST_PipeSegments = -2008163,
        [DisplayText("Pipe Placeholders")]
        OST_PlaceHolderPipes = -2008161,
        [DisplayText("Duct Placeholders")]
        OST_PlaceHolderDucts = -2008160,
        [DisplayText("Pipe Insulation Tags")]
        OST_PipeInsulationsTags = -2008155,
        [DisplayText("Duct Lining Tags")]
        OST_DuctLiningsTags = -2008154,
        [DisplayText("Duct Insulation Tags")]
        OST_DuctInsulationsTags = -2008153,
        [DisplayText("Electrical Spare/Space Circuits")]
        OST_ElectricalInternalCircuits = -2008152,
        [DisplayText("Panel Schedule Graphics")]
        OST_PanelScheduleGraphics = -2008151,
        [DisplayText("Cable Tray Runs")]
        OST_CableTrayRun = -2008150,
        [DisplayText("Conduit Runs")]
        OST_ConduitRun = -2008149,
        [DisplayText("Conduit Tags")]
        OST_ConduitTags = -2008133,
        [DisplayText("Conduits")]
        OST_Conduit = -2008132,
        [DisplayText("Cable Tray Tags")]
        OST_CableTrayTags = -2008131,
        [DisplayText("Cable Trays")]
        OST_CableTray = -2008130,
        [DisplayText("Conduit Fitting Tags")]
        OST_ConduitFittingTags = -2008129,
        [DisplayText("Conduit Fittings")]
        OST_ConduitFitting = -2008128,
        [DisplayText("Cable Tray Fitting Tags")]
        OST_CableTrayFittingTags = -2008127,
        [DisplayText("Cable Tray Fittings")]
        OST_CableTrayFitting = -2008126,
        [DisplayText("Routing Preferences")]
        OST_RoutingPreferences = -2008125,
        [DisplayText("Duct Linings")]
        OST_DuctLinings = -2008124,
        [DisplayText("Duct Insulations")]
        OST_DuctInsulations = -2008123,
        [DisplayText("Pipe Insulations")]
        OST_PipeInsulations = -2008122,
        [DisplayText("Zone Tags")]
        OST_ZoneTags = -2008115,
        [DisplayText("HVAC Zones")]
        OST_HVAC_Zones = -2008107,
        [DisplayText("Switch System")]
        OST_SwitchSystem = -2008101,
        [DisplayText("Sprinkler Tags")]
        OST_SprinklerTags = -2008100,
        [DisplayText("Sprinklers")]
        OST_Sprinklers = -2008099,
        [DisplayText("Lighting Device Tags")]
        OST_LightingDeviceTags = -2008088,
        [DisplayText("Lighting Devices")]
        OST_LightingDevices = -2008087,
        [DisplayText("Fire Alarm Device Tags")]
        OST_FireAlarmDeviceTags = -2008086,
        [DisplayText("Fire Alarm Devices")]
        OST_FireAlarmDevices = -2008085,
        [DisplayText("Data Device Tags")]
        OST_DataDeviceTags = -2008084,
        [DisplayText("Data Devices")]
        OST_DataDevices = -2008083,
        [DisplayText("Communication Device Tags")]
        OST_CommunicationDeviceTags = -2008082,
        [DisplayText("Communication Devices")]
        OST_CommunicationDevices = -2008081,
        [DisplayText("Security Device Tags")]
        OST_SecurityDeviceTags = -2008080,
        [DisplayText("Security Devices")]
        OST_SecurityDevices = -2008079,
        [DisplayText("Nurse Call Device Tags")]
        OST_NurseCallDeviceTags = -2008078,
        [DisplayText("Nurse Call Devices")]
        OST_NurseCallDevices = -2008077,
        [DisplayText("Telephone Device Tags")]
        OST_TelephoneDeviceTags = -2008076,
        [DisplayText("Telephone Devices")]
        OST_TelephoneDevices = -2008075,
        [DisplayText("Duct Fitting Tags")]
        OST_DuctFittingTags = -2008061,
        [DisplayText("Pipe Fitting Tags")]
        OST_PipeFittingTags = -2008060,
        [DisplayText("Pipe Color Fill")]
        OST_PipeColorFills = -2008059,
        [DisplayText("Pipe Color Fill Legends")]
        OST_PipeColorFillLegends = -2008058,
        [DisplayText("Wire Tags")]
        OST_WireTags = -2008057,
        [DisplayText("Pipe Accessory Tags")]
        OST_PipeAccessoryTags = -2008056,
        [DisplayText("Pipe Accessories")]
        OST_PipeAccessory = -2008055,
        [DisplayText("Flex Pipes")]
        OST_FlexPipeCurves = -2008050,
        [DisplayText("Pipe Fittings")]
        OST_PipeFitting = -2008049,
        [DisplayText("Flex Pipe Tags")]
        OST_FlexPipeTags = -2008048,
        [DisplayText("Pipe Tags")]
        OST_PipeTags = -2008047,
        [DisplayText("Pipes")]
        OST_PipeCurves = -2008044,
        [DisplayText("Piping Systems")]
        OST_PipingSystem = -2008043,
        [DisplayText("Wires")]
        OST_Wire = -2008039,
        [DisplayText("Electrical Circuits")]
        OST_ElectricalCircuit = -2008037,
        [DisplayText("Flex Ducts")]
        OST_FlexDuctCurves = -2008020,
        [DisplayText("Duct Accessory Tags")]
        OST_DuctAccessoryTags = -2008017,
        [DisplayText("Duct Accessories")]
        OST_DuctAccessory = -2008016,
        [DisplayText("Duct Systems")]
        OST_DuctSystem = -2008015,
        [DisplayText("Air Terminal Tags")]
        OST_DuctTerminalTags = -2008014,
        [DisplayText("Air Terminals")]
        OST_DuctTerminal = -2008013,
        [DisplayText("Duct Fittings")]
        OST_DuctFitting = -2008010,
        [DisplayText("Duct Color Fill")]
        OST_DuctColorFills = -2008005,
        [DisplayText("Flex Duct Tags")]
        OST_FlexDuctTags = -2008004,
        [DisplayText("Duct Tags")]
        OST_DuctTags = -2008003,
        [DisplayText("Ducts")]
        OST_DuctCurves = -2008000,
        [DisplayText("Duct Color Fill Legends")]
        OST_DuctColorFillLegends = -2007004,
        [DisplayText("Structural Tendon Tags")]
        OST_StructuralTendonTags = -2006276,
        [DisplayText("Structural Tendons")]
        OST_StructuralTendons = -2006274,
        [DisplayText("Expansion Joint Tags")]
        OST_ExpansionJointTags = -2006273,
        [DisplayText("Expansion Joints")]
        OST_ExpansionJoints = -2006271,
        [DisplayText("Vibration Isolator Tags")]
        OST_VibrationIsolatorTags = -2006266,
        [DisplayText("Vibration Damper Tags")]
        OST_VibrationDamperTags = -2006264,
        [DisplayText("Vibration Management")]
        OST_VibrationManagement = -2006261,
        [DisplayText("Bridge Framing Tags")]
        OST_BridgeFramingTags = -2006243,
        [DisplayText("Bridge Framing")]
        OST_BridgeFraming = -2006241,
        [DisplayText("Pier Wall Tags")]
        OST_PierWallTags = -2006230,
        [DisplayText("Pier Pile Tags")]
        OST_PierPileTags = -2006226,
        [DisplayText("Pier Column Tags")]
        OST_PierColumnTags = -2006222,
        [DisplayText("Pier Cap Tags")]
        OST_PierCapTags = -2006220,
        [DisplayText("Approach Slab Tags")]
        OST_ApproachSlabTags = -2006211,
        [DisplayText("Abutment Wall Tags")]
        OST_AbutmentWallTags = -2006210,
        [DisplayText("Abutment Pile Tags")]
        OST_AbutmentPileTags = -2006209,
        [DisplayText("Abutment Foundation Tags")]
        OST_AbutmentFoundationTags = -2006208,
        [DisplayText("Bearing Tags")]
        OST_BridgeBearingTags = -2006178,
        [DisplayText("Pier Foundation Tags")]
        OST_BridgeFoundationTags = -2006176,
        [DisplayText("Bridge Deck Tags")]
        OST_BridgeDeckTags = -2006175,
        [DisplayText("Bridge Cable Tags")]
        OST_BridgeCableTags = -2006173,
        [DisplayText("Pier Tower Tags")]
        OST_BridgeTowerTags = -2006172,
        [DisplayText("Pier Tags")]
        OST_BridgePierTags = -2006171,
        [DisplayText("Abutment Tags")]
        OST_BridgeAbutmentTags = -2006170,
        [DisplayText("Bearings")]
        OST_BridgeBearings = -2006138,
        [DisplayText("Bridge Decks")]
        OST_BridgeDecks = -2006135,
        [DisplayText("Bridge Cables")]
        OST_BridgeCables = -2006133,
        [DisplayText("Piers")]
        OST_BridgePiers = -2006131,
        [DisplayText("Abutments")]
        OST_BridgeAbutments = -2006130,
        [DisplayText("Brace in Plan View Symbols")]
        OST_StructuralBracePlanReps = -2006110,
        [DisplayText("Connection Symbols")]
        OST_StructConnectionSymbols = -2006100,
        [DisplayText("Structural Annotations")]
        OST_StructuralAnnotations = -2006090,
        [DisplayText("Revision Cloud Tags")]
        OST_RevisionCloudTags = -2006080,
        [DisplayText("Revision")]
        OST_Revisions = -2006070,
        [DisplayText("Revision Clouds")]
        OST_RevisionClouds = -2006060,
        [DisplayText("Elevation Marks")]
        OST_ElevationMarks = -2006045,
        [DisplayText("Grid Heads")]
        OST_GridHeads = -2006040,
        [DisplayText("Level Heads")]
        OST_LevelHeads = -2006020,
        [DisplayText("Scope Boxes")]
        OST_VolumeOfInterest = -2006000,
        [DisplayText("Boundary Conditions")]
        OST_BoundaryConditions = -2005301,
        [DisplayText("Internal Area Load Tags")]
        OST_InternalAreaLoadTags = -2005255,
        [DisplayText("Internal Line Load Tags")]
        OST_InternalLineLoadTags = -2005254,
        [DisplayText("Internal Point Load Tags")]
        OST_InternalPointLoadTags = -2005253,
        [DisplayText("Area Load Tags")]
        OST_AreaLoadTags = -2005252,
        [DisplayText("Line Load Tags")]
        OST_LineLoadTags = -2005251,
        [DisplayText("Point Load Tags")]
        OST_PointLoadTags = -2005250,
        [DisplayText("Structural Load Cases")]
        OST_LoadCases = -2005210,
        [DisplayText("Structural Internal Loads")]
        OST_InternalLoads = -2005204,
        [DisplayText("Structural Loads")]
        OST_Loads = -2005200,
        [DisplayText("Structural Beam System Tags")]
        OST_BeamSystemTags = -2005130,
        [DisplayText("Foundation Span Direction Symbol")]
        OST_FootingSpanDirectionSymbol = -2005111,
        [DisplayText("Span Direction Symbol")]
        OST_SpanDirectionSymbol = -2005110,
        [DisplayText("Spot Elevation Symbols")]
        OST_SpotElevSymbols = -2005100,
        [DisplayText("Multi Leader Tag")]
        OST_MultiLeaderTag = -2005033,
        [DisplayText("Curtain Wall Mullion Tags")]
        OST_CurtainWallMullionTags = -2005032,
        [DisplayText("Structural Truss Tags")]
        OST_TrussTags = -2005030,
        [DisplayText("Keynote Tags")]
        OST_KeynoteTags = -2005029,
        [DisplayText("Detail Item Tags")]
        OST_DetailComponentTags = -2005028,
        [DisplayText("Material Tags")]
        OST_MaterialTags = -2005027,
        [DisplayText("Floor Tags")]
        OST_FloorTags = -2005026,
        [DisplayText("Curtain System Tags")]
        OST_CurtaSystemTags = -2005025,
        [DisplayText("Stair Tags")]
        OST_StairsTags = -2005023,
        [DisplayText("Multi-Category Tags")]
        OST_MultiCategoryTags = -2005022,
        [DisplayText("Planting Tags")]
        OST_PlantingTags = -2005021,
        [DisplayText("Area Tags")]
        OST_AreaTags = -2005020,
        [DisplayText("Structural Foundation Tags")]
        OST_StructuralFoundationTags = -2005019,
        [DisplayText("Structural Column Tags")]
        OST_StructuralColumnTags = -2005018,
        [DisplayText("Parking Tags")]
        OST_ParkingTags = -2005017,
        [DisplayText("Site Tags")]
        OST_SiteTags = -2005016,
        [DisplayText("Structural Framing Tags")]
        OST_StructuralFramingTags = -2005015,
        [DisplayText("Specialty Equipment Tags")]
        OST_SpecialityEquipmentTags = -2005014,
        [DisplayText("Generic Model Tags")]
        OST_GenericModelTags = -2005013,
        [DisplayText("Curtain Panel Tags")]
        OST_CurtainWallPanelTags = -2005012,
        [DisplayText("Wall Tags")]
        OST_WallTags = -2005011,
        [DisplayText("Plumbing Fixture Tags")]
        OST_PlumbingFixtureTags = -2005010,
        [DisplayText("Mechanical Equipment Tags")]
        OST_MechanicalEquipmentTags = -2005009,
        [DisplayText("Lighting Fixture Tags")]
        OST_LightingFixtureTags = -2005008,
        [DisplayText("Furniture System Tags")]
        OST_FurnitureSystemTags = -2005007,
        [DisplayText("Furniture Tags")]
        OST_FurnitureTags = -2005006,
        [DisplayText("Electrical Fixture Tags")]
        OST_ElectricalFixtureTags = -2005004,
        [DisplayText("Electrical Equipment Tags")]
        OST_ElectricalEquipmentTags = -2005003,
        [DisplayText("Ceiling Tags")]
        OST_CeilingTags = -2005002,
        [DisplayText("Casework Tags")]
        OST_CaseworkTags = -2005001,
        [DisplayText("Spaces")]
        OST_MEPSpaces = -2003600,
        [DisplayText("Mass Floor Tags")]
        OST_MassAreaFaceTags = -2003410,
        [DisplayText("Mass Tags")]
        OST_MassTags = -2003405,
        [DisplayText("Mass")]
        OST_Mass = -2003400,
        [DisplayText("Areas")]
        OST_Areas = -2003200,
        [DisplayText("Project Information")]
        OST_ProjectInformation = -2003101,
        [DisplayText("Sheets")]
        OST_Sheets = -2003100,
        [DisplayText("Detail Items")]
        OST_DetailComponents = -2002000,
        [DisplayText("Entourage")]
        OST_Entourage = -2001370,
        [DisplayText("Planting")]
        OST_Planting = -2001360,
        [DisplayText("Structural Stiffener Tags")]
        OST_StructuralStiffenerTags = -2001355,
        [DisplayText("Structural Stiffeners")]
        OST_StructuralStiffener = -2001354,
        [DisplayText("RVT Links")]
        OST_RvtLinks = -2001352,
        [DisplayText("Specialty Equipment")]
        OST_SpecialityEquipment = -2001350,
        [DisplayText("Topography")]
        OST_Topography = -2001340,
        [DisplayText("Structural Trusses")]
        OST_StructuralTruss = -2001336,
        [DisplayText("Structural Columns")]
        OST_StructuralColumns = -2001330,
        [DisplayText("Structural Beam Systems")]
        OST_StructuralFramingSystem = -2001327,
        [DisplayText("Structural Framing")]
        OST_StructuralFraming = -2001320,
        [DisplayText("Structural Foundations")]
        OST_StructuralFoundation = -2001300,
        [DisplayText("Property Line Segment Tags")]
        OST_SitePropertyLineSegmentTags = -2001269,
        [DisplayText("Property Tags")]
        OST_SitePropertyTags = -2001267,
        [DisplayText("Site")]
        OST_Site = -2001260,
        [DisplayText("Road Tags")]
        OST_RoadTags = -2001221,
        [DisplayText("Roads")]
        OST_Roads = -2001220,
        [DisplayText("Parking")]
        OST_Parking = -2001180,
        [DisplayText("Plumbing Fixtures")]
        OST_PlumbingFixtures = -2001160,
        [DisplayText("Mechanical Equipment")]
        OST_MechanicalEquipment = -2001140,
        [DisplayText("Lighting Fixtures")]
        OST_LightingFixtures = -2001120,
        [DisplayText("Furniture Systems")]
        OST_FurnitureSystems = -2001100,
        [DisplayText("Signage Tags")]
        OST_SignageTags = -2001061,
        [DisplayText("Electrical Fixtures")]
        OST_ElectricalFixtures = -2001060,
        [DisplayText("Signage")]
        OST_Signage = -2001058,
        [DisplayText("Audio Visual Device Tags")]
        OST_AudioVisualDeviceTags = -2001057,
        [DisplayText("Audio Visual Devices")]
        OST_AudioVisualDevices = -2001055,
        [DisplayText("Vertical Circulation Tags")]
        OST_VerticalCirculationTags = -2001054,
        [DisplayText("Vertical Circulation")]
        OST_VerticalCirculation = -2001052,
        [DisplayText("Fire Protection Tags")]
        OST_FireProtectionTags = -2001051,
        [DisplayText("Fire Protection")]
        OST_FireProtection = -2001049,
        [DisplayText("Medical Equipment Tags")]
        OST_MedicalEquipmentTags = -2001048,
        [DisplayText("Medical Equipment")]
        OST_MedicalEquipment = -2001046,
        [DisplayText("Food Service Equipment Tags")]
        OST_FoodServiceEquipmentTags = -2001045,
        [DisplayText("Food Service Equipment")]
        OST_FoodServiceEquipment = -2001043,
        [DisplayText("Temporary Structure Tags")]
        OST_TemporaryStructureTags = -2001042,
        [DisplayText("Electrical Equipment")]
        OST_ElectricalEquipment = -2001040,
        [DisplayText("Temporary Structures")]
        OST_TemporaryStructure = -2001039,
        [DisplayText("Hardscape Tags")]
        OST_HardscapeTags = -2001038,
        [DisplayText("Hardscape")]
        OST_Hardscape = -2001036,
        [DisplayText("Alignment Station Labels")]
        OST_AlignmentStationLabels = -2001017,
        [DisplayText("Alignment Station Label Sets")]
        OST_AlignmentStationLabelSets = -2001016,
        [DisplayText("Alignment Tags")]
        OST_AlignmentsTags = -2001015,
        [DisplayText("Alignments")]
        OST_Alignments = -2001012,
        [DisplayText("Zone Equipment")]
        OST_ZoneEquipment = -2001010,
        [DisplayText("Water Loops")]
        OST_MEPAnalyticalWaterLoop = -2001009,
        [DisplayText("Air Systems")]
        OST_MEPAnalyticalAirLoop = -2001008,
        [DisplayText("System-Zone Tags")]
        OST_MEPSystemZoneTags = -2001007,
        [DisplayText("System-Zones")]
        OST_MEPSystemZone = -2001001,
        [DisplayText("Casework")]
        OST_Casework = -2001000,
        [DisplayText("Shaft Openings")]
        OST_ShaftOpening = -2000996,
        [DisplayText("Mechanical Equipment Set Boundary Lines")]
        OST_MechanicalEquipmentSetBoundaryLines = -2000987,
        [DisplayText("Mechanical Equipment Set Tags")]
        OST_MechanicalEquipmentSetTags = -2000986,
        [DisplayText("Mechanical Equipment Sets")]
        OST_MechanicalEquipmentSet = -2000985,
        [DisplayText("Analytical Pipe Connections")]
        OST_AnalyticalPipeConnections = -2000983,
        [DisplayText("Coordination Model")]
        OST_Coordination_Model = -2000982,
        [DisplayText("Multi-Rebar Annotations")]
        OST_MultiReferenceAnnotations = -2000970,
        [DisplayText("Analytical Node Tags")]
        OST_NodeAnalyticalTags = -2000956,
        [DisplayText("Analytical Link Tags")]
        OST_LinkAnalyticalTags = -2000955,
        [DisplayText("Stair Tread/Riser Numbers")]
        OST_StairsTriserNumbers = -2000944,
        [DisplayText("Stair Support Tags")]
        OST_StairsSupportTags = -2000942,
        [DisplayText("Stair Landing Tags")]
        OST_StairsLandingTags = -2000941,
        [DisplayText("Stair Run Tags")]
        OST_StairsRunTags = -2000940,
        [DisplayText("Stair Paths")]
        OST_StairsPaths = -2000938,
        [DisplayText("Adaptive Points")]
        OST_AdaptivePoints = -2000900,
        [DisplayText("Path of Travel Tags")]
        OST_PathOfTravelTags = -2000834,
        [DisplayText("Reference Points")]
        OST_ReferencePoints = -2000710,
        [DisplayText("Materials")]
        OST_Materials = -2000700,
        [DisplayText("Schedules")]
        OST_Schedules = -2000573,
        [DisplayText("Schedule Graphics")]
        OST_ScheduleGraphics = -2000570,
        [DisplayText("Raster Images")]
        OST_RasterImages = -2000560,
        [DisplayText("Color Fill Legends")]
        OST_ColorFillLegends = -2000550,
        [DisplayText("Annotation Crop Boundary")]
        OST_AnnotationCrop = -2000547,
        [DisplayText("Callout Boundary")]
        OST_CalloutBoundary = -2000539,
        [DisplayText("Callout Heads")]
        OST_CalloutHeads = -2000538,
        [DisplayText("Callouts")]
        OST_Callouts = -2000537,
        [DisplayText("Crop Boundaries")]
        OST_CropBoundary = -2000536,
        [DisplayText("Elevations")]
        OST_Elev = -2000535,
        [DisplayText("Reference Planes")]
        OST_CLines = -2000530,
        [DisplayText("View Titles")]
        OST_ViewportLabel = -2000515,
        [DisplayText("Viewports")]
        OST_Viewports = -2000510,
        [DisplayText("Cameras")]
        OST_Camera_Lines = -2000501,
        [DisplayText("Space Tags")]
        OST_MEPSpaceTags = -2000485,
        [DisplayText("Room Tags")]
        OST_RoomTags = -2000480,
        [DisplayText("Door Tags")]
        OST_DoorTags = -2000460,
        [DisplayText("Window Tags")]
        OST_WindowTags = -2000450,
        [DisplayText("Section Marks")]
        OST_SectionHeads = -2000400,
        [DisplayText("Contour Labels")]
        OST_ContourLabels = -2000350,
        [DisplayText("Curtain Systems")]
        OST_CurtaSystem = -2000340,
        [DisplayText("Analysis Display Style")]
        OST_AnalysisDisplayStyle = -2000304,
        [DisplayText("Analysis Results")]
        OST_AnalysisResults = -2000303,
        [DisplayText("Render Regions")]
        OST_RenderRegions = -2000302,
        [DisplayText("Section Boxes")]
        OST_SectionBox = -2000301,
        [DisplayText("Text Notes")]
        OST_TextNotes = -2000300,
        [DisplayText("Title Blocks")]
        OST_TitleBlocks = -2000280,
        [DisplayText("Views")]
        OST_Views = -2000279,
        [DisplayText("Part Tags")]
        OST_PartTags = -2000270,
        [DisplayText("Parts")]
        OST_Parts = -2000269,
        [DisplayText("Assembly Tags")]
        OST_AssemblyTags = -2000268,
        [DisplayText("Assemblies")]
        OST_Assemblies = -2000267,
        [DisplayText("Roof Tags")]
        OST_RoofTags = -2000266,
        [DisplayText("Spot Slopes")]
        OST_SpotSlopes = -2000265,
        [DisplayText("Spot Coordinates")]
        OST_SpotCoordinates = -2000264,
        [DisplayText("Spot Elevations")]
        OST_SpotElevations = -2000263,
        [DisplayText("Dimensions")]
        OST_Dimensions = -2000260,
        [DisplayText("Levels")]
        OST_Levels = -2000240,
        [DisplayText("Displacement Path")]
        OST_DisplacementPath = -2000223,
        [DisplayText("Grids")]
        OST_Grids = -2000220,
        [DisplayText("Section Line")]
        OST_SectionLine = -2000201,
        [DisplayText("Sections")]
        OST_Sections = -2000200,
        [DisplayText("View Reference")]
        OST_ReferenceViewerSymbol = -2000197,
        [DisplayText("Imports in Families")]
        OST_ImportObjectStyles = -2000196,
        [DisplayText("Masking Region")]
        OST_MaskingRegion = -2000194,
        [DisplayText("Matchline")]
        OST_Matchline = -2000193,
        [DisplayText("Plan Region")]
        OST_PlanRegion = -2000191,
        [DisplayText("Filled region")]
        OST_FilledRegion = -2000190,
        [DisplayText("Ramps")]
        OST_Ramps = -2000180,
        [DisplayText("Curtain Grids")]
        OST_CurtainGrids = -2000173,
        [DisplayText("Curtain Wall Mullions")]
        OST_CurtainWallMullions = -2000171,
        [DisplayText("Curtain Panels")]
        OST_CurtainWallPanels = -2000170,
        [DisplayText("Rooms")]
        OST_Rooms = -2000160,
        [DisplayText("Generic Models")]
        OST_GenericModel = -2000151,
        [DisplayText("Generic Annotations")]
        OST_GenericAnnotation = -2000150,
        [DisplayText("Railing Tags")]
        OST_StairsRailingTags = -2000133,
        [DisplayText("Railings")]
        OST_StairsRailing = -2000126,
        [DisplayText("Stairs")]
        OST_Stairs = -2000120,
        [DisplayText("Guide Grid")]
        OST_GuideGrid = -2000107,
        [DisplayText("Columns")]
        OST_Columns = -2000100,
        [DisplayText("Model Groups")]
        OST_IOSModelGroups = -2000095,
        [DisplayText("Reference Lines")]
        OST_ReferenceLines = -2000083,
        [DisplayText("Furniture")]
        OST_Furniture = -2000080,
        [DisplayText("Lines")]
        OST_Lines = -2000051,
        [DisplayText("Ceilings")]
        OST_Ceilings = -2000038,
        [DisplayText("Roofs")]
        OST_Roofs = -2000035,
        [DisplayText("Floors")]
        OST_Floors = -2000032,
        [DisplayText("Doors")]
        OST_Doors = -2000023,
        [DisplayText("Windows")]
        OST_Windows = -2000014,
        [DisplayText("Walls")]
        OST_Walls = -2000011,
    }
}
