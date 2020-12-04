/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
            Autodesk.Revit.DB.Plumbing.PipeType pipeType = pipe.SectionProperty.ToRevitElementType(document, new List<BuiltInCategory> { BuiltInCategory.OST_PipingSystem }, settings, refObjects) as Autodesk.Revit.DB.Plumbing.PipeType;

            // End points
            XYZ start = pipe.StartPoint.ToRevit();
            XYZ end = pipe.EndPoint.ToRevit();

            // Level
            Level level = document.LevelBelow(Math.Min(start.Z, end.Z), settings);
            if (level == null)
                return null;

            // Default system used for now
            // TODO: in the future you could look for the existing connectors and check if any of them overlaps with start/end of this pipe - if so, use it in Pipe.Create.
            // hacky/heavy way of getting all connectors in the link below - however, i would rather filter the connecting elements out by type/bounding box first for performance reasons
            // https://thebuildingcoder.typepad.com/blog/2010/06/retrieve-mep-elements-and-connectors.html

            Autodesk.Revit.DB.Plumbing.PipingSystemType pst = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipingSystemType)).OfType<Autodesk.Revit.DB.Plumbing.PipingSystemType>().FirstOrDefault();

            revitPipe = Autodesk.Revit.DB.Plumbing.Pipe.Create(document, pst.Id, pipeType.Id, level.Id, start, end);

            // Copy parameters from BHoM object to Revit element
            revitPipe.CopyParameters(pipe, settings);

            SectionProfile sectionProfile = pipe.SectionProperty.SectionProfile;
            if (sectionProfile == null)
            {
                BH.Engine.Reflection.Compute.RecordError("Pipe creation requires a SectionProfile. \n No elements created.");
                return null;
            }

            PipeSectionProperty pipeSectionProperty = pipe.SectionProperty;

            // ToDo: Verify builtInParameter for correct value. This is not functioning as expected
            double flowRate = pipe.FlowRate;
            revitPipe.SetParameter(BuiltInParameter.RBS_DUCT_FLOW_PARAM, flowRate); //Not being set correctly.

            // Round Pipe 
            if (revitPipe.Shape() == ConnectorProfileType.Round)
            {
                TubeProfile elementProfile = sectionProfile.ElementProfile as TubeProfile;
                if (elementProfile == null)
                    return null;

                // Set Element Diameter
                double diameter = elementProfile.Diameter;
                revitPipe.SetParameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM, diameter);

                // Pipe Lining is not supported in RevitAPI.

                // Set InsulationProfile
                TubeProfile insulationProfile = sectionProfile.InsulationProfile as TubeProfile;
                if (insulationProfile != null)
                {
                    double insulationThickness = insulationProfile.Thickness;

                    // Get first available pipeLiningType from document
                    Autodesk.Revit.DB.Plumbing.PipeInsulationType pit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Plumbing.PipeInsulationType)).FirstOrDefault() as Autodesk.Revit.DB.Plumbing.PipeInsulationType;
                    if (pit == null)
                    {
                        BH.Engine.Reflection.Compute.RecordError("You must first create a pipe with insulation in your Revit model.");
                        return null;
                    }
                    // Create pipe Insulation
                    Autodesk.Revit.DB.Plumbing.PipeInsulation pIn = Autodesk.Revit.DB.Plumbing.PipeInsulation.Create(document, revitPipe.Id, pit.Id, insulationThickness);
                }        
                // Parameters not found in RevitAPI for Pipes, needs further investigation. Not critical for current workflows. 
                // double profileThickness = elementProfile.Thickness;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("No objects created. Only TubeProfiles are supported for Pipes.");
                return null;
            }

            refObjects.AddOrReplace(pipe, revitPipe);
            return revitPipe;
        }

        /***************************************************/
    }
}
