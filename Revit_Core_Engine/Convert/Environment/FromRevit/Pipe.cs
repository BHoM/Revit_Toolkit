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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using BH.Engine.Geometry;
using BH.oM.MEP.System;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit pipe into BHoM pipes.")]
        [Input("revitPipe", "Revit pipe to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("pipes", "List of BHoM MEP pipes converted from a Revit pipes.")]
        public static List<BH.oM.MEP.System.Pipe> PipeFromRevit(this Autodesk.Revit.DB.Plumbing.Pipe revitPipe, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            
            // Reuse a BHoM duct from refObjects if it has been converted before
            List<BH.oM.MEP.System.Pipe> bhomPipes = refObjects.GetValues<BH.oM.MEP.System.Pipe>(revitPipe.Id);
            if (bhomPipes != null)
            {
                return bhomPipes;
            }
            else
            {
                bhomPipes = new List<BH.oM.MEP.System.Pipe>();
            }

            bool isStartConnected = false;
            bool isEndConnected = false;
            List<BH.oM.Geometry.Line> queried = Query.LocationCurveMEP(revitPipe,out isStartConnected, out isEndConnected, settings);
            // Flow rate
            double flowRate = revitPipe.LookupParameterDouble(BuiltInParameter.RBS_PIPE_FLOW_PARAM); // Flow rate 
            // Pipe section property
            BH.oM.MEP.System.SectionProperties.PipeSectionProperty sectionProperty = revitPipe.PipeSectionProperty(settings);

            for (int i = 0; i < queried.Count; i++)
            {
                BH.oM.Geometry.Line segment = queried[i];
                BH.oM.MEP.System.Pipe thisSegment = new Pipe
                {
                    StartPoint = segment.StartPoint(),
                    EndPoint = segment.EndPoint(),
                    FlowRate = flowRate,
                    SectionProperty = sectionProperty
                };
                //Set identifiers, parameters & custom data
                thisSegment.SetIdentifiers(revitPipe);
                thisSegment.CopyParameters(revitPipe, settings.ParameterSettings);
                thisSegment.SetProperties(revitPipe, settings.ParameterSettings);
                bhomPipes.Add(thisSegment);
            }
            
            refObjects.AddOrReplace(revitPipe.Id, bhomPipes);
            return bhomPipes;
        }

        /***************************************************/
    }
}
