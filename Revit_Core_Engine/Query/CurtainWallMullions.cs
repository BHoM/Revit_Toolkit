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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Facade.Elements;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.Physical.Elements;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the mullions from a Revit curtain element and returns them in a form of BHoM FrameEdges.")]
        [Input("element", "Revit curtain element to extract the mullions from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("mullions", "Mullions extracted from the input Revit curtain element and converted to BHoM FrameEdges.")]
        public static List<FrameEdge> CurtainWallMullions(this HostObject element, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (element == null)
                return null;

            string refId = $"{element.Id}_Mullions";
            List<FrameEdge> edges = refObjects.GetValues<FrameEdge>(refId);
            if (edges != null)
                return edges;

            edges = new List<FrameEdge>();
            List<Element> mullions = element.ICurtainGrids().SelectMany(x => x.GetMullionIds()).Select(x => element.Document.GetElement(x)).ToList();
            foreach (Mullion mullion in mullions.Where(x => x.get_BoundingBox(null) != null))
            {
                edges.Add(mullion.FrameEdgeFromRevit(settings, refObjects));
            }

            refObjects.AddOrReplace(refId, edges);
            return edges;
        }

        /***************************************************/
    }
}




