using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Revit;
using System;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<IBHoMObject> GetEnergyAnalysisModel(this Document document, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;
                
            if(pullSettings.RefObjects == null)
                pullSettings.RefObjects = new Dictionary<int, List<IBHoMObject>>();

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
                        document.Delete(aEnergyAnalysisDetailModel.Id);

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
                    try
                    {
                        Space aSpace = aEnergyAnalysisSpace.ToBHoMSpace(pullSettings);
                        pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aSpace);

                        foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aEnergyAnalysisSpace.GetAnalyticalSurfaces())
                            if (!aEnergyAnalysisSurfaces.ContainsKey(aEnergyAnalysisSurface.SurfaceName))
                                aEnergyAnalysisSurfaces.Add(aEnergyAnalysisSurface.SurfaceName, aEnergyAnalysisSurface);
                    }
                    catch(Exception aException)
                    {
                        aEnergyAnalysisSpace.ElementCouldNotBeQueried();
                    }
                }

                //EnergyAnalysisSurfaces
                foreach (KeyValuePair<string, EnergyAnalysisSurface> aKeyValuePair in aEnergyAnalysisSurfaces)
                {
                    try
                    {
                        BuildingElement aBuildingElement = aKeyValuePair.Value.ToBHoMBuildingElement(pullSettings);
                        pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElement);

                        List<IBHoMObject> aBHoMObjectList_Hosted = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aKeyValuePair.Value.GetAnalyticalOpenings())
                        {
                            BuildingElement aBuildingElement_Opening = aEnergyAnalysisOpening.ToBHoMBuildingElement(pullSettings);
                            if (aBuildingElement_Opening != null)
                            {
                                aBHoMObjectList_Hosted.Add(aBuildingElement_Opening);
                                pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElement_Opening);
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

                                if (pullSettings.CopyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(Convert.ElementId))
                                    aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, Convert.ElementId, aBuildingElement_Hosted.CustomData[Convert.ElementId]) as BuildingElementOpening;

                                aBuildingElementPanel.Openings.Add(aBuildingElementOpening);
                            }

                        }
                    }
                    catch (Exception aException)
                    {
                        aKeyValuePair.Value.ElementCouldNotBeQueried();
                    }
                }


                //AnalyticalShadingSurfaces
                IList<EnergyAnalysisSurface> aAnalyticalShadingSurfaceList = aEnergyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
                foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aAnalyticalShadingSurfaceList)
                {
                    try
                    {
                        BuildingElement aBuildingElement = aEnergyAnalysisSurface.ToBHoMBuildingElement(pullSettings);
                        pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElement);

                        List<IBHoMObject> aBHoMObjectList_Hosted = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aEnergyAnalysisSurface.GetAnalyticalOpenings())
                        {
                            BuildingElement aBuildingElement_Opening = aEnergyAnalysisOpening.ToBHoMBuildingElement(pullSettings);
                            if (aBuildingElement_Opening != null)
                            {
                                aBHoMObjectList_Hosted.Add(aBuildingElement_Opening);
                                pullSettings.RefObjects = Modify.AddBHoMObject(pullSettings.RefObjects, aBuildingElement_Opening);
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

                                if (pullSettings.CopyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(Convert.ElementId))
                                    aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, Convert.ElementId, aBuildingElement_Hosted.CustomData[Convert.ElementId]) as BuildingElementOpening;

                                aBuildingElementPanel.Openings.Add(aBuildingElementOpening);
                            }

                        }
                        //-------------------------------------------
                    }
                    catch (Exception aException)
                    {
                        aEnergyAnalysisSurface.ElementCouldNotBeQueried();
                    }

                }

                aTransaction.RollBack();
            }

            List<IBHoMObject> aResult = new List<IBHoMObject>();
            foreach (List<IBHoMObject> aBHoMObjectList in pullSettings.RefObjects.Values)
                if (aBHoMObjectList != null)
                    aResult.AddRange(aBHoMObjectList);

            return aResult;
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