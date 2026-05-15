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
using BH.Engine.Physical;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
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

        [Description("Extracts the outer rectangular boundary of a PadFoundation as a Polyline.")]
        [Input("element", "PadFoundation element whose boundary should be extracted.")]
        [Output("outline", "Polyline representing the PadFoundation external boundary.")]
        public static Polyline Outline(this PadFoundation element)
        {
            ICurve outline = element?.Location?.ExternalBoundary;
            if (outline == null)
                return null;

            if (element.Location.InternalBoundaries.Count != 0)
                BH.Engine.Base.Compute.RecordWarning($"PadFoundation with internal boundaries are currently not supported, holes were skipped. BHoM_Guid: {element.BHoM_Guid}");

            if (outline.ISubParts().Any(x => !(x is Line)))
            {
                BH.Engine.Base.Compute.RecordError($"PadFoundation boundary contains non-linear curve segments. Only linear segments are currently supported. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            return outline.IToPolyline();
        }

        /***************************************************/

        [Description("Computes the total thickness (sum of construction layers) of a PadFoundation.")]
        [Input("element", "PadFoundation element whose thickness should be computed.")]
        [Output("thickness", "Total thickness of all construction layers.")]
        public static double Thickness(this PadFoundation element)
        {
            oM.Physical.Constructions.Construction construction = element?.Construction as oM.Physical.Constructions.Construction;
            if (construction?.Layers == null)
                return double.NaN;

            return construction.Layers.Sum(layer => layer.Thickness);
        }

        /***************************************************/

        public static Polyline OrientToOrigin(this Polyline outline)
        {
            (Vector translation, double rotation) = outline.TransformToOriginInXY();
            if (translation == null || double.IsNaN(rotation))
                return null;

            return outline.Translate(translation).Rotate(new Point(), Vector.ZAxis, rotation);
        }

        /***************************************************/

        public static Vector LongestEdgeInXY(this Polyline polyline)
        {
            List<BH.oM.Geometry.Point> pts = polyline?.ControlPoints;
            if (pts == null || pts.Count < 2)
                return null;

            int n = pts.Count;
            if (n > 2 && (pts[n - 1] - pts[0]).Length() <= BH.oM.Geometry.Tolerance.Distance)
                n--;

            double tol = BH.oM.Geometry.Tolerance.Distance;

            bool Edge(int i, out Vector e, out double len)
            {
                e = pts[(i + 1) % n] - pts[i];
                e.Z = 0;
                len = e.Length();
                return len > tol;
            }

            double maxLen = 0;
            for (int i = 0; i < n; i++)
            {
                if (Edge(i, out _, out double len) && len > maxLen)
                    maxLen = len;
            }

            if (maxLen <= tol)
                return null;

            double tieTol = Math.Max(tol, 1e-9 * maxLen);
            double bestAz = double.MaxValue;

            Vector longestEdge = null;
            for (int i = 0; i < n; i++)
            {
                if (!Edge(i, out Vector e, out double len) || len < maxLen - tieTol)
                    continue;

                double az = Math.Atan2(e.Y, e.X);
                if (az < 0)
                    az += 2 * Math.PI;

                if (longestEdge == null || az < bestAz - 1e-12)
                {
                    bestAz = az;
                    longestEdge = e;
                }
            }

            return longestEdge;
        }

        /***************************************************/

        public static double RotationToGlobalX(Vector longestEdge)
        {
            double theta = Math.Atan2(longestEdge.Y, longestEdge.X).NormalizeAngleDomain();
            if (theta > Math.PI / 2)
                theta -= Math.PI;
            else if (theta < -Math.PI / 2)
                theta += Math.PI;

            return theta;
        }

        /***************************************************/

        public static (Vector, double) TransformToOriginInXY(this Polyline outline)
        {
            Vector translation = null;
            double rotation = double.NaN;

            List<Point> pts = outline?.ControlPoints;
            if (pts == null || pts.Count < 3)
                return (translation, rotation);

            int n = pts.Count;
            if (n >= 3 && (pts[n - 1] - pts[0]).Length() <= BH.oM.Geometry.Tolerance.Distance)
                n--;

            if (n < 3)
                return (translation, rotation);

            Point centroid = outline.Centroid();
            if (centroid == null)
                return (translation, rotation);

            translation = (centroid - new Point()).ProjectToXY();

            Vector longestEdge = outline.LongestEdgeInXY();
            if (longestEdge != null)
                rotation = RotationToGlobalX(longestEdge);

            return (translation, rotation);
        }

        /***************************************************/

        public static Point Centroid(this PadFoundation element)
        {
            return element?.Outline()?.Centroid();
        }

        /***************************************************/
    }
}


