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
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Geometry.Mesh MeshFromRevit(this Mesh mesh)
        {
            if (mesh == null)
                return null;

            oM.Geometry.Mesh bHoMMesh = new oM.Geometry.Mesh { Vertices = mesh.Vertices.Select(x => x.PointFromRevit()).ToList() };
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                bHoMMesh.Faces.Add(mesh.get_Triangle(i).FromRevit());
            }

            return bHoMMesh;
        }

        /***************************************************/

        public static oM.Geometry.Mesh MeshFromRevit(this Curve curve)
        {
            if (curve == null)
                return null;

            List<XYZ> vertices = curve.Tessellate().ToList();
            oM.Geometry.Mesh mesh = new oM.Geometry.Mesh { Vertices = vertices.Select(x => x.PointFromRevit()).ToList() };
            for (int i = 0; i < vertices.Count - 1; i++)
            {
                mesh.Faces.Add(new oM.Geometry.Face { A = i, B = i + 1, C = i + 1 });
            }

            return mesh;
        }

        /***************************************************/
    }
}

