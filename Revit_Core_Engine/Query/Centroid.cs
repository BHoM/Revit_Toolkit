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

        [Description("Finds geometrical centroid of an element.")]
        [Input("element", "Element to query for centroid.")]
        [Input("options", "Options to use when extracting element solids.")]
        [Output("centroid", "Geometrical centroid of the input element.")]
        public static XYZ Centroid(this Element element, Options options)
        {
            var solids = element.Solids(options).Where(x => x.Volume > 1e-3).ToList();
            return solids.Select(x => x.ComputeCentroid() * x.Volume).Aggregate((x, y) => x + y) / solids.Sum(x => x.Volume);
        }

        /***************************************************/

        [Description("Finds geometrical centroid of a collection of solids.")]
        [Input("solids", "Collection of solids to query for centroid.")]
        [Output("centroid", "Geometrical centroid of the input collection of solids.")]
        public static XYZ Centroid(this IEnumerable<Solid> solids)
        {
            solids = solids.Where(x => x.Volume > 1e-3).ToList();
            return solids.Select(x => x.ComputeCentroid() * x.Volume).Aggregate((x, y) => x + y) / solids.Sum(x => x.Volume);
        }

        /***************************************************/

        [Description("Finds geometrical centroid of a planar face.")]
        [Input("face", "Planar face to query for centroid.")]
        [Output("centroid", "Geometrical centroid of the input planar face.")]
        public static XYZ Centroid(this PlanarFace face)
        {
            return face.Triangulate().PlanarMeshCentroid();
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static XYZ PlanarMeshCentroid(this Mesh mesh)
        {
            double totalArea = 0;
            double x = 0, y = 0, z = 0;

            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                MeshTriangle triangle = mesh.get_Triangle(i);
                XYZ p1 = triangle.get_Vertex(0);
                XYZ p2 = triangle.get_Vertex(1);
                XYZ p3 = triangle.get_Vertex(2);

                double area = 0.5 * ((p2 - p1).CrossProduct(p3 - p1)).GetLength();
                XYZ centroid = (p1 + p2 + p3) / 3;

                x += centroid.X * area;
                y += centroid.Y * area;
                z += centroid.Z * area;
                totalArea += area;
            }

            return new XYZ(x / totalArea, y / totalArea, z / totalArea);
        }

        /***************************************************/
    }
}
