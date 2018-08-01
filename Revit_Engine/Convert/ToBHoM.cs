using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using BH.oM.Environment.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Structural.Properties;
using BH.oM.Environment.Elements;

using BH.Engine.Environment;
using BHS = BH.Engine.Structure;
using BHG = BH.Engine.Geometry;

using BH.oM.Base;
using Autodesk.Revit.DB.Structure.StructuralSections;
using Autodesk.Revit.DB.Analysis;
using BH.oM.Adapters.Revit;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****      Convert Revit elements to BHoM       ****/
        /***************************************************/

        public static List<BHoMObject> ToBHoM(this PlanarFace planarFace, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        EdgeArrayArray aEdgeArrayArray = planarFace.EdgeLoops;
                        if (aEdgeArrayArray != null && aEdgeArrayArray.Size > 0)
                        {
                            List<BHoMObject> aResult = new List<BHoMObject>();
                            for (int i = 0; i < aEdgeArrayArray.Size; i++)
                            {
                                EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                                List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                                foreach (Autodesk.Revit.DB.Edge aEdge in aEdgeArray)
                                {
                                    Curve aCurve = aEdge.AsCurve();
                                    if (aCurve != null)
                                        aCurveList.Add(aCurve.ToBHoM(convertUnits));
                                }

                                if (aCurveList != null && aCurveList.Count > 0)
                                {
                                    BuildingElementPanel aBuildingElementPanel = new BuildingElementPanel();
                                    aBuildingElementPanel = aBuildingElementPanel.SetGeometry(Geometry.Create.PolyCurve(aCurveList));
                                    aResult.Add(aBuildingElementPanel);
                                }
                            }
                            return aResult;
                        }
                        return null;
                    }
            }

            return null;
        }

        /***************************************************/
        
        public static BuildingElementCurve ToBHoMBuildingElementCurve(this Wall wall, Discipline discipline = Discipline.Environmental, bool convertUnits = true)
        {
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve
            {
                Curve = (wall.Location as LocationCurve).ToBHoM(convertUnits)
            };
            return aBuildingElementCurve;
        }

        /***************************************************/
        
        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this Element element, bool convertUnits = true)
        {
            return ToBHoMBuildingElementPanels(element.get_Geometry(new Options()), convertUnits);
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this RoofBase roofBase, bool convertUnits = true)
        {
            return ToBHoMBuildingElementPanels(roofBase.get_Geometry(new Options()), convertUnits);
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this FamilyInstance familyInstance, bool convertUnits = true)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();

            //TODO: Get more accurate shape. Currently, Windows and doors goes as rectangular panel
            BoundingBoxXYZ aBoundingBoxXYZ = familyInstance.get_BoundingBox(null);

            XYZ aVector = aBoundingBoxXYZ.Max - aBoundingBoxXYZ.Min;

            double aWidth = Math.Abs(aVector.Y);
            double aHeight = Math.Abs(aVector.Z);

            XYZ aVector_Y = (aBoundingBoxXYZ.Transform.BasisY * aWidth) / 2;
            XYZ aVector_Z = (aBoundingBoxXYZ.Transform.BasisZ * aHeight) / 2;

            XYZ aMiddle = (aBoundingBoxXYZ.Max + aBoundingBoxXYZ.Min) / 2;

            XYZ aXYZ_1 = aMiddle + aVector_Z - aVector_Y;
            XYZ aXYZ_2 = aMiddle + aVector_Z + aVector_Y;
            XYZ aXYZ_3 = aMiddle - aVector_Z + aVector_Y;
            XYZ aXYZ_4 = aMiddle - aVector_Z - aVector_Y;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            aPointList.Add(aXYZ_1.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_2.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_3.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_4.ToBHoM(convertUnits));
            aPointList.Add(aXYZ_1.ToBHoM(convertUnits));

            BuildingElementPanel aBuildingElementPanel = Create.BuildingElementPanel(new oM.Geometry.Polyline[] { Geometry.Create.Polyline(aPointList) });
            if (aBuildingElementPanel != null)
                aResult.Add(aBuildingElementPanel);

            return aResult;
        }

        /***************************************************/

        public static List<BuildingElementPanel> ToBHoMBuildingElementPanels(this GeometryElement geometryElement, bool convertUnits = true)
        {
            List<BuildingElementPanel> aResult = new List<BuildingElementPanel>();
            foreach (GeometryObject aGeometryObject in geometryElement)
            {
                Solid aSolid = aGeometryObject as Solid;
                if (aSolid == null)
                    continue;

                PlanarFace aPlanarFace = Query.Top(aSolid);
                if (aPlanarFace == null)
                    continue;

                List<BHoMObject> aBHoMObjectList = aPlanarFace.ToBHoM(Discipline.Environmental, convertUnits);
                if (aBHoMObjectList == null || aBHoMObjectList.Count < 1)
                    continue;

                List<BuildingElementPanel> aBuildingElementPanelList = aBHoMObjectList.Cast<BuildingElementPanel>().ToList();
                if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                    aResult.AddRange(aBuildingElementPanelList);
            }

            return aResult;

        }

        /***************************************************/
        
        public static BuildingElement ToBHoMBuildingElement(this Element element, BuildingElementPanel buildingElementPanel, Dictionary<ElementId, List<BHoMObject>> objects = null, bool copyCustomData = true, bool convertUnits = true)
        {
            ElementType aElementType = element.Document.GetElement(element.GetTypeId()) as ElementType;
            BuildingElementProperties aBuildingElementProperties = null;
            if (objects != null)
            {
                List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                if (objects.TryGetValue(aElementType.Id, out aBHoMObjectList))
                    if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                        aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
            }

            if (aBuildingElementProperties == null)
            {
                BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)aElementType.Category.Id.IntegerValue);
                if (!aBuildingElementType.HasValue)
                    return null;

                aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, aElementType.Name);
                aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, aElementType) as BuildingElementProperties;
                if (copyCustomData)
                    aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, aElementType, convertUnits) as BuildingElementProperties;

                if (objects != null)
                    objects.Add(aElementType.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElementProperties }));
            }


            oM.Architecture.Elements.Level aLevel = Query.Level(element, objects, Discipline.Environmental, copyCustomData, convertUnits);
            if (aLevel == null)
                return null;

            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, buildingElementPanel);
            aBuildingElement.Level = aLevel;
            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, element) as BuildingElement;
            if (copyCustomData)
                aBuildingElement = Modify.SetCustomData(aBuildingElement, element, convertUnits) as BuildingElement;

            if (objects != null)
            {
                List<BHoMObject> aBHoMObjectList = null;
                if (objects.TryGetValue(element.Id, out aBHoMObjectList))
                    aBHoMObjectList.Add(aBuildingElement);
                else
                    objects.Add(element.Id, new List<BHoMObject>(new BHoMObject[] { aBuildingElement }));
            }

            return aBuildingElement;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this  Document document, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            if (document == null)
                return null;

            switch (discipline)
            {
                case Discipline.Environmental:

                    double aElevation = 0;
                    double aLongitude = 0;
                    double aLatitude = 0;
                    double aTimeZone = 0;
                    string aPlaceName = string.Empty;
                    string aWeatherStationName = string.Empty;

                    if(document.SiteLocation != null)
                    {
                        aElevation = document.SiteLocation.Elevation;
                        aLongitude = document.SiteLocation.Longitude;
                        aLatitude = document.SiteLocation.Latitude;
                        aTimeZone = document.SiteLocation.TimeZone;
                        aPlaceName = document.SiteLocation.PlaceName;
                        aWeatherStationName = document.SiteLocation.WeatherStationName;

                        if(convertUnits)
                        {
                            aElevation = UnitUtils.ConvertFromInternalUnits(aElevation, DisplayUnitType.DUT_METERS);
                            aLongitude = UnitUtils.ConvertFromInternalUnits(aLongitude, DisplayUnitType.DUT_METERS);
                            aLatitude = UnitUtils.ConvertFromInternalUnits(aLatitude, DisplayUnitType.DUT_METERS);
                        }
                    }

                    double aProjectAngle = 0;
                    double aProjectEastWestOffset = 0;
                    double aProjectElevation = 0;
                    double aProjectNorthSouthOffset = 0;

                    if(document.ActiveProjectLocation != null)
                    {
                        ProjectLocation aProjectLocation = document.ActiveProjectLocation;
                        XYZ aXYZ = new XYZ(0, 0, 0);
                        ProjectPosition aProjectPosition = aProjectLocation.GetProjectPosition(aXYZ);
                        if(aProjectPosition != null)
                        {
                            aProjectAngle = aProjectPosition.Angle;
                            aProjectEastWestOffset = aProjectPosition.EastWest;
                            aProjectElevation = aProjectPosition.Elevation;
                            aProjectNorthSouthOffset = aProjectPosition.NorthSouth;
                        }
                    }


                    Building aBuilding = new Building
                    {
                        //TODO: Include missing properties
                        Elevation = aElevation,
                        Longitude = aLongitude,
                        Latitude = aLatitude,
                        Location = new oM.Geometry.Point()
                    };

                    aBuilding = Modify.SetIdentifiers(aBuilding, document.ProjectInformation) as Building;
                    if (copyCustomData)
                    {
                        aBuilding = Modify.SetCustomData(aBuilding, "Time Zone", aTimeZone) as Building;
                        aBuilding = Modify.SetCustomData(aBuilding, "Place Name", aPlaceName) as Building;
                        aBuilding = Modify.SetCustomData(aBuilding, "Weather Station Name", aWeatherStationName) as Building;

                        aBuilding = Modify.SetCustomData(aBuilding, "Project Angle", aProjectAngle) as Building;
                        aBuilding = Modify.SetCustomData(aBuilding, "Project East/West Offset", aProjectEastWestOffset) as Building;
                        aBuilding = Modify.SetCustomData(aBuilding, "Project North/South Offset", aProjectNorthSouthOffset) as Building;
                        aBuilding = Modify.SetCustomData(aBuilding, "Project Elevation", aProjectElevation) as Building;

                        aBuilding = Modify.SetCustomData(aBuilding, document.ProjectInformation, convertUnits) as Building;
                    }
                        

                    //-------- Create BHoM building structure -----

                    Dictionary<ElementId, List<BHoMObject>> aObjects = new Dictionary<ElementId, List<BHoMObject>>();
                    using (Transaction aTransaction = new Transaction(document, "GetAnalyticalModel"))
                    {
                        aTransaction.Start();

                        FailureHandlingOptions aFailureHandlingOptions = aTransaction.GetFailureHandlingOptions();
                        aFailureHandlingOptions.SetFailuresPreprocessor(new WarningSwallower());
                        aTransaction.SetFailureHandlingOptions(aFailureHandlingOptions);

                        EnergyAnalysisDetailModel aEnergyAnalysisDetailModel = null;

                        using (SubTransaction aSubTransaction = new SubTransaction(document))
                        {
                            aSubTransaction.Start();
                            aEnergyAnalysisDetailModel = EnergyAnalysisDetailModel.GetMainEnergyAnalysisDetailModel(document);
                            if (aEnergyAnalysisDetailModel != null && aEnergyAnalysisDetailModel.IsValidObject)
                            {
                                document.Delete(aEnergyAnalysisDetailModel.Id);
                            }

                            aSubTransaction.Commit();
                        }

                        EnergyAnalysisDetailModelOptions aEnergyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
                        aEnergyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
                        aEnergyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
                        aEnergyAnalysisDetailModelOptions.ExportMullions = true;
                        aEnergyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
                        aEnergyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

                        EnergyDataSettings aEnergyDataSettings = EnergyDataSettings.GetFromDocument(document);
                        aEnergyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                        aEnergyDataSettings.ExportDefaults = false;
                        aEnergyDataSettings.SliverSpaceTolerance = UnitUtils.ConvertToInternalUnits(5, DisplayUnitType.DUT_MILLIMETERS);
                        aEnergyDataSettings.AnalysisType = AnalysisMode.BuildingElements;
                        aEnergyDataSettings.EnergyModel = false;

                        //AnalyticalSpaces
                        aEnergyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document, aEnergyAnalysisDetailModelOptions);
                        IList<EnergyAnalysisSpace> aEnergyAnalysisSpaces = aEnergyAnalysisDetailModel.GetAnalyticalSpaces();
                        Dictionary<string, EnergyAnalysisSurface> aEnergyAnalysisSurfaces = new Dictionary<string, EnergyAnalysisSurface>();
                        foreach (EnergyAnalysisSpace aEnergyAnalysisSpace in aEnergyAnalysisSpaces)
                        {
                            Space aSpace = aEnergyAnalysisSpace.ToBHoM(aObjects, discipline, copyCustomData, convertUnits) as Space;
                            AddBHoMObject(aSpace, aObjects);

                            foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aEnergyAnalysisSpace.GetAnalyticalSurfaces())
                                if (!aEnergyAnalysisSurfaces.ContainsKey(aEnergyAnalysisSurface.SurfaceName))
                                    aEnergyAnalysisSurfaces.Add(aEnergyAnalysisSurface.SurfaceName, aEnergyAnalysisSurface);
                        }

                        //EnergyAnalysisSurfaces
                        foreach (KeyValuePair<string, EnergyAnalysisSurface> aKeyValuePair in aEnergyAnalysisSurfaces)
                        {
                            List<BHoMObject> aBHoMObjectList = aKeyValuePair.Value.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                            AddBHoMObjects(aBHoMObjectList, aObjects);

                            List<BHoMObject> aBHoMObjectList_Hosted = new List<BHoMObject>();
                            foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aKeyValuePair.Value.GetAnalyticalOpenings())
                            {
                                List<BHoMObject> aBHoMObjectList_Hosted_Temp = aEnergyAnalysisOpening.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                                if(aBHoMObjectList_Hosted_Temp != null && aBHoMObjectList_Hosted_Temp.Count > 0)
                                {
                                    aBHoMObjectList_Hosted.AddRange(aBHoMObjectList_Hosted_Temp);
                                    AddBHoMObjects(aBHoMObjectList_Hosted_Temp, aObjects);
                                }
                                
                            }

                            //------------ Cutting openings ----------------
                            if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                            {
                                foreach (BHoMObject aBHoMObject in aBHoMObjectList.FindAll(x => x is BuildingElement))
                                {
                                    BuildingElementPanel aBuildingElementPanel = (aBHoMObject as BuildingElement).BuildingElementGeometry as BuildingElementPanel;
                                    if (aBuildingElementPanel == null)
                                        continue;

                                    foreach (BuildingElement aBuildingElement_Hosted in aBHoMObjectList_Hosted.FindAll(x => x is BuildingElement))
                                    {
                                        BuildingElementPanel aBuildingElementPanel_Hosted = aBuildingElement_Hosted.BuildingElementGeometry as BuildingElementPanel;
                                        if (aBuildingElementPanel_Hosted == null)
                                            continue;

                                        BuildingElementOpening aBuildingElementOpening = new BuildingElementOpening()
                                        {
                                            PolyCurve = aBuildingElementPanel_Hosted.PolyCurve
                                        };

                                        if (copyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(Adapter.Revit.Id.ElementId))
                                            aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, Adapter.Revit.Id.ElementId, aBuildingElement_Hosted.CustomData[Adapter.Revit.Id.ElementId]) as BuildingElementOpening;

                                        aBuildingElementPanel.Openings.Add(aBuildingElementOpening);
                                    }
                                }
                            }
                            //-------------------------------------------
                        }


                        //AnalyticalShadingSurfaces
                        IList<EnergyAnalysisSurface> aAnalyticalShadingSurfaceList = aEnergyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
                        foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aAnalyticalShadingSurfaceList)
                        {
                            List<BHoMObject> aBHoMObjectList = aEnergyAnalysisSurface.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                            AddBHoMObjects(aBHoMObjectList, aObjects);

                            List<BHoMObject> aBHoMObjectList_Hosted = new List<BHoMObject>();
                            foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aEnergyAnalysisSurface.GetAnalyticalOpenings())
                            {
                                List<BHoMObject> aBHoMObjectList_Hosted_Temp = aEnergyAnalysisOpening.ToBHoM(aObjects, discipline, copyCustomData, convertUnits);
                                if (aBHoMObjectList_Hosted_Temp != null && aBHoMObjectList_Hosted_Temp.Count > 0)
                                {
                                    aBHoMObjectList_Hosted.AddRange(aBHoMObjectList_Hosted_Temp);
                                    AddBHoMObjects(aBHoMObjectList_Hosted_Temp, aObjects);
                                }

                            }

                            //------------ Cutting openings ----------------
                            if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                            {
                                foreach (BHoMObject aBHoMObject in aBHoMObjectList.FindAll(x => x is BuildingElement))
                                {
                                    BuildingElementPanel aBuildingElementPanel = (aBHoMObject as BuildingElement).BuildingElementGeometry as BuildingElementPanel;
                                    if (aBuildingElementPanel == null)
                                        continue;

                                    foreach (BuildingElement aBuildingElement_Hosted in aBHoMObjectList_Hosted.FindAll(x => x is BuildingElement))
                                    {
                                        BuildingElementPanel aBuildingElementPanel_Hosted = aBuildingElement_Hosted.BuildingElementGeometry as BuildingElementPanel;
                                        if (aBuildingElementPanel_Hosted == null)
                                            continue;

                                        BuildingElementOpening aBuildingElementOpening = new BuildingElementOpening()
                                        {
                                            PolyCurve = aBuildingElementPanel_Hosted.PolyCurve
                                        };

                                        if (copyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(Adapter.Revit.Id.ElementId))
                                            aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, Adapter.Revit.Id.ElementId, aBuildingElement_Hosted.CustomData[Adapter.Revit.Id.ElementId]) as BuildingElementOpening;

                                        aBuildingElementPanel.Openings.Add(aBuildingElementOpening);
                                    }
                                }
                            }
                            //-------------------------------------------
                        }

                        aTransaction.RollBack();
                    }

                    foreach (KeyValuePair<ElementId, List<BHoMObject>> aKeyValuePair in aObjects)
                        foreach (BHoMObject aBHoMObject in aKeyValuePair.Value)
                        {
                            if (aBHoMObject is Space)
                                aBuilding.Spaces.Add(aBHoMObject as Space);
                            else if (aBHoMObject is oM.Architecture.Elements.Level)
                                aBuilding.Levels.Add(aBHoMObject as oM.Architecture.Elements.Level);
                            else if (aBHoMObject is BuildingElementProperties)
                                aBuilding.BuildingElementProperties.Add(aBHoMObject as BuildingElementProperties);
                            else if (aBHoMObject is BuildingElement)
                                aBuilding.BuildingElements.Add(aBHoMObject as BuildingElement);
                        }


                    //TODO: To be removed for next release when Space.BuildingElements removed from Space
                    foreach (BuildingElement aBuildingElement in aBuilding.BuildingElements)
                    {
                        if (aBuildingElement.AdjacentSpaces != null && aBuildingElement.AdjacentSpaces.Count > 0)
                            foreach (Guid aGuid in aBuildingElement.AdjacentSpaces)
                            {
                                Space aSpace = aBuilding.Spaces.Find(x => x.BHoM_Guid == aGuid);
                                if(aSpace != null)
                                {
                                    if (aSpace.BuildingElements.Find(x => x.BHoM_Guid == aBuildingElement.BHoM_Guid) == null)
                                        aSpace.BuildingElements.Add(aBuildingElement);
                                }
                            }

                    }


                    //---------------------------------------------

                    return aBuilding;
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FamilyInstance familyInstance, Discipline discipline = Discipline.Structural, bool copyCustomData = true, bool convertUnits = true)
        {
            if (familyInstance == null)
                //TODO: Add warning here
                return null;

            switch (discipline)
            {
                case Discipline.Structural:
                    return familyInstance.ToBHoMFramingElement(copyCustomData, convertUnits);
                case Discipline.Environmental:
                    {
                        BuildingElementType? aBuildingElementType = Query.BuildingElementType((BuiltInCategory)familyInstance.Category.Id.IntegerValue);
                        if(aBuildingElementType.HasValue)
                        {
                            List<BuildingElementPanel> aBuildingElementPanelList = ToBHoMBuildingElementPanels(familyInstance, convertUnits);
                            if (aBuildingElementPanelList != null && aBuildingElementPanelList.Count > 0)
                                return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), null, copyCustomData, convertUnits);
                            return ToBHoMBuildingElement(familyInstance, aBuildingElementPanelList.First(), null, convertUnits);
                        }
                        return null;
                    }
                case Discipline.Architecture:
                    {
                        //TODO: add code for Architectural FamilyInstances
                        return null;
                    }
            }

            //TODO: Add warning about null here
            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Wall wall, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {

                        BuildingElementProperties aBuildingElementProperties = wall.WallType.ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

                        BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, ToBHoMBuildingElementCurve(wall, discipline, convertUnits), ToBHoM(wall.Document.GetElement(wall.LevelId) as Level, discipline, copyCustomData, convertUnits) as BH.oM.Architecture.Elements.Level);

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, wall) as BuildingElement;
                        if (copyCustomData)
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, wall, convertUnits) as BuildingElement;

                        return new List<BHoMObject> { aBuildingElement };
                    }

                case Discipline.Structural:
                    return wall.ToBHoMPanelPlanar(copyCustomData, convertUnits).ConvertAll(p => p as BHoMObject);
            }

            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Ceiling ceiling, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = (ceiling.Document.GetElement(ceiling.GetTypeId()) as CeilingType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(ceiling))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(ceiling.Document.GetElement(ceiling.LevelId) as Level, discipline, convertUnits) as oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, ceiling) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, ceiling, convertUnits) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
            }

            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this Floor floor, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = floor.FloorType.ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(floor, convertUnits))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(floor.Document.GetElement(floor.LevelId) as Level, discipline, convertUnits) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, floor) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, floor, convertUnits) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
                case Discipline.Structural:
                    return floor.ToBHoMPanelPlanar(copyCustomData, convertUnits).ConvertAll(p => p as BHoMObject);
            }

            return null;
        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this RoofBase roofBase, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        // we need the same like this for walls?


                        List<BHoMObject> aResult = new List<BHoMObject>();
                        BuildingElementProperties aBuildingElementProperties = roofBase.RoofType.ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        foreach (BuildingElementPanel aBuildingElementPanel in ToBHoMBuildingElementPanels(roofBase, convertUnits))
                        {
                            BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, aBuildingElementPanel, ToBHoM(roofBase.Document.GetElement(roofBase.LevelId) as Level, discipline, convertUnits) as BH.oM.Architecture.Elements.Level);

                            aBuildingElement = Modify.SetIdentifiers(aBuildingElement, roofBase) as BuildingElement;
                            if (copyCustomData)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, roofBase, convertUnits) as BuildingElement;

                            aResult.Add(aBuildingElement);
                        }
                        return aResult;
                    }
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this WallType wallType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Wall, wallType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, wallType) as BuildingElementProperties;
                        if (copyCustomData)
                        {
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, convertUnits) as BuildingElementProperties;
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, wallType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
                        }
                            

                        return aBuildingElementProperties;
                    }

                case Discipline.Structural:
                    return wallType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as BHoMObject;
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FloorType floorType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true, string materialGrade = null)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Floor, floorType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, floorType) as BuildingElementProperties;
                        if (copyCustomData)
                        {
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, convertUnits) as BuildingElementProperties;
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, floorType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
                        }
                            

                        return aBuildingElementProperties;
                    }

                case Discipline.Structural:
                    return floorType.ToBHoMProperty2D(copyCustomData, convertUnits, materialGrade) as BHoMObject;
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this CeilingType ceilingType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Ceiling, ceilingType.Name);

            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, ceilingType) as BuildingElementProperties;
            if (copyCustomData)
            {
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, convertUnits) as BuildingElementProperties;
                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, ceilingType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
            }

            return aBuildingElementProperties;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this RoofType roofType, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuildingElementType.Roof, roofType.Name);

                        aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, roofType) as BuildingElementProperties;
                        if (copyCustomData)
                        {
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, convertUnits) as BuildingElementProperties;
                            aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, roofType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
                        }

                        return aBuildingElementProperties;
                    }
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this FamilySymbol familySymbol, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            if (familySymbol == null)
                return null;

            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        BuildingElementType? aBuildingElementType = Query.BuildingElementType(familySymbol.Category);
                        if (aBuildingElementType.HasValue)
                        {
                            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(aBuildingElementType.Value, familySymbol.Name);
                            aBuildingElementProperties = Modify.SetIdentifiers(aBuildingElementProperties, familySymbol) as BuildingElementProperties;
                            if (copyCustomData)
                            {
                                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, convertUnits) as BuildingElementProperties;
                                aBuildingElementProperties = Modify.SetCustomData(aBuildingElementProperties, familySymbol, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElementProperties;
                            }

                            return aBuildingElementProperties;
                        }

                        return null;
                    }
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this ElementType elementType, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    BuildingElementProperties aBuildingElementProperties = null;
                    if (objects != null)
                    {
                        List<BHoMObject> aBHoMObjectList = new List<BHoMObject>();
                        if (objects.TryGetValue(elementType.Id, out aBHoMObjectList))
                            if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                                aBuildingElementProperties = aBHoMObjectList.First() as BuildingElementProperties;
                    }

                    if (aBuildingElementProperties == null)
                    {
                        //TODO: dynamic does not work. ToBHoM for WallType not recognized
                        //aBuildingElementProperties = (elementType as dynamic).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;

                        if (elementType is WallType)
                            aBuildingElementProperties = (elementType as WallType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is FloorType)
                            aBuildingElementProperties = (elementType as FloorType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is CeilingType)
                            aBuildingElementProperties = (elementType as CeilingType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is RoofType)
                            aBuildingElementProperties = (elementType as RoofType).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        else if (elementType is FamilySymbol)
                            aBuildingElementProperties = (elementType as FamilySymbol).ToBHoM(discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                    }
                    return aBuildingElementProperties;

            }

            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this StructuralMaterialType structuralMaterialType, string materialGrade, Discipline discipline = Discipline.Structural)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return structuralMaterialType.ToBHoMMaterial(materialGrade);
            }
            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this Material material, string materialGrade, Discipline discipline = Discipline.Structural)
        {
            switch (discipline)
            {
                case Discipline.Structural:
                    return material.ToBHoMMaterial(materialGrade);
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Level level, Discipline discipline = Discipline.Environmental, bool CopyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return level.ToBHoMLevel(CopyCustomData, convertUnits);
            }

            return null;
        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this SpatialElement spatialElement, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                        aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
                        aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

                        SpatialElementGeometryCalculator aSpatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, aSpatialElementBoundaryOptions);
                        
                        return ToBHoM(spatialElement, aSpatialElementGeometryCalculator, objects, discipline, copyCustomData, convertUnits);
                    }
            }
            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch(discipline)
            {
                case Discipline.Environmental:
                    {
                        if (spatialElement == null || spatialElementBoundaryOptions == null)
                            return null;

                        Document aDocument = spatialElement.Document;

                        oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects, discipline, copyCustomData, convertUnits);

                        List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
                        IList<IList<BoundarySegment>> aBoundarySegmentListList = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
                        if (aBoundarySegmentListList != null)
                            foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                                foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                                {
                                    oM.Geometry.ICurve aICurve = aBoundarySegment.GetCurve().ToBHoM(convertUnits);
                                    Element aElement = aDocument.GetElement(aBoundarySegment.ElementId);
                                    ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                                    BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                                    AddBHoMObject(aBuildingElementProperties, objects);

                                    BuildingElement aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementCurve(aICurve), aLevel);
                                    aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                    if (copyCustomData)
                                        aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                                    aBuildingElmementList.Add(aBuildingElement);

                                    AddBHoMObject(aBuildingElement, objects);
                                }

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            BuildingElements = aBuildingElmementList,
                            Name = spatialElement.Name,
                            Location = (spatialElement.Location as LocationPoint).ToBHoM(convertUnits)

                        };

                        aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, spatialElement, convertUnits) as Space;

                        return aSpace;
                    }
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (spatialElement == null || spatialElementGeometryCalculator == null)
                            return null;

                        if (!SpatialElementGeometryCalculator.CanCalculateGeometry(spatialElement))
                            return null;

                        SpatialElementGeometryResults aSpatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);

                        Solid aSolid = aSpatialElementGeometryResults.GetGeometry();
                        if (aSolid == null)
                            return null;

                        oM.Architecture.Elements.Level aLevel = Query.Level(spatialElement, objects, discipline, copyCustomData, convertUnits);

                        List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
                        foreach (Face aFace in aSolid.Faces)
                        {
                            foreach (SpatialElementBoundarySubface aSpatialElementBoundarySubface in aSpatialElementGeometryResults.GetBoundaryFaceInfo(aFace))
                            {
                                Element aElement = Query.Element(spatialElement.Document, aSpatialElementBoundarySubface.SpatialBoundaryElement);
                                if (aElement == null)
                                    continue;

                                ElementType aElementType = aElement.Document.GetElement(aElement.GetTypeId()) as ElementType;

                                BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                                AddBHoMObject(aBuildingElementProperties, objects);

                                //Face aFace_BoundingElementFace = aSpatialElementBoundarySubface.GetBoundingElementFace();
                                //Face aFace_Subface = aSpatialElementBoundarySubface.GetSubface();
                                //Face aFace_SpatialElementFace = aSpatialElementBoundarySubface.GetSpatialElementFace();
                                Face aFace_BuildingElement = aSpatialElementBoundarySubface.GetSubface();
                                if (aFace_BuildingElement == null)
                                    aFace_BuildingElement = aSpatialElementBoundarySubface.GetSpatialElementFace();

                                if (aFace_BuildingElement != null)
                                    foreach (CurveLoop aCurveLoop in aFace_BuildingElement.GetEdgesAsCurveLoops())
                                    {
                                        BuildingElement aBuildingElement = null;

                                        aBuildingElement = Create.BuildingElement(aBuildingElementProperties, Create.BuildingElementPanel(aCurveLoop.ToBHoM(convertUnits)));
                                        aBuildingElement.Level = aLevel;
                                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                                        if (copyCustomData)
                                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                                        aBuildingElmementList.Add(aBuildingElement);


                                        //---- Get Hosted Building Elements -----------
                                        List<BuildingElement> aBuildingElmementList_Hosted = Query.HostedBuildingElements(aElement as HostObject, aFace_BuildingElement, objects, copyCustomData, convertUnits);
                                        if (aBuildingElmementList_Hosted != null && aBuildingElmementList_Hosted.Count > 0)
                                        {
                                            aBuildingElmementList.AddRange(aBuildingElmementList_Hosted);

                                            //------------ Cutting openings ----------------
                                            BuildingElementPanel aBuildingElementPanel = aBuildingElement.BuildingElementGeometry as BuildingElementPanel;
                                            if (aBuildingElementPanel == null)
                                                continue;

                                            foreach (BuildingElement aBuildingElement_Hosted in aBuildingElmementList_Hosted)
                                            {
                                                BuildingElementPanel aBuildingElementPanel_Hosted = aBuildingElement_Hosted.BuildingElementGeometry as BuildingElementPanel;
                                                if (aBuildingElementPanel_Hosted == null)
                                                    continue;

                                                aBuildingElementPanel.Openings.Add(new BuildingElementOpening() { PolyCurve = aBuildingElementPanel_Hosted.PolyCurve });
                                            }
                                            //---------------------------------------------
                                        }

                                        //---------------------------------------------


                                        AddBHoMObject(aBuildingElement, objects);
                                    }
                            }
                        }

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            BuildingElements = aBuildingElmementList,
                            Name = spatialElement.Name,
                            Location = (spatialElement.Location as LocationPoint).ToBHoM(convertUnits)

                        };

                        if (aBuildingElmementList != null)
                            foreach (BuildingElement aBuildingElement in aBuildingElmementList)
                                aBuildingElement.AdjacentSpaces.Add(aSpace.BHoM_Guid);

                        aSpace = Modify.SetIdentifiers(aSpace, spatialElement) as Space;
                        if (copyCustomData)
                            aSpace = Modify.SetCustomData(aSpace, spatialElement, convertUnits) as Space;

                        return aSpace;
                    }
            }

            return null;

        }

        /***************************************************/
        
        public static BHoMObject ToBHoM(this EnergyAnalysisSpace energyAnalysisSpace, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (energyAnalysisSpace == null)
                            return null;

                        SpatialElement aSpatialElement = Query.Element(energyAnalysisSpace.Document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                        if (aSpatialElement == null)
                            return null;

                        oM.Architecture.Elements.Level aLevel = Query.Level(aSpatialElement, objects, discipline, copyCustomData, convertUnits);

                        Space aSpace = new Space
                        {
                            Level = aLevel,
                            Name = energyAnalysisSpace.SpaceName,
                            Location = (aSpatialElement.Location as LocationPoint).ToBHoM(convertUnits)

                        };

                        aSpace = Modify.SetIdentifiers(aSpace, aSpatialElement) as Space;
                        if (copyCustomData)
                        {
                            aSpace = Modify.SetCustomData(aSpace, aSpatialElement, convertUnits) as Space;
                            double aInnerVolume = energyAnalysisSpace.InnerVolume;
                            double aAnalyticalVolume = energyAnalysisSpace.AnalyticalVolume;
                            if (convertUnits)
                            {
                                aInnerVolume = UnitUtils.ConvertFromInternalUnits(aInnerVolume, DisplayUnitType.DUT_CUBIC_METERS);
                                aAnalyticalVolume = UnitUtils.ConvertFromInternalUnits(aAnalyticalVolume, DisplayUnitType.DUT_CUBIC_METERS);
                            }
                                
                            aSpace = Modify.SetCustomData(aSpace, "Inner Volume", aInnerVolume) as Space;
                            aSpace = Modify.SetCustomData(aSpace, "Analytical Volume", aAnalyticalVolume) as Space;
                        }

                        if(aSpace.CustomData.ContainsKey("Number"))
                            aSpace.Number = aSpace.CustomData["Number"].ToString();                            

                        return aSpace;
                    }
            }

            return null;

        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this EnergyAnalysisSurface energyAnalysisSurface, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (energyAnalysisSurface == null)
                            return null;

                        Polyloop aPolyLoop = energyAnalysisSurface.GetPolyloop();
                        if (aPolyLoop == null)
                            return null;

                        Document aDocument = energyAnalysisSurface.Document;

                        Element aElement = Query.Element(aDocument, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId);
                        BuildingElementProperties aBuildingElementProperties = null;
                        string aName = string.Empty;
                        oM.Architecture.Elements.Level aLevel = null;


                        ElementType aElementType = null;
                        if (aElement == null)
                        {
                            EnergyAnalysisSpace aEnergyAnalysisSpace = energyAnalysisSurface.GetAnalyticalSpace();
                            SpatialElement aSpatialElement = Query.Element(aEnergyAnalysisSpace.Document, aEnergyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
                            aLevel = Query.Level(aSpatialElement, objects, discipline, copyCustomData, convertUnits);
                        }
                        else
                        {
                            aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;
                            aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                            AddBHoMObject(aBuildingElementProperties, objects);

                            aName = aElement.Name;
                            aLevel = Query.Level(aElement, objects, discipline, copyCustomData, convertUnits);
                        }
                        

                        List<Space> aSpaceList = new List<Space>();
                        List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisSurface);
                        if (aElementIdList != null && objects != null)
                        {
                            List<BHoMObject> aBHoMObjectList = null;
                            foreach(ElementId aElementId in aElementIdList)
                            if(objects.TryGetValue(aElementId, out aBHoMObjectList))
                                    if(aBHoMObjectList != null)
                                        foreach(BHoMObject aBHoMObject in aBHoMObjectList)
                                            if(aBHoMObject is Space)
                                                aSpaceList.Add(aBHoMObject as Space);


                        }

                        BuildingElement aBuildingElement = new BuildingElement
                        {
                            Level = aLevel,
                            Name = aName,
                            BuildingElementGeometry = Create.BuildingElementPanel(aPolyLoop.ToBHoM(convertUnits)),
                            BuildingElementProperties = aBuildingElementProperties,
                            AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

                        };

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (copyCustomData)
                        {
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;
                            double aHeight = energyAnalysisSurface.Height;
                            double aWidth = energyAnalysisSurface.Width;
                            double aAzimuth = energyAnalysisSurface.Azimuth;
                            if(convertUnits)
                            {
                                aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                                aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                            }
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Azimuth", aAzimuth) as BuildingElement;
                            if (aElementType != null)
                                aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElement;
                        }
                            

                        return new List<BHoMObject>(new BHoMObject[] { aBuildingElement });
                    }
            }

            return null;

        }

        /***************************************************/

        public static List<BHoMObject> ToBHoM(this EnergyAnalysisOpening energyAnalysisOpening, Dictionary<ElementId, List<BHoMObject>> objects = null, Discipline discipline = Discipline.Environmental, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Environmental:
                    {
                        if (energyAnalysisOpening == null)
                            return null;

                        Polyloop aPolyLoop = energyAnalysisOpening.GetPolyloop();
                        if (aPolyLoop == null)
                            return null;

                        Document aDocument = energyAnalysisOpening.Document;

                        Element aElement = Query.Element(aDocument, energyAnalysisOpening.CADObjectUniqueId, energyAnalysisOpening.CADLinkUniqueId);

                        if (aElement == null)
                            return null;

                        ElementType aElementType = aDocument.GetElement(aElement.GetTypeId()) as ElementType;

                        BuildingElementProperties aBuildingElementProperties = aElementType.ToBHoM(objects, discipline, copyCustomData, convertUnits) as BuildingElementProperties;
                        AddBHoMObject(aBuildingElementProperties, objects);

                        List<Space> aSpaceList = new List<Space>();
                        List<ElementId> aElementIdList = Query.SpatialElementIds(energyAnalysisOpening.GetAnalyticalSurface());
                        if (aElementIdList != null && objects != null)
                        {
                            List<BHoMObject> aBHoMObjectList = null;
                            foreach (ElementId aElementId in aElementIdList)
                                if (objects.TryGetValue(aElementId, out aBHoMObjectList))
                                    if (aBHoMObjectList != null)
                                        foreach (BHoMObject aBHoMObject in aBHoMObjectList)
                                            if (aBHoMObject is Space)
                                                aSpaceList.Add(aBHoMObject as Space);


                        }

                        oM.Architecture.Elements.Level aLevel = Query.Level(aElement, objects, discipline, copyCustomData, convertUnits);

                        BuildingElement aBuildingElement = new BuildingElement
                        {
                            Level = aLevel,
                            Name = aElement.Name,
                            BuildingElementProperties = aBuildingElementProperties,
                            BuildingElementGeometry = Create.BuildingElementPanel(aPolyLoop.ToBHoM(convertUnits)),
                            AdjacentSpaces = aSpaceList.ConvertAll(x => x.BHoM_Guid)

                        };

                        aBuildingElement = Modify.SetIdentifiers(aBuildingElement, aElement) as BuildingElement;
                        if (copyCustomData)
                        {
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElement, convertUnits) as BuildingElement;

                            double aHeight = energyAnalysisOpening.Height;
                            double aWidth = energyAnalysisOpening.Width;
                            if (convertUnits)
                            {
                                aHeight = UnitUtils.ConvertFromInternalUnits(aHeight, DisplayUnitType.DUT_METERS);
                                aWidth = UnitUtils.ConvertFromInternalUnits(aWidth, DisplayUnitType.DUT_METERS);
                            }
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Height", aHeight) as BuildingElement;
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Width", aWidth) as BuildingElement;
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Type", energyAnalysisOpening.OpeningType.ToString()) as BuildingElement;
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, "Opening Name", energyAnalysisOpening.OpeningName) as BuildingElement;
                            aBuildingElement = Modify.SetCustomData(aBuildingElement, aElementType, BuiltInParameter.ALL_MODEL_FAMILY_NAME, convertUnits) as BuildingElement;
                        }
                            

                        return new List<BHoMObject>(new BHoMObject[] { aBuildingElement });
                    }
            }

            return null;
        }

        /***************************************************/

        public static BHoMObject ToBHoM(this Grid grid, Discipline discipline = Discipline.Architecture, bool copyCustomData = true, bool convertUnits = true)
        {
            switch (discipline)
            {
                case Discipline.Architecture:
                case Discipline.Environmental:
                case Discipline.Structural:
                    return grid.ToBHoMGrid(copyCustomData, convertUnits);
            }
            return null;
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static List<BHoMObject> GetBHoMObjects(ElementId elementId, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (objects == null || elementId == null)
                return null;

            List<BHoMObject> aResult = null;
            if (objects.TryGetValue(elementId, out aResult))
                return aResult;

            return null;
        }

        /***************************************************/

        private static void AddBHoMObject(BHoMObject bHoMObject, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (objects == null || bHoMObject == null)
                return;

            ElementId aElementId = Query.ElementId(bHoMObject);
            if (aElementId == null)
                aElementId = Autodesk.Revit.DB.ElementId.InvalidElementId;

            List<BHoMObject> aResult = null;
            if (objects.TryGetValue(aElementId, out aResult))
            {
                if (aResult == null)
                    aResult = new List<BHoMObject>();

                if (aResult.Find(x => x != null && x.BHoM_Guid == bHoMObject.BHoM_Guid) == null)
                    aResult.Add(bHoMObject);
            }
            else
            {
                objects.Add(aElementId, new List<BHoMObject>(new BHoMObject[] { bHoMObject }));
            }
        }
        
        /***************************************************/

        private static void AddBHoMObjects(IEnumerable<BHoMObject> bHoMObjects, Dictionary<ElementId, List<BHoMObject>> objects)
        {
            if (bHoMObjects == null)
                return;

            foreach (BHoMObject aBHoMObject in bHoMObjects)
                AddBHoMObject(aBHoMObject, objects);
        }

        /***************************************************/
        
        private class WarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor FailuresAccessor)
            {
                IList<FailureMessageAccessor> aFailureMessageAccessors = FailuresAccessor.GetFailureMessages();

                foreach (FailureMessageAccessor aFailureMessageAccessor in aFailureMessageAccessors)
                {
                    FailuresAccessor.DeleteWarning(aFailureMessageAccessor);
                }
                return FailureProcessingResult.Continue;
            }
        }
    }
}
 