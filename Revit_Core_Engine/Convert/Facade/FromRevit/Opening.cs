/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Facade.Elements;
using BH.oM.Geometry;
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

        [Description("Converts a Revit FamilyInstance to BH.oM.Facade.Elements.Opening.")]
        [Input("familyInstance", "Revit FamilyInstance to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("opening", "BH.oM.Facade.Elements.Opening resulting from converting the input Revit FamilyInstance.")]
        public static oM.Facade.Elements.Opening FacadeOpeningFromRevit(this FamilyInstance familyInstance, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null)
                return null;

            settings = settings.DefaultIfNull();

            string refId = familyInstance.Id.ReferenceIdentifier(familyInstance.Host);
            oM.Facade.Elements.Opening opening = refObjects.GetValue<oM.Facade.Elements.Opening>(refId);
            if (opening != null)
                return opening;

            List<FrameEdge> edges = new List<FrameEdge>();
            HostObject host = familyInstance.Host as HostObject;
            if (host?.ICurtainGrids()?.Count > 0)
            {
                List<PolyCurve> edgeLoops = familyInstance.EdgeLoops();

                if (edgeLoops != null && edgeLoops.Count != 0)
                {
                    if (edgeLoops.Count != 1)
                        BH.Engine.Base.Compute.RecordWarning($"Opening has more than one closed outline. Revit ElementId: {familyInstance.Id.IntegerValue}");

                    List<FrameEdge> mullions = host.CurtainWallMullions(settings, refObjects);
                    foreach (var curve in edgeLoops.SelectMany(x => x.SubParts()))
                    {
                        BH.oM.Geometry.Point mid = curve.IPointAtParameter(0.5);
                        FrameEdge mullion = mullions.FirstOrDefault(x => x.Curve != null && mid.IDistance(x.Curve) <= settings.DistanceTolerance);
                        
                        if (mullion == null)
                            BH.Engine.Base.Compute.RecordWarning($"Mullion information is missing for some edges of a curtain Opening. Revit ElementId: {{familyInstance.Id.IntegerValue}}\"");
                     
                        edges.Add(new FrameEdge { Curve = curve, FrameEdgeProperty = mullion?.FrameEdgeProperty });
                    }
                }
                else
                    BH.Engine.Base.Compute.RecordWarning($"Edge curves of the opening could not be retrieved from the model (possibly it has zero area). An opening object without frame edges has been returned. Revit ElementId: {familyInstance.Id.IntegerValue}");
            }
            else
            {
                ISurface openingSurface = familyInstance.OpeningSurface(host, settings);
                if (openingSurface != null)
                {
                    List<ICurve> extCrvs = openingSurface.IExternalEdges().SelectMany(x => x.ISubParts()).ToList();
                    edges.AddRange(extCrvs.Select(x => new FrameEdge { Curve = x, FrameEdgeProperty = null }));
                    BH.Engine.Base.Compute.RecordWarning($"Frame edge properties of the opening could not be retrieved from the model. Revit ElementId: {familyInstance.Id.IntegerValue}");
                }
                else
                    BH.Engine.Base.Compute.RecordWarning($"Edge curves of the opening could not be retrieved from the model (possibly it has zero area or complex geometry). An opening object without frame edges has been returned. Revit ElementId: {familyInstance.Id.IntegerValue}");
            }

            oM.Physical.Constructions.Construction construction = familyInstance.FacadePanelConstruction(settings, refObjects);
            OpeningType openingType = familyInstance.FacadeOpeningType();
            opening = new BH.oM.Facade.Elements.Opening { Name = familyInstance.Name, Edges = edges, OpeningConstruction = construction, Type = openingType };

            //Set identifiers, parameters & custom data
            opening.SetIdentifiers(familyInstance);
            opening.CopyParameters(familyInstance, settings.MappingSettings);
            opening.SetProperties(familyInstance, settings.MappingSettings);

            refObjects.AddOrReplace(refId, opening);
            return opening;
        }

        /***************************************************/
    }
}



