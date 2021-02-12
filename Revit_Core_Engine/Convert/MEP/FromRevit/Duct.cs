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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using BH.Engine.Geometry;
using BH.oM.MEP.System;
using BH.oM.MEP.Fragments;
using BH.oM.MEP.System.ConnectionProperties;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit duct to BHoM ducts.")]
        [Input("revitDuct", "Revit duct to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("ducts", "List of BHoM duct objects converted from a Revit duct elements.")]
        public static List<BH.oM.MEP.System.Duct> DuctFromRevit(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM duct from refObjects if it has been converted before
            List<BH.oM.MEP.System.Duct> bhomDucts = refObjects.GetValues<BH.oM.MEP.System.Duct>(revitDuct.Id);
            if (bhomDucts != null)
            {
                return bhomDucts;
            }
            else
            {
                bhomDucts = new List<BH.oM.MEP.System.Duct>();
            }
            
            List<BH.oM.Geometry.Line> queried = Query.LocationCurveMEP(revitDuct, settings);

            // Element Sizing 
            // TODO DuctElementSize() help
            DimensionalFragment elementSize = Query.DuctElementSize(revitDuct, settings);

            // Section Profile
            List<BH.oM.MEP.System.SectionProperties.SectionProfile> sectionProfile = Query.DuctSectionProfile(revitDuct, settings);

            // Orientation angle
            double orientationAngle = revitDuct.OrientationAngle(settings); //ToDo - resolve in next issue, specific issue being raised

            // Coincident Elements 
            // TODO Coincident Elements - Fittings, Valves, Dampers
            List<ICoincident> coincidentElements = new List<ICoincident>();

            // Flow Properties 
            // TODO FlowFragment creation in method
            List<FlowFragment> flow = Query.DuctFlow(revitDuct, settings);

            // Connection properties
            // TODO figure out connections
            ConnectionProperty connectionProperty = Query.GetMepConnectors(revitDuct, settings);

            for (int i = 0; i < queried.Count; i++)
            {
                BH.oM.Geometry.Line segment = queried[i];
                BH.oM.MEP.System.Duct thisSegment = new Duct
                {
                    ElementSize = elementSize,
                    StartPoint = (Node)segment.StartPoint(),
                    EndPoint = (Node)segment.EndPoint(),                    
                    SectionProfile = sectionProfile,
                    OrientationAngle = orientationAngle,
                    CoincidentElements = coincidentElements,
                    Flow = flow,
                    ConnectionProperty = connectionProperty
                };
                //Set identifiers, parameters & custom data
                thisSegment.SetIdentifiers(revitDuct);
                thisSegment.CopyParameters(revitDuct, settings.ParameterSettings);
                thisSegment.SetProperties(revitDuct, settings.ParameterSettings);
                bhomDucts.Add(thisSegment);
            }

            refObjects.AddOrReplace(revitDuct.Id, bhomDucts);
            return bhomDucts;
        }

        /***************************************************/
    }
}

