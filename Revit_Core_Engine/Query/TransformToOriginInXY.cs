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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
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

            translation = (centroid - new Point()).ProjectToXY();
            Vector longestEdge = outline.LongestEdgeDirection(BH.oM.Geometry.Tolerance.Distance);
            if (longestEdge != null)
                rotation = Vector.XAxis.SignedAngle(longestEdge, Vector.ZAxis);

            return (translation, rotation);
        }
    }
}
