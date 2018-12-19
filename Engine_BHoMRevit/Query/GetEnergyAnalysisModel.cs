using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<IBHoMObject> GetEnergyAnalysisModel(this Document document, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();
                
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

                        foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aEnergyAnalysisSpace.GetAnalyticalSurfaces())
                            if (!aEnergyAnalysisSurfaces.ContainsKey(aEnergyAnalysisSurface.SurfaceName))
                                aEnergyAnalysisSurfaces.Add(aEnergyAnalysisSurface.SurfaceName, aEnergyAnalysisSurface);
                    }
                    catch(Exception aException)
                    {
                        aEnergyAnalysisSpace.ElementCouldNotBeQueriedWarning();
                    }
                }

                //EnergyAnalysisSurfaces
                foreach (KeyValuePair<string, EnergyAnalysisSurface> aKeyValuePair in aEnergyAnalysisSurfaces)
                {
                    try
                    {
                        BuildingElement aBuildingElement = aKeyValuePair.Value.ToBHoMBuildingElement(pullSettings);

                        List<IBHoMObject> aBHoMObjectList_Hosted = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aKeyValuePair.Value.GetAnalyticalOpenings())
                        {
                            BuildingElement aBuildingElement_Opening = aEnergyAnalysisOpening.ToBHoMBuildingElement(aKeyValuePair.Value, pullSettings);

                            if (aBuildingElement_Opening != null)
                                aBHoMObjectList_Hosted.Add(aBuildingElement_Opening);
                        }

                        //------------ Cutting openings ----------------
                        if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                        {
                            foreach (BuildingElement aBuildingElement_Hosted in aBHoMObjectList_Hosted.FindAll(x => x is BuildingElement))
                            {
                                oM.Environment.Elements.Opening aBuildingElementOpening = BH.Engine.Environment.Create.Opening(aBuildingElement_Hosted.PanelCurve);

                                if (pullSettings.CopyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(BH.Engine.Adapters.Revit.Convert.ElementId))
                                    aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, BH.Engine.Adapters.Revit.Convert.ElementId, aBuildingElement_Hosted.CustomData[BH.Engine.Adapters.Revit.Convert.ElementId]) as BH.oM.Environment.Elements.Opening;

                                aBuildingElement.Openings.Add(aBuildingElementOpening);
                            }

                        }
                    }
                    catch (Exception aException)
                    {
                        aKeyValuePair.Value.ElementCouldNotBeQueriedWarning();
                    }
                }

                //AnalyticalShadingSurfaces
                IList<EnergyAnalysisSurface> aAnalyticalShadingSurfaceList = aEnergyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
                foreach (EnergyAnalysisSurface aEnergyAnalysisSurface in aAnalyticalShadingSurfaceList)
                {
                    try
                    {
                        BuildingElement aBuildingElement = aEnergyAnalysisSurface.ToBHoMBuildingElement(pullSettings);

                        List<IBHoMObject> aBHoMObjectList_Hosted = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening aEnergyAnalysisOpening in aEnergyAnalysisSurface.GetAnalyticalOpenings())
                        {
                            BuildingElement aBuildingElement_Opening = aEnergyAnalysisOpening.ToBHoMBuildingElement(aEnergyAnalysisSurface, pullSettings);
                            if (aBuildingElement_Opening != null)
                                aBHoMObjectList_Hosted.Add(aBuildingElement_Opening);
                        }

                        //------------ Cutting openings ----------------
                        if (aBHoMObjectList_Hosted != null && aBHoMObjectList_Hosted.Count > 0)
                        {
                            foreach (BuildingElement aBuildingElement_Hosted in aBHoMObjectList_Hosted.FindAll(x => x is BuildingElement))
                            {
                                BH.oM.Environment.Elements.Opening aBuildingElementOpening = BH.Engine.Environment.Create.Opening(aBuildingElement_Hosted.PanelCurve);

                                if (pullSettings.CopyCustomData && aBuildingElement_Hosted.CustomData.ContainsKey(BH.Engine.Adapters.Revit.Convert.ElementId))
                                    aBuildingElementOpening = Modify.SetCustomData(aBuildingElementOpening, BH.Engine.Adapters.Revit.Convert.ElementId, aBuildingElement_Hosted.CustomData[BH.Engine.Adapters.Revit.Convert.ElementId]) as BH.oM.Environment.Elements.Opening;

                                aBuildingElement.Openings.Add(aBuildingElementOpening);
                            }

                        }
                        //-------------------------------------------
                    }
                    catch (Exception aException)
                    {
                        aEnergyAnalysisSurface.ElementCouldNotBeQueriedWarning();
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