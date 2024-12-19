/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Facade;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Dimensional;
using BH.oM.Physical.Constructions;
using BH.oM.Facade.Elements;
using BH.oM.Facade.SectionProperties;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Facade.Elements.CurtainWall.")]
        [Input("wall", "Revit Wall to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("curtainWall", "BH.oM.Facade.Elements.CurtainWall resulting from converting the input Revit Wall.")]
        public static CurtainWall CurtainWallFromRevit(this Wall wall, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return ((HostObject)wall).CurtainWallFromRevit(settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit Wall to BH.oM.Facade.Elements.CurtainSystem.")]
        [Input("system", "Revit CurtainSystem to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("curtainWall", "BH.oM.Facade.Elements.CurtainWall resulting from converting the input Revit CurtainSystem.")]
        public static CurtainWall CurtainWallFromRevit(this CurtainSystem system, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return ((HostObject)system).CurtainWallFromRevit(settings, refObjects);
        }


        /***************************************************/
        /****               Private Methods             ****/
        /***************************************************/

        private static CurtainWall CurtainWallFromRevit(this HostObject element, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null)
                return null;

            settings = settings.DefaultIfNull();

            CurtainWall bHoMCurtainWall = refObjects.GetValue<CurtainWall>(element.Id);
            if (bHoMCurtainWall != null)
                return bHoMCurtainWall;

            IEnumerable<oM.Facade.Elements.Opening> curtainPanels = element.FacadeCurtainPanels(settings, refObjects);

            if (curtainPanels == null || !curtainPanels.Any())
                BH.Engine.Base.Compute.RecordError($"Processing of panels of a Revit curtain wall failed. BHoM curtain wall without location has been returned. Revit ElementId: {element.Id.IntegerValue}");

            // Unify edges
            double sqTol = settings.DistanceTolerance * settings.DistanceTolerance;
            List<(FrameEdge, BH.oM.Geometry.Point)> edges = new List<(FrameEdge, BH.oM.Geometry.Point)>();
            foreach (oM.Facade.Elements.Opening panel in curtainPanels)
            {
                for (int i = 0; i < panel.Edges.Count; i++)
                {
                    FrameEdge edge = panel.Edges[i];
                    BH.oM.Geometry.Point midPoint = edge.Curve.IPointAtParameter(0.5);
                    
                    FrameEdge existing = edges.FirstOrDefault(x => x.Item2.SquareDistance(midPoint) <= sqTol).Item1;
                    if (existing != null)
                        panel.Edges[i] = existing;
                    else
                        edges.Add((edge, midPoint));
                }
            }

            // Get external edges of whole curtain wall
            List<FrameEdge> allEdges = curtainPanels.SelectMany(x => x.Edges).ToList();
            List<FrameEdge> extEdges = allEdges.Distinct().Where(x => allEdges.Count(y => x == y) == 1).ToList();

            bHoMCurtainWall = new CurtainWall { ExternalEdges = extEdges, Openings = curtainPanels.ToList(), Name = element.FamilyTypeFullName() };

            //Set identifiers, parameters & custom data
            bHoMCurtainWall.SetIdentifiers(element);
            bHoMCurtainWall.CopyParameters(element, settings.MappingSettings);
            bHoMCurtainWall.SetProperties(element, settings.MappingSettings);

            refObjects.AddOrReplace(element.Id, bHoMCurtainWall);

            return bHoMCurtainWall;
        }

        /***************************************************/
    }
}

