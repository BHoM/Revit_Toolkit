using BH.oM.Base.Attributes;

namespace BH.oM.Revit.Enums
{
    public enum Category
    {
        [DisplayText("Abutment Foundation Tags")]
        OST_AbutmentFoundationTags,
        [DisplayText("Abutment Pile Tags")]
        OST_AbutmentPileTags,
        [DisplayText("Abutment Tags")]
        OST_BridgeAbutmentTags,
        [DisplayText("Abutment Wall Tags")]
        OST_AbutmentWallTags,
        [DisplayText("Abutments")]
        OST_BridgeAbutments,
        [DisplayText("Adaptive Points")]
        OST_AdaptivePoints,
        [DisplayText("Air Systems")]
        OST_MEPAnalyticalAirLoop,
        [DisplayText("Air Terminal Tags")]
        OST_DuctTerminalTags,
        [DisplayText("Air Terminals")]
        OST_DuctTerminal,
        [DisplayText("Alignment Station Label Sets")]
        OST_AlignmentStationLabelSets,
        [DisplayText("Alignment Station Labels")]
        OST_AlignmentStationLabels,
        [DisplayText("Alignment Tags")]
        OST_AlignmentsTags,
        [DisplayText("Alignments")]
        OST_Alignments,
        [DisplayText("Analysis Display Style")]
        OST_AnalysisDisplayStyle,
        [DisplayText("Analysis Results")]
        OST_AnalysisResults,
        [DisplayText("Analytical Beam Tags")]
        OST_BeamAnalyticalTags,
        [DisplayText("Analytical Beams")]
        OST_BeamAnalytical,
        [DisplayText("Analytical Brace Tags")]
        OST_BraceAnalyticalTags,
        [DisplayText("Analytical Braces")]
        OST_BraceAnalytical,
        [DisplayText("Analytical Column Tags")]
        OST_ColumnAnalyticalTags,
        [DisplayText("Analytical Columns")]
        OST_ColumnAnalytical,
        [DisplayText("Analytical Floor Tags")]
        OST_FloorAnalyticalTags,
        [DisplayText("Analytical Floors")]
        OST_FloorAnalytical,
        [DisplayText("Analytical Foundation Slabs")]
        OST_FoundationSlabAnalytical,
        [DisplayText("Analytical Isolated Foundation Tags")]
        OST_IsolatedFoundationAnalyticalTags,
        [DisplayText("Analytical Isolated Foundations")]
        OST_IsolatedFoundationAnalytical,
        [DisplayText("Analytical Link Tags")]
        OST_LinkAnalyticalTags,
        [DisplayText("Analytical Links")]
        OST_LinksAnalytical,
        [DisplayText("Analytical Node Tags")]
        OST_NodeAnalyticalTags,
        [DisplayText("Analytical Nodes")]
        OST_AnalyticalNodes,
        [DisplayText("Analytical Pipe Connections")]
        OST_AnalyticalPipeConnections,
        [DisplayText("Analytical Slab Foundation Tags")]
        OST_FoundationSlabAnalyticalTags,
        [DisplayText("Analytical Spaces")]
        OST_AnalyticSpaces,
        [DisplayText("Analytical Surfaces")]
        OST_AnalyticSurfaces,
        [DisplayText("Analytical Wall Foundation Tags")]
        OST_WallFoundationAnalyticalTags,
        [DisplayText("Analytical Wall Foundations")]
        OST_WallFoundationAnalytical,
        [DisplayText("Analytical Wall Tags")]
        OST_WallAnalyticalTags,
        [DisplayText("Analytical Walls")]
        OST_WallAnalytical,
        [DisplayText("Anchor Tags")]
        OST_StructConnectionAnchorTags,
        [DisplayText("Annotation Crop Boundary")]
        OST_AnnotationCrop,
        [DisplayText("Approach Slab Tags")]
        OST_ApproachSlabTags,
        [DisplayText("Area Load Tags")]
        OST_AreaLoadTags,
        [DisplayText("Area Tags")]
        OST_AreaTags,
        [DisplayText("Areas")]
        OST_Areas,
        [DisplayText("Assemblies")]
        OST_Assemblies,
        [DisplayText("Assembly Tags")]
        OST_AssemblyTags,
        [DisplayText("Audio Visual Device Tags")]
        OST_AudioVisualDeviceTags,
        [DisplayText("Audio Visual Devices")]
        OST_AudioVisualDevices,
        [DisplayText("Bearing Tags")]
        OST_BridgeBearingTags,
        [DisplayText("Bearings")]
        OST_BridgeBearings,
        [DisplayText("Bolt Tags")]
        OST_StructConnectionBoltTags,
        [DisplayText("Boundary Conditions")]
        OST_BoundaryConditions,
        [DisplayText("Brace in Plan View Symbols")]
        OST_StructuralBracePlanReps,
        [DisplayText("Bridge Cable Tags")]
        OST_BridgeCableTags,
        [DisplayText("Bridge Cables")]
        OST_BridgeCables,
        [DisplayText("Bridge Deck Tags")]
        OST_BridgeDeckTags,
        [DisplayText("Bridge Decks")]
        OST_BridgeDecks,
        [DisplayText("Bridge Framing")]
        OST_BridgeFraming,
        [DisplayText("Bridge Framing Tags")]
        OST_BridgeFramingTags,
        [DisplayText("Cable Tray Fitting Tags")]
        OST_CableTrayFittingTags,
        [DisplayText("Cable Tray Fittings")]
        OST_CableTrayFitting,
        [DisplayText("Cable Tray Runs")]
        OST_CableTrayRun,
        [DisplayText("Cable Tray Tags")]
        OST_CableTrayTags,
        [DisplayText("Cable Trays")]
        OST_CableTray,
        [DisplayText("Callout Boundary")]
        OST_CalloutBoundary,
        [DisplayText("Callout Heads")]
        OST_CalloutHeads,
        [DisplayText("Callouts")]
        OST_Callouts,
        [DisplayText("Cameras")]
        OST_Camera_Lines,
        [DisplayText("Casework")]
        OST_Casework,
        [DisplayText("Casework Tags")]
        OST_CaseworkTags,
        [DisplayText("Ceiling Tags")]
        OST_CeilingTags,
        [DisplayText("Ceilings")]
        OST_Ceilings,
        [DisplayText("Color Fill Legends")]
        OST_ColorFillLegends,
        [DisplayText("Columns")]
        OST_Columns,
        [DisplayText("Communication Device Tags")]
        OST_CommunicationDeviceTags,
        [DisplayText("Communication Devices")]
        OST_CommunicationDevices,
        [DisplayText("Conduit Fitting Tags")]
        OST_ConduitFittingTags,
        [DisplayText("Conduit Fittings")]
        OST_ConduitFitting,
        [DisplayText("Conduit Runs")]
        OST_ConduitRun,
        [DisplayText("Conduit Tags")]
        OST_ConduitTags,
        [DisplayText("Conduits")]
        OST_Conduit,
        [DisplayText("Connection Symbols")]
        OST_StructConnectionSymbols,
        [DisplayText("Contour Labels")]
        OST_ContourLabels,
        [DisplayText("Coordination Model")]
        OST_Coordination_Model,
        [DisplayText("Crop Boundaries")]
        OST_CropBoundary,
        [DisplayText("Curtain Grids")]
        OST_CurtainGrids,
        [DisplayText("Curtain Panel Tags")]
        OST_CurtainWallPanelTags,
        [DisplayText("Curtain Panels")]
        OST_CurtainWallPanels,
        [DisplayText("Curtain System Tags")]
        OST_CurtaSystemTags,
        [DisplayText("Curtain Systems")]
        OST_CurtaSystem,
        [DisplayText("Curtain Wall Mullion Tags")]
        OST_CurtainWallMullionTags,
        [DisplayText("Curtain Wall Mullions")]
        OST_CurtainWallMullions,
        [DisplayText("Data Device Tags")]
        OST_DataDeviceTags,
        [DisplayText("Data Devices")]
        OST_DataDevices,
        [DisplayText("Detail Item Tags")]
        OST_DetailComponentTags,
        [DisplayText("Detail Items")]
        OST_DetailComponents,
        [DisplayText("Dimensions")]
        OST_Dimensions,
        [DisplayText("Displacement Path")]
        OST_DisplacementPath,
        [DisplayText("Door Tags")]
        OST_DoorTags,
        [DisplayText("Doors")]
        OST_Doors,
        [DisplayText("Duct Accessories")]
        OST_DuctAccessory,
        [DisplayText("Duct Accessory Tags")]
        OST_DuctAccessoryTags,
        [DisplayText("Duct Color Fill")]
        OST_DuctColorFills,
        [DisplayText("Duct Color Fill Legends")]
        OST_DuctColorFillLegends,
        [DisplayText("Duct Fitting Tags")]
        OST_DuctFittingTags,
        [DisplayText("Duct Fittings")]
        OST_DuctFitting,
        [DisplayText("Duct Insulation Tags")]
        OST_DuctInsulationsTags,
        [DisplayText("Duct Insulations")]
        OST_DuctInsulations,
        [DisplayText("Duct Lining Tags")]
        OST_DuctLiningsTags,
        [DisplayText("Duct Linings")]
        OST_DuctLinings,
        [DisplayText("Duct Placeholders")]
        OST_PlaceHolderDucts,
        [DisplayText("Duct Systems")]
        OST_DuctSystem,
        [DisplayText("Duct Tags")]
        OST_DuctTags,
        [DisplayText("Ducts")]
        OST_DuctCurves,
        [DisplayText("Electrical Circuits")]
        OST_ElectricalCircuit,
        [DisplayText("Electrical Equipment")]
        OST_ElectricalEquipment,
        [DisplayText("Electrical Equipment Tags")]
        OST_ElectricalEquipmentTags,
        [DisplayText("Electrical Fixture Tags")]
        OST_ElectricalFixtureTags,
        [DisplayText("Electrical Fixtures")]
        OST_ElectricalFixtures,
        [DisplayText("Electrical Spare/Space Circuits")]
        OST_ElectricalInternalCircuits,
        [DisplayText("Elevation Marks")]
        OST_ElevationMarks,
        [DisplayText("Elevations")]
        OST_Elev,
        [DisplayText("Entourage")]
        OST_Entourage,
        [DisplayText("Expansion Joint Tags")]
        OST_ExpansionJointTags,
        [DisplayText("Expansion Joints")]
        OST_ExpansionJoints,
        [DisplayText("Filled region")]
        OST_FilledRegion,
        [DisplayText("Fire Alarm Device Tags")]
        OST_FireAlarmDeviceTags,
        [DisplayText("Fire Alarm Devices")]
        OST_FireAlarmDevices,
        [DisplayText("Fire Protection")]
        OST_FireProtection,
        [DisplayText("Fire Protection Tags")]
        OST_FireProtectionTags,
        [DisplayText("Flex Duct Tags")]
        OST_FlexDuctTags,
        [DisplayText("Flex Ducts")]
        OST_FlexDuctCurves,
        [DisplayText("Flex Pipe Tags")]
        OST_FlexPipeTags,
        [DisplayText("Flex Pipes")]
        OST_FlexPipeCurves,
        [DisplayText("Floor Tags")]
        OST_FloorTags,
        [DisplayText("Floors")]
        OST_Floors,
        [DisplayText("Food Service Equipment")]
        OST_FoodServiceEquipment,
        [DisplayText("Food Service Equipment Tags")]
        OST_FoodServiceEquipmentTags,
        [DisplayText("Foundation Span Direction Symbol")]
        OST_FootingSpanDirectionSymbol,
        [DisplayText("Furniture")]
        OST_Furniture,
        [DisplayText("Furniture System Tags")]
        OST_FurnitureSystemTags,
        [DisplayText("Furniture Systems")]
        OST_FurnitureSystems,
        [DisplayText("Furniture Tags")]
        OST_FurnitureTags,
        [DisplayText("Generic Annotations")]
        OST_GenericAnnotation,
        [DisplayText("Generic Model Tags")]
        OST_GenericModelTags,
        [DisplayText("Generic Models")]
        OST_GenericModel,
        [DisplayText("Grid Heads")]
        OST_GridHeads,
        [DisplayText("Grids")]
        OST_Grids,
        [DisplayText("Guide Grid")]
        OST_GuideGrid,
        [DisplayText("Hardscape")]
        OST_Hardscape,
        [DisplayText("Hardscape Tags")]
        OST_HardscapeTags,
        [DisplayText("Hole Tags")]
        OST_StructConnectionHoleTags,
        [DisplayText("HVAC Zones")]
        OST_HVAC_Zones,
        [DisplayText("Imports in Families")]
        OST_ImportObjectStyles,
        [DisplayText("Internal Area Load Tags")]
        OST_InternalAreaLoadTags,
        [DisplayText("Internal Line Load Tags")]
        OST_InternalLineLoadTags,
        [DisplayText("Internal Point Load Tags")]
        OST_InternalPointLoadTags,
        [DisplayText("Keynote Tags")]
        OST_KeynoteTags,
        [DisplayText("Level Heads")]
        OST_LevelHeads,
        [DisplayText("Levels")]
        OST_Levels,
        [DisplayText("Lighting Device Tags")]
        OST_LightingDeviceTags,
        [DisplayText("Lighting Devices")]
        OST_LightingDevices,
        [DisplayText("Lighting Fixture Tags")]
        OST_LightingFixtureTags,
        [DisplayText("Lighting Fixtures")]
        OST_LightingFixtures,
        [DisplayText("Line Load Tags")]
        OST_LineLoadTags,
        [DisplayText("Lines")]
        OST_Lines,
        [DisplayText("Masking Region")]
        OST_MaskingRegion,
        [DisplayText("Mass")]
        OST_Mass,
        [DisplayText("Mass Floor Tags")]
        OST_MassAreaFaceTags,
        [DisplayText("Mass Tags")]
        OST_MassTags,
        [DisplayText("Matchline")]
        OST_Matchline,
        [DisplayText("Material Tags")]
        OST_MaterialTags,
        [DisplayText("Materials")]
        OST_Materials,
        [DisplayText("Mechanical Equipment")]
        OST_MechanicalEquipment,
        [DisplayText("Mechanical Equipment Set Boundary Lines")]
        OST_MechanicalEquipmentSetBoundaryLines,
        [DisplayText("Mechanical Equipment Set Tags")]
        OST_MechanicalEquipmentSetTags,
        [DisplayText("Mechanical Equipment Sets")]
        OST_MechanicalEquipmentSet,
        [DisplayText("Mechanical Equipment Tags")]
        OST_MechanicalEquipmentTags,
        [DisplayText("Medical Equipment")]
        OST_MedicalEquipment,
        [DisplayText("Medical Equipment Tags")]
        OST_MedicalEquipmentTags,
        [DisplayText("MEP Fabrication Containment")]
        OST_FabricationContainment,
        [DisplayText("MEP Fabrication Containment Tags")]
        OST_FabricationContainmentTags,
        [DisplayText("MEP Fabrication Ductwork")]
        OST_FabricationDuctwork,
        [DisplayText("MEP Fabrication Ductwork Tags")]
        OST_FabricationDuctworkTags,
        [DisplayText("MEP Fabrication Hanger Tags")]
        OST_FabricationHangerTags,
        [DisplayText("MEP Fabrication Hangers")]
        OST_FabricationHangers,
        [DisplayText("MEP Fabrication Pipework")]
        OST_FabricationPipework,
        [DisplayText("MEP Fabrication Pipework Tags")]
        OST_FabricationPipeworkTags,
        [DisplayText("Model Groups")]
        OST_IOSModelGroups,
        [DisplayText("Multi Leader Tag")]
        OST_MultiLeaderTag,
        [DisplayText("Multi-Category Tags")]
        OST_MultiCategoryTags,
        [DisplayText("Multi-Rebar Annotations")]
        OST_MultiReferenceAnnotations,
        [DisplayText("Nurse Call Device Tags")]
        OST_NurseCallDeviceTags,
        [DisplayText("Nurse Call Devices")]
        OST_NurseCallDevices,
        [DisplayText("Panel Schedule Graphics")]
        OST_PanelScheduleGraphics,
        [DisplayText("Parking")]
        OST_Parking,
        [DisplayText("Parking Tags")]
        OST_ParkingTags,
        [DisplayText("Part Tags")]
        OST_PartTags,
        [DisplayText("Parts")]
        OST_Parts,
        [DisplayText("Path of Travel Tags")]
        OST_PathOfTravelTags,
        [DisplayText("Pier Cap Tags")]
        OST_PierCapTags,
        [DisplayText("Pier Column Tags")]
        OST_PierColumnTags,
        [DisplayText("Pier Foundation Tags")]
        OST_BridgeFoundationTags,
        [DisplayText("Pier Pile Tags")]
        OST_PierPileTags,
        [DisplayText("Pier Tags")]
        OST_BridgePierTags,
        [DisplayText("Pier Tower Tags")]
        OST_BridgeTowerTags,
        [DisplayText("Pier Wall Tags")]
        OST_PierWallTags,
        [DisplayText("Piers")]
        OST_BridgePiers,
        [DisplayText("Pipe Accessories")]
        OST_PipeAccessory,
        [DisplayText("Pipe Accessory Tags")]
        OST_PipeAccessoryTags,
        [DisplayText("Pipe Color Fill")]
        OST_PipeColorFills,
        [DisplayText("Pipe Color Fill Legends")]
        OST_PipeColorFillLegends,
        [DisplayText("Pipe Fitting Tags")]
        OST_PipeFittingTags,
        [DisplayText("Pipe Fittings")]
        OST_PipeFitting,
        [DisplayText("Pipe Insulation Tags")]
        OST_PipeInsulationsTags,
        [DisplayText("Pipe Insulations")]
        OST_PipeInsulations,
        [DisplayText("Pipe Placeholders")]
        OST_PlaceHolderPipes,
        [DisplayText("Pipe Segments")]
        OST_PipeSegments,
        [DisplayText("Pipe Tags")]
        OST_PipeTags,
        [DisplayText("Pipes")]
        OST_PipeCurves,
        [DisplayText("Piping Systems")]
        OST_PipingSystem,
        [DisplayText("Plan Region")]
        OST_PlanRegion,
        [DisplayText("Planting")]
        OST_Planting,
        [DisplayText("Planting Tags")]
        OST_PlantingTags,
        [DisplayText("Plate Tags")]
        OST_StructConnectionPlateTags,
        [DisplayText("Plumbing Fixture Tags")]
        OST_PlumbingFixtureTags,
        [DisplayText("Plumbing Fixtures")]
        OST_PlumbingFixtures,
        [DisplayText("Point Clouds")]
        OST_PointClouds,
        [DisplayText("Point Load Tags")]
        OST_PointLoadTags,
        [DisplayText("Profile Tags")]
        OST_StructConnectionProfilesTags,
        [DisplayText("Project Information")]
        OST_ProjectInformation,
        [DisplayText("Property Line Segment Tags")]
        OST_SitePropertyLineSegmentTags,
        [DisplayText("Property Tags")]
        OST_SitePropertyTags,
        [DisplayText("Railing Tags")]
        OST_StairsRailingTags,
        [DisplayText("Railings")]
        OST_StairsRailing,
        [DisplayText("Ramps")]
        OST_Ramps,
        [DisplayText("Raster Images")]
        OST_RasterImages,
        [DisplayText("Rebar Cover References")]
        OST_RebarCover,
        [DisplayText("Rebar Set Toggle")]
        OST_RebarSetToggle,
        [DisplayText("Rebar Shape")]
        OST_RebarShape,
        [DisplayText("Reference Lines")]
        OST_ReferenceLines,
        [DisplayText("Reference Planes")]
        OST_CLines,
        [DisplayText("Reference Points")]
        OST_ReferencePoints,
        [DisplayText("Render Regions")]
        OST_RenderRegions,
        [DisplayText("Revision")]
        OST_Revisions,
        [DisplayText("Revision Cloud Tags")]
        OST_RevisionCloudTags,
        [DisplayText("Revision Clouds")]
        OST_RevisionClouds,
        [DisplayText("Road Tags")]
        OST_RoadTags,
        [DisplayText("Roads")]
        OST_Roads,
        [DisplayText("Roof Tags")]
        OST_RoofTags,
        [DisplayText("Roofs")]
        OST_Roofs,
        [DisplayText("Room Tags")]
        OST_RoomTags,
        [DisplayText("Rooms")]
        OST_Rooms,
        [DisplayText("Routing Preferences")]
        OST_RoutingPreferences,
        [DisplayText("RVT Links")]
        OST_RvtLinks,
        [DisplayText("Schedule Graphics")]
        OST_ScheduleGraphics,
        [DisplayText("Schedules")]
        OST_Schedules,
        [DisplayText("Scope Boxes")]
        OST_VolumeOfInterest,
        [DisplayText("Section Boxes")]
        OST_SectionBox,
        [DisplayText("Section Line")]
        OST_SectionLine,
        [DisplayText("Section Marks")]
        OST_SectionHeads,
        [DisplayText("Sections")]
        OST_Sections,
        [DisplayText("Security Device Tags")]
        OST_SecurityDeviceTags,
        [DisplayText("Security Devices")]
        OST_SecurityDevices,
        [DisplayText("Shaft Openings")]
        OST_ShaftOpening,
        [DisplayText("Shear Stud Tags")]
        OST_StructConnectionShearStudTags,
        [DisplayText("Sheets")]
        OST_Sheets,
        [DisplayText("Signage")]
        OST_Signage,
        [DisplayText("Signage Tags")]
        OST_SignageTags,
        [DisplayText("Site")]
        OST_Site,
        [DisplayText("Site Tags")]
        OST_SiteTags,
        [DisplayText("Space Tags")]
        OST_MEPSpaceTags,
        [DisplayText("Spaces")]
        OST_MEPSpaces,
        [DisplayText("Span Direction Symbol")]
        OST_SpanDirectionSymbol,
        [DisplayText("Specialty Equipment")]
        OST_SpecialityEquipment,
        [DisplayText("Specialty Equipment Tags")]
        OST_SpecialityEquipmentTags,
        [DisplayText("Spot Coordinates")]
        OST_SpotCoordinates,
        [DisplayText("Spot Elevation Symbols")]
        OST_SpotElevSymbols,
        [DisplayText("Spot Elevations")]
        OST_SpotElevations,
        [DisplayText("Spot Slopes")]
        OST_SpotSlopes,
        [DisplayText("Sprinkler Tags")]
        OST_SprinklerTags,
        [DisplayText("Sprinklers")]
        OST_Sprinklers,
        [DisplayText("Stair Landing Tags")]
        OST_StairsLandingTags,
        [DisplayText("Stair Paths")]
        OST_StairsPaths,
        [DisplayText("Stair Run Tags")]
        OST_StairsRunTags,
        [DisplayText("Stair Support Tags")]
        OST_StairsSupportTags,
        [DisplayText("Stair Tags")]
        OST_StairsTags,
        [DisplayText("Stair Tread/Riser Numbers")]
        OST_StairsTriserNumbers,
        [DisplayText("Stairs")]
        OST_Stairs,
        [DisplayText("Structural Annotations")]
        OST_StructuralAnnotations,
        [DisplayText("Structural Area Reinforcement")]
        OST_AreaRein,
        [DisplayText("Structural Area Reinforcement Symbols")]
        OST_AreaReinSpanSymbol,
        [DisplayText("Structural Area Reinforcement Tags")]
        OST_AreaReinTags,
        [DisplayText("Structural Beam System Tags")]
        OST_BeamSystemTags,
        [DisplayText("Structural Beam Systems")]
        OST_StructuralFramingSystem,
        [DisplayText("Structural Column Tags")]
        OST_StructuralColumnTags,
        [DisplayText("Structural Columns")]
        OST_StructuralColumns,
        [DisplayText("Structural Connection Tags")]
        OST_StructConnectionTags,
        [DisplayText("Structural Connections")]
        OST_StructConnections,
        [DisplayText("Structural Fabric Areas")]
        OST_FabricAreas,
        [DisplayText("Structural Fabric Reinforcement")]
        OST_FabricReinforcement,
        [DisplayText("Structural Fabric Reinforcement Symbols")]
        OST_FabricReinSpanSymbol,
        [DisplayText("Structural Fabric Reinforcement Tags")]
        OST_FabricReinforcementTags,
        [DisplayText("Structural Foundation Tags")]
        OST_StructuralFoundationTags,
        [DisplayText("Structural Foundations")]
        OST_StructuralFoundation,
        [DisplayText("Structural Framing")]
        OST_StructuralFraming,
        [DisplayText("Structural Framing Tags")]
        OST_StructuralFramingTags,
        [DisplayText("Structural Internal Loads")]
        OST_InternalLoads,
        [DisplayText("Structural Load Cases")]
        OST_LoadCases,
        [DisplayText("Structural Loads")]
        OST_Loads,
        [DisplayText("Structural Path Reinforcement")]
        OST_PathRein,
        [DisplayText("Structural Path Reinforcement Symbols")]
        OST_PathReinSpanSymbol,
        [DisplayText("Structural Path Reinforcement Tags")]
        OST_PathReinTags,
        [DisplayText("Structural Rebar")]
        OST_Rebar,
        [DisplayText("Structural Rebar Coupler Tags")]
        OST_CouplerTags,
        [DisplayText("Structural Rebar Couplers")]
        OST_Coupler,
        [DisplayText("Structural Rebar Tags")]
        OST_RebarTags,
        [DisplayText("Structural Stiffener Tags")]
        OST_StructuralStiffenerTags,
        [DisplayText("Structural Stiffeners")]
        OST_StructuralStiffener,
        [DisplayText("Structural Tendon Tags")]
        OST_StructuralTendonTags,
        [DisplayText("Structural Tendons")]
        OST_StructuralTendons,
        [DisplayText("Structural Truss Tags")]
        OST_TrussTags,
        [DisplayText("Structural Trusses")]
        OST_StructuralTruss,
        [DisplayText("Switch System")]
        OST_SwitchSystem,
        [DisplayText("System-Zone Tags")]
        OST_MEPSystemZoneTags,
        [DisplayText("System-Zones")]
        OST_MEPSystemZone,
        [DisplayText("Telephone Device Tags")]
        OST_TelephoneDeviceTags,
        [DisplayText("Telephone Devices")]
        OST_TelephoneDevices,
        [DisplayText("Temporary Structure Tags")]
        OST_TemporaryStructureTags,
        [DisplayText("Temporary Structures")]
        OST_TemporaryStructure,
        [DisplayText("Text Notes")]
        OST_TextNotes,
        [DisplayText("Title Blocks")]
        OST_TitleBlocks,
        [DisplayText("Topography")]
        OST_Topography,
        [DisplayText("Vertical Circulation")]
        OST_VerticalCirculation,
        [DisplayText("Vertical Circulation Tags")]
        OST_VerticalCirculationTags,
        [DisplayText("Vibration Damper Tags")]
        OST_VibrationDamperTags,
        [DisplayText("Vibration Isolator Tags")]
        OST_VibrationIsolatorTags,
        [DisplayText("Vibration Management")]
        OST_VibrationManagement,
        [DisplayText("View Reference")]
        OST_ReferenceViewerSymbol,
        [DisplayText("View Titles")]
        OST_ViewportLabel,
        [DisplayText("Viewports")]
        OST_Viewports,
        [DisplayText("Views")]
        OST_Views,
        [DisplayText("Wall Tags")]
        OST_WallTags,
        [DisplayText("Walls")]
        OST_Walls,
        [DisplayText("Water Loops")]
        OST_MEPAnalyticalWaterLoop,
        [DisplayText("Weld Tags")]
        OST_StructConnectionWeldTags,
        [DisplayText("Window Tags")]
        OST_WindowTags,
        [DisplayText("Windows")]
        OST_Windows,
        [DisplayText("Wire Tags")]
        OST_WireTags,
        [DisplayText("Wires")]
        OST_Wire,
        [DisplayText("Zone Equipment")]
        OST_ZoneEquipment,
        [DisplayText("Zone Tags")]
        OST_ZoneTags,
    }
}
