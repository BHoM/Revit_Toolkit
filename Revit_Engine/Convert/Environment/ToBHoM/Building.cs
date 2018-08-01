using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.Engine.Environment;
using BH.oM.Adapters.Revit;
using BH.oM.Environment.Properties;
using Autodesk.Revit.DB.Analysis;
using System;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Public methods                            ****/
        /***************************************************/

        public static Building ToBHoMBuilding(this Document document, bool copyCustomData = true, bool convertUnits = true)
        {
            double aElevation = 0;
            double aLongitude = 0;
            double aLatitude = 0;
            double aTimeZone = 0;
            string aPlaceName = string.Empty;
            string aWeatherStationName = string.Empty;

            if (document.SiteLocation != null)
            {
                aElevation = document.SiteLocation.Elevation;
                aLongitude = document.SiteLocation.Longitude;
                aLatitude = document.SiteLocation.Latitude;
                aTimeZone = document.SiteLocation.TimeZone;
                aPlaceName = document.SiteLocation.PlaceName;
                aWeatherStationName = document.SiteLocation.WeatherStationName;

                if (convertUnits)
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

            if (document.ActiveProjectLocation != null)
            {
                ProjectLocation aProjectLocation = document.ActiveProjectLocation;
                XYZ aXYZ = new XYZ(0, 0, 0);
                ProjectPosition aProjectPosition = aProjectLocation.GetProjectPosition(aXYZ);
                if (aProjectPosition != null)
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
                    Space aSpace = aEnergyAnalysisSpace.ToBHoM(aObjects, Discipline.Environmental, copyCustomData, convertUnits) as Space;
                    AddBHoMObject(aSpace, aObjects);

                    foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aEnergyAnalysisSpace.GetAnalyticalSurfaces())
                        if (!aEnergyAnalysisSurfaces.ContainsKey(aEnergyAnalysisSurface.SurfaceName))
                            aEnergyAnalysisSurfaces.Add(aEnergyAnalysisSurface.SurfaceName, aEnergyAnalysisSurface);
                }

                //EnergyAnalysisSurfaces
                foreach (KeyValuePair<string, EnergyAnalysisSurface> aKeyValuePair in aEnergyAnalysisSurfaces)
                {
                    BuildingElement aBuildingElement = aKeyValuePair.Value.ToBHoMBuildingElement(aObjects, copyCustomData, convertUnits);
                    AddBHoMObject(aBuildingElement, aObjects);

                    List<BHoMObject> aBHoMObjectList_Hosted = new List<BHoMObject>();
                    foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aKeyValuePair.Value.GetAnalyticalOpenings())
                    {
                        BuildingElement aBuildingElement_Opening = aEnergyAnalysisOpening.ToBHoMBuildingElement(aObjects, copyCustomData, convertUnits);
                        if (aBuildingElement_Opening != null)
                        {
                            aBHoMObjectList_Hosted.Add(aBuildingElement_Opening);
                            AddBHoMObject(aBuildingElement_Opening, aObjects);
                        }
                    }

                    //------------ Cutting openings ----------------
                    if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                    {
                        BuildingElementPanel aBuildingElementPanel = aBuildingElement.BuildingElementGeometry as BuildingElementPanel;
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

                            if (copyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(Convert.ElementId))
                                aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, Convert.ElementId, aBuildingElement_Hosted.CustomData[Convert.ElementId]) as BuildingElementOpening;

                            aBuildingElementPanel.Openings.Add(aBuildingElementOpening);
                        }

                    }
                    //-------------------------------------------
                }


                //AnalyticalShadingSurfaces
                IList<EnergyAnalysisSurface> aAnalyticalShadingSurfaceList = aEnergyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
                foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aAnalyticalShadingSurfaceList)
                {
                    BuildingElement aBuildingElement = aEnergyAnalysisSurface.ToBHoMBuildingElement(aObjects, copyCustomData, convertUnits);
                    AddBHoMObject(aBuildingElement, aObjects);

                    List<BHoMObject> aBHoMObjectList_Hosted = new List<BHoMObject>();
                    foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aEnergyAnalysisSurface.GetAnalyticalOpenings())
                    {
                        BuildingElement aBuildingElement_Opening = aEnergyAnalysisOpening.ToBHoMBuildingElement(aObjects, copyCustomData, convertUnits);
                        if (aBuildingElement_Opening != null)
                        {
                            aBHoMObjectList_Hosted.Add(aBuildingElement_Opening);
                            AddBHoMObject(aBuildingElement_Opening, aObjects);
                        }
                    }

                    //------------ Cutting openings ----------------
                    if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                    {

                        BuildingElementPanel aBuildingElementPanel = aBuildingElement.BuildingElementGeometry as BuildingElementPanel;
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

                            if (copyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(Convert.ElementId))
                                aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, Convert.ElementId, aBuildingElement_Hosted.CustomData[Convert.ElementId]) as BuildingElementOpening;

                            aBuildingElementPanel.Openings.Add(aBuildingElementOpening);
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
                        if (aSpace != null)
                        {
                            if (aSpace.BuildingElements.Find(x => x.BHoM_Guid == aBuildingElement.BHoM_Guid) == null)
                                aSpace.BuildingElements.Add(aBuildingElement);
                        }
                    }

            }


            //---------------------------------------------

            return aBuilding;
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
        /**** Private Classes                           ****/
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

        /***************************************************/
    }
}
