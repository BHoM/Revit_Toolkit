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
            if (!outline.TryPadOutlinePlacementInXY(out BH.oM.Geometry.Point centroid, out double rotation, out double length, out double width))
                return null;

            Point origin = new Point();
            return outline.Translate(origin - centroid).Rotate(origin, Vector.ZAxis, rotation);
        }

        /***************************************************/

        [Description("Finds the longest polyline edge in the XY plane (closed boundary). When several edges tie for length (e.g. squares), picks the edge whose direction has the smallest azimuth in [0, 2π) so pad rotation is stable.")]
        [Input("polyline", "Closed polyline boundary.")]
        [Output("success", "True if a non-degenerate longest edge was found.")]
        public static bool TryLongestEdgeInXY(this Polyline polyline, out Vector longestEdge)
        {
            longestEdge = null;
            List<BH.oM.Geometry.Point> pts = polyline?.ControlPoints;
            if (pts == null || pts.Count < 2)
                return false;

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
                return false;

            double tieTol = Math.Max(tol, 1e-9 * maxLen);
            double bestAz = double.MaxValue;

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

            return longestEdge != null;
        }

        /***************************************************/

        [Description("Plan rotation (radians, same convention as Revit plan rotation) from the longest boundary edge direction in XY: aligns that edge with family +X after folding to (-π/2, π/2].")]
        [Input("longestEdge", "Longest edge direction in the XY plane (non-zero).")]
        [Output("rotationAboutZ", "Rotation about world Z consistent with TryPadOutlinePlacementInXY.")]
        public static double PlanRotationAboutZFromLongestEdgeXY(Vector longestEdge)
        {
            double theta = Math.Atan2(longestEdge.Y, longestEdge.X).NormalizeAngleDomain();
            if (theta > Math.PI / 2)
                theta -= Math.PI;
            else if (theta < -Math.PI / 2)
                theta += Math.PI;

            return theta;
        }

        /***************************************************/

        [Description("Single source of truth for pad plan placement: centroid of the BHoM outline (top face boundary), rotationAboutZ (radians) from the longest boundary edge in XY (same rule as Revit top-face longest edge in SetLocation(PadFoundation)), and extents in that frame for BHE_Length/BHE_Width.")]
        [Input("outline", "Closed polyline in model XY.")]
        [Output("success", "False if outline is invalid or degenerate.")]
        public static bool TryPadOutlinePlacementInXY(this Polyline outline, out BH.oM.Geometry.Point centroid, out double rotationAboutZ, out double extentAlongFamilyX, out double extentAlongFamilyY)
        {
            rotationAboutZ = double.NaN;
            extentAlongFamilyX = double.NaN;
            extentAlongFamilyY = double.NaN;
            centroid = null;

            List<BH.oM.Geometry.Point> pts = outline?.ControlPoints;
            if (pts == null || pts.Count < 3)
                return false;

            int n = pts.Count;
            if (n >= 3 && (pts[n - 1] - pts[0]).Length() <= BH.oM.Geometry.Tolerance.Distance)
                n--;

            if (n < 3)
                return false;

            centroid = outline.Centroid();
            if (centroid == null)
                return false;

            if (!outline.TryLongestEdgeInXY(out Vector longestEdge))
                return false;

            rotationAboutZ = PlanRotationAboutZFromLongestEdgeXY(longestEdge);

            double cosT = Math.Cos(rotationAboutZ);
            double sinT = Math.Sin(rotationAboutZ);
            double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;

            for (int i = 0; i < n; i++)
            {
                double dx = pts[i].X - centroid.X;
                double dy = pts[i].Y - centroid.Y;
                double lx = dx * cosT + dy * sinT;
                double ly = -dx * sinT + dy * cosT;
                if (lx < minX) minX = lx;
                if (lx > maxX) maxX = lx;
                if (ly < minY) minY = ly;
                if (ly > maxY) maxY = ly;
            }

            extentAlongFamilyX = maxX - minX;
            extentAlongFamilyY = maxY - minY;

            return true;
        }

        /***************************************************/

        [Description("Gets the origin point (centroid) of a PadFoundation.")]
        [Input("element", "PadFoundation element whose origin should be computed.")]
        [Output("origin", "Origin point (centroid) of the foundation external boundary, or null if invalid.")]
        public static BH.oM.Geometry.Point Origin(this PadFoundation element)
        {
            return element?.Outline()?.Centroid();
        }

        /***************************************************/
    }
}


