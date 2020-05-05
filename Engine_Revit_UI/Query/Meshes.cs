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
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<oM.Geometry.IGeometry> Meshes(this GeometryElement geometryElement, Transform transform = null, Options options = null, RevitSettings settings = null)
        {
            if (geometryElement == null)
                return null;

            if (options == null)
                options = new Options();

            List<oM.Geometry.IGeometry> result = new List<oM.Geometry.IGeometry>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is GeometryInstance)
                {
                    GeometryInstance geometryInstance = (GeometryInstance)geometryObject;

                    Transform geometryTransform = geometryInstance.Transform;
                    if (transform != null)
                        geometryTransform = geometryTransform.Multiply(transform.Inverse);

                    List<oM.Geometry.IGeometry> mesh = null;
                    GeometryElement geomElement = null;

                    geomElement = geometryInstance.GetInstanceGeometry(geometryTransform);
                    if (geomElement == null)
                        continue;

                    mesh = geomElement.Meshes(null, options, settings);
                    if (mesh != null && mesh.Count != 0)
                        result.AddRange(mesh);
                }
                else if (geometryObject is Solid)
                {
                    Solid solid = (Solid)geometryObject;
                    FaceArray faces = solid.Faces;
                    if (faces == null)
                        continue;

                    foreach(Face face in faces)
                    {
                        result.Add(face.Triangulate(options.DetailLevel.FaceTriangulationFactor()).FromRevit());
                    }
                }
                else if (geometryObject is Face)
                {
                    Face face = (Face)geometryObject;
                    result.Add(face.Triangulate(options.DetailLevel.FaceTriangulationFactor()).FromRevit());
                }
                else if (geometryObject is Curve)
                {
                    Curve curve = (Curve)geometryObject;
                    if (curve.IsBound)
                    {
                        result.Add(new BH.oM.Geometry.Polyline { ControlPoints = curve.Tessellate().Select(x => x.PointFromRevit()).ToList() });
                    }
                }
            }

            return result;
        }

        /***************************************************/

        public static List<oM.Geometry.IGeometry> Meshes(this Element element, Options options, RevitSettings settings = null)
        {
            GeometryElement geometryElement = element.get_Geometry(options);

            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            return geometryElement.Meshes(transform, options, settings);
        }

        /***************************************************/
    }
}

