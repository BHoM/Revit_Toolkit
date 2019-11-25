/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Environment.Fragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<IBHoMObject> GetEnergyAnalysisModel(this Document document, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();
                
            if(pullSettings.RefObjects == null)
                pullSettings.RefObjects = new Dictionary<int, List<IBHoMObject>>();

            using (Transaction transaction = new Transaction(document, "GetAnalyticalModel"))
            {
                transaction.Start();

                FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
                failureHandlingOptions.SetFailuresPreprocessor(new WarningSwallower());
                transaction.SetFailureHandlingOptions(failureHandlingOptions);

                EnergyAnalysisDetailModel energyAnalysisDetailModel = null;

                using (SubTransaction subTransaction = new SubTransaction(document))
                {
                    subTransaction.Start();
                    energyAnalysisDetailModel = EnergyAnalysisDetailModel.GetMainEnergyAnalysisDetailModel(document);
                    if (energyAnalysisDetailModel != null && energyAnalysisDetailModel.IsValidObject)
                        document.Delete(energyAnalysisDetailModel.Id);

                    subTransaction.Commit();
                }

                EnergyAnalysisDetailModelOptions energyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
                energyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
                energyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
                energyAnalysisDetailModelOptions.ExportMullions = true;
                energyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
                energyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

                EnergyDataSettings energyDataSettings = EnergyDataSettings.GetFromDocument(document);
                energyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                energyDataSettings.ExportDefaults = false;
                energyDataSettings.SliverSpaceTolerance = Convert.FromSI(0.005, UnitType.UT_Length);
                energyDataSettings.AnalysisType = AnalysisMode.BuildingElements;
                energyDataSettings.EnergyModel = false;

                //Reseting Project Base Point
                IEnumerable<Element> elements = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_ProjectBasePoint);
                foreach (Element element in elements)
                {
                    if (element.Pinned)
                        element.Pinned = false;

                    Parameter parameter = null;

                    parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM);
                    if (parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);

                    parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM);
                    if (parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);

                    parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                    if (parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);

                    parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM);
                    if(parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);

                    parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
                    if (parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);
                }

                //AnalyticalSpaces
                energyAnalysisDetailModel = EnergyAnalysisDetailModel.Create(document, energyAnalysisDetailModelOptions);
                IList<EnergyAnalysisSpace> energyAnalysisSpaces = energyAnalysisDetailModel.GetAnalyticalSpaces();
                Dictionary<string, EnergyAnalysisSurface> energyAnalsyisSurfaces = new Dictionary<string, EnergyAnalysisSurface>();
                foreach (EnergyAnalysisSpace energyAnalysisSpace in energyAnalysisSpaces)
                {
                    try
                    {
                        Space space = energyAnalysisSpace.ToBHoMSpace(pullSettings);

                        foreach (EnergyAnalysisSurface energyAnalysisSurface in energyAnalysisSpace.GetAnalyticalSurfaces())
                        {
                            if (!energyAnalsyisSurfaces.ContainsKey(energyAnalysisSurface.SurfaceName))
                                energyAnalsyisSurfaces.Add(energyAnalysisSurface.SurfaceName, energyAnalysisSurface);
                        }
                    }
                    catch
                    {
                        energyAnalysisSpace.ElementCouldNotBeQueriedWarning();
                    }
                }

                //EnergyAnalysisSurfaces
                foreach (KeyValuePair<string, EnergyAnalysisSurface> kvp in energyAnalsyisSurfaces)
                {
                    try
                    {
                        oM.Environment.Elements.Panel panel = kvp.Value.ToBHoMEnvironmentPanel(pullSettings);

                        List<IBHoMObject> hostedBHoMObjects = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening energyAnalysisOpening in kvp.Value.GetAnalyticalOpenings())
                        {
                            oM.Environment.Elements.Opening opening = energyAnalysisOpening.ToBHoMOpening(pullSettings);
                            panel.Openings.Add(opening);
                        }
                    }
                    catch
                    {
                        kvp.Value.ElementCouldNotBeQueriedWarning();
                    }
                }

                //AnalyticalShadingSurfaces
                IList<EnergyAnalysisSurface> analyticalShadingSurfaces = energyAnalysisDetailModel.GetAnalyticalShadingSurfaces();
                foreach (EnergyAnalysisSurface energyAnalysisSurface in analyticalShadingSurfaces)
                {
                    try
                    {
                        oM.Environment.Elements.Panel panel = energyAnalysisSurface.ToBHoMEnvironmentPanel(pullSettings);

                        List<IBHoMObject> hostedBHoMObjects = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening energyOpening in energyAnalysisSurface.GetAnalyticalOpenings())
                        {
                            oM.Environment.Elements.Opening opening = energyOpening.ToBHoMOpening(pullSettings);
                            panel.Openings.Add(opening);
                        }
                    }
                    catch
                    {
                        energyAnalysisSurface.ElementCouldNotBeQueriedWarning();
                    }

                }

                transaction.RollBack();
            }

            //Levels
            IEnumerable<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>();
            foreach (Level level in levels)
                Convert.ToBHoMLevel(level, pullSettings);

            List<IBHoMObject> result = new List<IBHoMObject>();
            foreach (List<IBHoMObject> bhomObjects in pullSettings.RefObjects.Values)
                if (bhomObjects != null)
                    result.AddRange(bhomObjects);

            return result;
        }


        /***************************************************/
        /****              Private Classes              ****/
        /***************************************************/

        private class WarningSwallower : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                IList<FailureMessageAccessor> failureMessageAccessors = failuresAccessor.GetFailureMessages();

                foreach (FailureMessageAccessor failureMessageAccessor in failureMessageAccessors)
                {
                    failuresAccessor.DeleteWarning(failureMessageAccessor);
                }
                return FailureProcessingResult.Continue;
            }
        }

        /***************************************************/
    }
}