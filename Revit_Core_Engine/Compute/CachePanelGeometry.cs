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
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.Revit.Engine.Core.Objects;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the panel geometry from Revit walls, floors, roofs etc. and caches it in refObjects in order to use it in panel converts downstream." +
                     "\nIt is an optimisation meant to minimise the number of document regenerations on panel converts, which often become the major time consumer.")]
        [Input("document", "Revit document hosting the panel elements with locations to be cached.")]
        [Input("elementIds", "Ids of the panel elements with locations to be cached.")]
        [Input("discipline", "Engineering discipline based on the BHoM discipline classification.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once." +
               "\nExtracted location surfaces are stored here.")]
        public static void CachePanelGeometry(this Document document, List<ElementId> elementIds, Discipline discipline, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects)
        {
            List<HostObject> hostObjectsToExtractSurfaces = new List<HostObject>();
            foreach (ElementId id in elementIds)
            {
                HostObject hostObject = document.GetElement(id) as HostObject;
                if (hostObject == null || hostObject is MEPCurve)
                    continue;

                bool outlinesFound = false;
                if (discipline == Discipline.Structural)
                {
                    List<ICurve> outlines = hostObject.AnalyticalOutlines(settings);
                    if (outlines != null && outlines.Count != 0)
                    {
                        outlinesFound = true;
                        OutlineCache cache = new OutlineCache { Outlines = outlines };
                        refObjects.Add(id.OutlineCacheKey(), new List<IBHoMObject> { cache });
                    }
                }

                if (!outlinesFound)
                    hostObjectsToExtractSurfaces.Add(hostObject);
            }

            var surfacesToCache = hostObjectsToExtractSurfaces.PanelSurfaces(discipline, settings);
            foreach (HostObject hostObject in hostObjectsToExtractSurfaces)
            {
                SurfaceCache cache = new SurfaceCache { Surfaces = surfacesToCache[hostObject.Id] };
                refObjects.Add(hostObject.Id.SurfaceCacheKey(), new List<IBHoMObject> { cache });
            }
        }

        /***************************************************/
    }
}
