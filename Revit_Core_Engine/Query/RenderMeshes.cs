/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.Engine.Graphics;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Graphics;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Attempts to retrieve Meshes corresponding to the input Revit Element. These meshes are returned as RenderMesh objects that can be used to visualise the input Revit Element.")]
        [Input("element", "Element to display through RenderMeshes.")]
        [Input("options", "Revit API options for the retrieval, including e.g. `IncludeNonVisibleObjects`.")]
        [Input("settings", "Revit Adapter settings to be applied in retrieving the meshes corresponding to the element.")]
        [Output("renderMeshes", "RenderMesh objects representing the input Revit Element.")]
        public static List<RenderMesh> RenderMeshes(this Element element, Options options, RevitSettings settings = null)
        {
            if (element == null)
                return null;

            List<RenderMesh> result = new List<RenderMesh>();
            foreach (Face face in element.Faces(options, settings))
            {
                RenderMesh renderMesh = face.Triangulate(options.DetailLevel.FaceTriangulationFactor()).MeshFromRevit().ToRenderMesh();
                System.Drawing.Color color = face.Color(element.Document);

                foreach (RenderPoint vertex in renderMesh.Vertices)
                {
                    vertex.Colour = color;
                }

                result.Add(renderMesh);
            }

            foreach (Curve curve in element.Curves(options, settings, false))
            {
                RenderMesh renderMesh = curve.MeshFromRevit().ToRenderMesh();
                System.Drawing.Color color = curve.Color(element.Document);
                foreach (RenderPoint vertex in renderMesh.Vertices)
                {
                    vertex.Colour = color;
                }

                result.Add(renderMesh);
            }

            result.AddRange(element.GeometryPrimitives(options, settings).Where(x => x is Mesh).Cast<Mesh>().Select(x => x.MeshFromRevit().ToRenderMesh()));
            return result;
        }

        /***************************************************/
    }
}




