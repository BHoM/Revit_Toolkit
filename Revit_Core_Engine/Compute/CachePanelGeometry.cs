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

        //[Description("Queries the WorksetId of the active workset in a given Revit document.")]
        //[Input("document", "Revit document to be queried for the active workset.")]
        //[Output("worksetId", "WorksetId of the active workset in the input Revit document.")]
        public static void CachePanelGeometry(this Document document, List<ElementId> elementIds, Discipline discipline, Dictionary<string, List<IBHoMObject>> refObjects, RevitSettings settings)
        {
            List<HostObject> hostObjectsToExtractSurfaces = new List<HostObject>();
            foreach (ElementId id in elementIds)
            {
                HostObject hostObject = document.GetElement(id) as HostObject;
                if (hostObject == null)
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
