/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System;
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

        [Description("Computes translation and rotation to orient a Polyline to the origin in XY.")]
        [Input("outline", "Polyline to transform.")]
        [Output("result", "Centroid projected to XY and rotation angle in radians around Z.")]
        public static (Vector, double) TransformToOriginInXY(this Polyline outline)
        {
            Vector translation = null;
            double rotation = double.NaN;

            List<Point> pts = outline?.ControlPoints;
            if (pts == null || pts.Count < 3)
                return (translation, rotation);

            Point centroid = outline.Centroid();
            if (centroid == null)
                return (translation, rotation);

            translation = (new Point() - centroid);

            // 1. Find dominant edge direction
            // 2. Find longest edge parallel to the dominant direction
            // 3. Rotate the longest edge to X axis and check if the start point is above or below the centroid to determine the rotation direction 
            Vector dominantEdge = outline.DominantEdgeDirection(Tolerance.Distance);
            if (dominantEdge != null)
            {
                Line longestEdge = outline.SubParts().Where(x => x.Direction().IsParallel(dominantEdge, Tolerance.Angle) != 0).OrderByDescending(x => x.Length()).First();
                rotation = dominantEdge.SignedAngle(Vector.XAxis, Vector.ZAxis);
                Point orientedStart = longestEdge.Start.Rotate(centroid, Vector.ZAxis, rotation);
                if (orientedStart.Y > centroid.Y)
                    rotation -= Math.PI;
            }

            return (translation, rotation);
        }
    }
}
