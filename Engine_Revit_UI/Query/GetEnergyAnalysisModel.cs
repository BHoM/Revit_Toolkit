/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.Properties;

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

                //Reseting Project Base Point
                IEnumerable<Element> aElementList = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_ProjectBasePoint);
                foreach (Element aElement in aElementList)
                {
                    if (aElement.Pinned)
                        aElement.Pinned = false;

                    Parameter aParameter = null;

                    aParameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
                    if (aParameter != null && !aParameter.IsReadOnly)
                        aParameter.Set(0.0);

                    aParameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
                    if (aParameter != null && !aParameter.IsReadOnly)
                        aParameter.Set(0.0);

                    aParameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                    if (aParameter != null && !aParameter.IsReadOnly)
                        aParameter.Set(0.0);

                    aParameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                    if(aParameter != null && !aParameter.IsReadOnly)
                        aParameter.Set(0.0);

                    aParameter = aElement.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
                    if (aParameter != null && !aParameter.IsReadOnly)
                        aParameter.Set(0.0);
                }

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

                            oM.Environment.Elements.Opening aOpening = aEnergyAnalysisOpening.ToBHoMOpening(pullSettings);
                            aBuildingElement.Openings.Add(aOpening);
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

                            oM.Environment.Elements.Opening aOpening = aEnergyAnalysisOpening.ToBHoMOpening(pullSettings);
                            aBuildingElement.Openings.Add(aOpening);
                        }
                    }
                    catch (Exception aException)
                    {
                        aEnergyAnalysisSurface.ElementCouldNotBeQueriedWarning();
                    }

                }

                aTransaction.RollBack();
            }

            //Levels
            IEnumerable<Level> aLevels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>();
            foreach (Level aLevel in aLevels)
                Convert.ToBHoMLevel(aLevel, pullSettings);

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