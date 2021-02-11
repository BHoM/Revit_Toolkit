/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.MEP.System.SectionProperties;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Autodesk.Revit.DB.Plumbing.Pipe ToRevitPipe(this oM.MEP.System.Pipe pipe, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (document == null)
                return null;

            // Check valid pipe object
            if (pipe == null)
                return null;

            // Construct Revit Pipe
            Autodesk.Revit.DB.Plumbing.Pipe revitPipe = refObjects.GetValue<Autodesk.Revit.DB.Plumbing.Pipe>(document, pipe.BHoM_Guid);
            if (revitPipe != null)
                return revitPipe;

            // Settings
            settings = settings.DefaultIfNull();

            // Pipe type
            Autodesk.Revit.DB.Plumbing.PipeType pipeType = pipe.ToRevitElementType(document, new List<BuiltInCategory> { BuiltInCategory.OST_PipingSystem }, settings, refObjects) as Autodesk.Revit.DB.Plumbing.PipeType;
            if(pipeType == null)
            {
                BH.Engine.Reflection.Compute.RecordError("No valid family has been found in the Revit model. Pipe creation requires the presence of the default Pipe Family Type.");
                return null;
            }

            // End points
            XYZ start = pipe.StartPoint.Position.ToRevit();
            XYZ end = pipe.EndPoint.Position.ToRevit();

            // Level
            Level level = document.LevelBelow(Math.Min(start.Z, end.Z), settings);
            if (level == null)
                return null;

            // Default system used for now
            // TODO: in the future you could look for the existing connectors and check if any of them overlaps with start/end of this pipe - if so, use it in Pipe.Create.
            // hacky/heavy way of getting all connectors in the link below - however, i would rather filter the connecting elements out by type/bounding box first for performance reasons
            // https://thebuildingcoder.typepad.com/blog/2010/06/retrieve-mep-elements-and-connectors.html

            Autodesk.Revit.DB.Plumbing.PipingSystemType pst = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipingSystemType)).OfType<Autodesk.Revit.DB.Plumbing.PipingSystemType>().FirstOrDefault();

            if (pst == null)
            {
                BH.Engine.Reflection.Compute.RecordError("No valid PipingSystemType can be found in the Revit model. Creating a revit Pipe requires a PipingSystemType.");
                return null;
            }

            BH.Engine.Reflection.Compute.RecordWarning("Pipe creation will utilise the first available PipingSystemType from the Revit model.");

            List<SectionProfile> sectionProfile = pipe.SectionProfile;
            if (sectionProfile == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Pipe creation requires a SectionProfile. \n No elements created.");
                return null;
            }

            // Create Revit Pipe
            revitPipe = Autodesk.Revit.DB.Plumbing.Pipe.Create(document, pst.Id, pipeType.Id, level.Id, start, end);
            if (revitPipe == null)
            {
                BH.Engine.Reflection.Compute.RecordError("No Revit Pipe has been created. Please check inputs prior to push attempt.");
                return null;
            }

            // Copy parameters from BHoM object to Revit element
            revitPipe.CopyParameters(pipe, settings);

            double flowRate = pipe.Flow.Select(x => x.FlowRate).Sum();
            revitPipe.SetParameter(BuiltInParameter.RBS_PIPE_FLOW_PARAM, flowRate);

            // Get first available pipeLiningType from document
            Autodesk.Revit.DB.Plumbing.PipeInsulationType pit = null;
            if (pipe.SectionProfile.Where(x => x.Type == oM.MEP.Enums.ProfileType.Lining).Count() > 0)
            {
                pit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipeInsulationType)).FirstOrDefault() as Autodesk.Revit.DB.Plumbing.PipeInsulationType;
                if (pit == null)
                {
                    BH.Engine.Reflection.Compute.RecordError("Any pipe insulation type needs to be present in the Revit model in order to push pipes with insulation.\n" +
                        "Pipe has been created but no insulation has been applied.");
                }
            }

            ShapeType elementShape = pipe.ElementSize.Shape;

            // Round Pipe 
            if (elementShape is ShapeType.Tube)
            {
                //TubeProfile elementProfile = sectionProfile.ElementProfile as TubeProfile;

                // Set Element Diameter
                double diameter = pipe.ElementSize.Diameter;
                revitPipe.SetParameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM, diameter);

                // Outer and Inner Diameters
                double elementThickness = pipe.SectionProfile.Where(x => x.Type == BH.oM.MEP.Enums.ProfileType.Element).First().Layer.Select(x => x.Thickness).Sum();
                double outDiameter = pipe.ElementSize.Diameter;
                double inDiameter = (((outDiameter / 2) - elementThickness) * 2);
                revitPipe.SetParameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER, outDiameter);
                revitPipe.SetParameter(BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM, inDiameter);

                // Set InsulationProfile
                if (pit != null)
                {
                    //TubeProfile insulationProfile = sectionProfile.InsulationProfile as TubeProfile;
                    double insulationThickness = pipe.SectionProfile.Where(x => x.Type == BH.oM.MEP.Enums.ProfileType.Insulation).First().Layer.Select(x => x.Thickness).Sum();
                    // Create pipe Insulation
                    Autodesk.Revit.DB.Plumbing.PipeInsulation pIn = Autodesk.Revit.DB.Plumbing.PipeInsulation.Create(document, revitPipe.Id, pit.Id, insulationThickness);
                }       
            }

            refObjects.AddOrReplace(pipe, revitPipe);
            return revitPipe;
        }

        /***************************************************/
    }
}

