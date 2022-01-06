/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.Elements;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts all Revit elements contained within EnergyAnalysisDetailModel to a collection of BH.oM.Environment.Elements objects.")]
        [Input("energyAnalysisModel", "Revit EnergyAnalysisDetailModel to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("elements", "Collection of BH.oM.Environment.Elements objects resulting from converting the input Revit EnergyAnalysisDetailModel.")]
        public static List<IBHoMObject> EnergyAnalysisModelFromRevit(this EnergyAnalysisDetailModel energyAnalysisModel, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (energyAnalysisModel.Document.IsLinked)
            {
                BH.Engine.Reflection.Compute.RecordError($"It is not allowed to pull the energy analysis model from linked models - please open the document {energyAnalysisModel.Document.PathName} and pull directly from it instead.");
                return null;
            }

            settings = settings.DefaultIfNull();

            ElementId modelId = energyAnalysisModel.Id;
            List<IBHoMObject> result = refObjects.GetValues<IBHoMObject>(energyAnalysisModel.Id);
            if (result != null)
                return result;

            Document document = energyAnalysisModel.Document;

            ProjectInfo projectInfo = new FilteredElementCollector(document).OfClass(typeof(ProjectInfo)).FirstOrDefault() as ProjectInfo;
            if (projectInfo == null)
                BH.Engine.Reflection.Compute.RecordError("Project info of a document has not been found.");
            else
                projectInfo.BuildingFromRevit(settings, refObjects);
                
            using (Transaction transaction = new Transaction(document, "GetAnalyticalModel"))
            {
                transaction.Start();

                FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions();
                failureHandlingOptions.SetFailuresPreprocessor(new WarningSwallower());
                transaction.SetFailureHandlingOptions(failureHandlingOptions);
                
                EnergyAnalysisDetailModelOptions energyAnalysisDetailModelOptions = new EnergyAnalysisDetailModelOptions();
                energyAnalysisDetailModelOptions.Tier = EnergyAnalysisDetailModelTier.SecondLevelBoundaries;
                energyAnalysisDetailModelOptions.EnergyModelType = EnergyModelType.SpatialElement;
                energyAnalysisDetailModelOptions.ExportMullions = true;
                energyAnalysisDetailModelOptions.IncludeShadingSurfaces = true;
                energyAnalysisDetailModelOptions.SimplifyCurtainSystems = false;

                EnergyDataSettings energyDataSettings = EnergyDataSettings.GetFromDocument(document);
                energyDataSettings.ExportComplexity = gbXMLExportComplexity.ComplexWithMullionsAndShadingSurfaces;
                energyDataSettings.ExportDefaults = false;
                energyDataSettings.SliverSpaceTolerance = Convert.FromSI(0.005, SpecTypeId.Length);
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
                    if (parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);

                    parameter = element.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
                    if (parameter != null && !parameter.IsReadOnly)
                        parameter.Set(0.0);
                }

                //AnalyticalSpaces
                energyAnalysisModel = EnergyAnalysisDetailModel.Create(document, energyAnalysisDetailModelOptions);
                IList<EnergyAnalysisSpace> energyAnalysisSpaces = energyAnalysisModel.GetAnalyticalSpaces();
                Dictionary<string, EnergyAnalysisSurface> energyAnalsyisSurfaces = new Dictionary<string, EnergyAnalysisSurface>();
                foreach (EnergyAnalysisSpace energyAnalysisSpace in energyAnalysisSpaces)
                {
                    try
                    {
                        Space space = energyAnalysisSpace.SpaceFromRevit(settings, refObjects);

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
                        oM.Environment.Elements.Panel panel = kvp.Value.EnvironmentPanelFromRevit(settings, refObjects);

                        List<IBHoMObject> hostedBHoMObjects = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening energyAnalysisOpening in kvp.Value.GetAnalyticalOpenings())
                        {
                            oM.Environment.Elements.Opening opening = energyAnalysisOpening.OpeningFromRevit(settings, refObjects);
                            panel.Openings.Add(opening);
                        }
                    }
                    catch
                    {
                        kvp.Value.ElementCouldNotBeQueriedWarning();
                    }
                }

                //AnalyticalShadingSurfaces
                IList<EnergyAnalysisSurface> analyticalShadingSurfaces = energyAnalysisModel.GetAnalyticalShadingSurfaces();
                foreach (EnergyAnalysisSurface energyAnalysisSurface in analyticalShadingSurfaces)
                {
                    try
                    {
                        oM.Environment.Elements.Panel panel = energyAnalysisSurface.EnvironmentPanelFromRevit(settings, refObjects);

                        List<IBHoMObject> hostedBHoMObjects = new List<IBHoMObject>();
                        foreach (EnergyAnalysisOpening energyOpening in energyAnalysisSurface.GetAnalyticalOpenings())
                        {
                            oM.Environment.Elements.Opening opening = energyOpening.OpeningFromRevit(settings, refObjects);
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
            {
                level.LevelFromRevit(settings, refObjects);
            }

            result = new List<IBHoMObject>();
            foreach (List<IBHoMObject> bhomObjects in refObjects.Values)
                if (bhomObjects != null)
                    result.AddRange(bhomObjects);
            
            refObjects.AddOrReplace(modelId, result);
            return result;
        }

        /***************************************************/
    }
}

