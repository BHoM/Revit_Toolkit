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
        public static Polyline Boundary(this PadFoundation element)
        {
            if (element.Location == null)
                return null;

            if (element.Location.InternalBoundaries.Count != 0)
            {
                BH.Engine.Base.Compute.RecordError($"PadFoundation with internal boundaries are currently not supported. BHoM_Guid: {element.BHoM_Guid}");
            }

            List<ICurve> segments = element.Location.ExternalBoundary.ISubParts().ToList();
            if (segments.Any(x => !(x is BH.oM.Geometry.Line)))
            {
                BH.Engine.Base.Compute.RecordError($"PadFoundation boundary contains non-linear curve segments. Only linear segments are currently supported. BHoM_Guid: {element.BHoM_Guid}");
            }

            return BH.Engine.Geometry.Create.Polyline(segments.Cast<BH.oM.Geometry.Line>().ToList());
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

        [Description("Finds the longest polyline edge in the XY plane (closed boundary), with deterministic tie-break by lowest vertex index.")]
        [Input("polyline", "Closed polyline boundary.")]
        [Output("success", "True if a non-degenerate longest edge was found.")]
        public static bool TryLongestEdgeInXY(this Polyline polyline, out Vector longestEdge)
        {

            longestEdge = null;

            var pts = polyline?.ControlPoints;
            if (pts == null || pts.Count < 2)
                return false;

            int n = pts.Count;
            if (n > 2 && (pts[n - 1] - pts[0]).Length() <= BH.oM.Geometry.Tolerance.Distance)
                n--;

            double maxLen = 0;

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;

                Vector e = pts[j] - pts[i];
                e.Z = 0;

                double len = e.Length();
                if (len <= BH.oM.Geometry.Tolerance.Distance)
                    continue;

                if (len > maxLen)
                {
                    maxLen = len;
                    longestEdge = e;
                }
            }

            return longestEdge != null;

        }

        /***************************************************/

        [Description("Placement data for a pad footprint: centroid, CCW-normalized rotation about +Z aligning the longest edge with global +X, and extents along family X/Y after that rotation (no scaling).")]
        [Input("outline", "Closed polyline in model XY.")]
        [Output("success", "False if outline is invalid or degenerate.")]
        public static bool TryPadOutlinePlacementInXY(this Polyline outline, out BH.oM.Geometry.Point centroid, out double rotationAboutZ, out double extentAlongFamilyX, out double extentAlongFamilyY)
        {
            centroid = null;
            rotationAboutZ = double.NaN;
            extentAlongFamilyX = double.NaN;
            extentAlongFamilyY = double.NaN;

            centroid = outline.Centroid();

            if (!outline.TryLongestEdgeInXY(out Vector longestEdge))
                return false;

            double theta = Math.Atan2(longestEdge.Y, longestEdge.X);

            List<BH.oM.Geometry.Point> pts = outline.ControlPoints;
            int n = pts.Count;
            if (n >= 3 && (pts[n - 1] - pts[0]).Length() <= BH.oM.Geometry.Tolerance.Distance)
                n--;

            if (n < 3)
                return false;

            double signedArea = FoundationSignedAreaXY(pts, n, centroid, theta);
            if (signedArea < 0)
                theta += Math.PI;

            rotationAboutZ = FoundationWrapAngle(theta);

            double cosT = Math.Cos(rotationAboutZ);
            double sinT = Math.Sin(rotationAboutZ);
            double minX = double.MaxValue, maxX = double.MinValue, minY = double.MaxValue, maxY = double.MinValue;

            for (int i = 0; i < n; i++)
            {
                double offsetXFromCentroid = pts[i].X - centroid.X;
                double offsetYFromCentroid = pts[i].Y - centroid.Y;
                double localXInRotatedFrame = offsetXFromCentroid * cosT + offsetYFromCentroid * sinT;
                double localYInRotatedFrame = -offsetXFromCentroid * sinT + offsetYFromCentroid * cosT;
                if (localXInRotatedFrame < minX) minX = localXInRotatedFrame;
                if (localXInRotatedFrame > maxX) maxX = localXInRotatedFrame;
                if (localYInRotatedFrame < minY) minY = localYInRotatedFrame;
                if (localYInRotatedFrame > maxY) maxY = localYInRotatedFrame;
            }

            extentAlongFamilyX = maxX - minX;
            extentAlongFamilyY = maxY - minY;

            return true;
        }

        /***************************************************/

        [Description("Signed shoelace area in XY after translating vertices to origin and rotating by angle about Z (foundation footprint winding).")]
        [Input("vertices", "Polygon vertices in order.")]
        [Input("vertexCount", "Number of vertices (closed ring without duplicate closing point).")]
        [Input("origin", "Translation subtracted from each vertex before rotation.")]
        [Input("angleRadiansZ", "Rotation about global Z in radians.")]
        [Output("signedArea", "Twice the usual shoelace sum / 2; sign indicates winding after transform.")]
        public static double FoundationSignedAreaXY(List<BH.oM.Geometry.Point> vertices, int vertexCount, BH.oM.Geometry.Point origin, double angleRadiansZ)
        {
            double cosT = Math.Cos(angleRadiansZ);
            double sinT = Math.Sin(angleRadiansZ);
            double sum = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                int j = (i + 1) % vertexCount;
                double dxI = vertices[i].X - origin.X;
                double dyI = vertices[i].Y - origin.Y;
                double dxJ = vertices[j].X - origin.X;
                double dyJ = vertices[j].Y - origin.Y;
                double qix = dxI * cosT + dyI * sinT;
                double qiy = -dxI * sinT + dyI * cosT;
                double qjx = dxJ * cosT + dyJ * sinT;
                double qjy = -dxJ * sinT + dyJ * cosT;
                sum += qix * qjy - qjx * qiy;
            }

            return 0.5 * sum;
        }

        /***************************************************/

        [Description("Wraps radians to (-π, π] for foundation placement angles.")]
        [Input("radians", "Angle in radians.")]
        [Output("wrapped", "Equivalent angle in (-π, π].")]
        public static double FoundationWrapAngle(double radians)
        {
            radians = radians % (2 * Math.PI);

            if (radians < -Math.PI)
                radians += Math.PI * 2;
            else if (radians > Math.PI)
                radians -= Math.PI * 2;

            return radians;
        }

        /***************************************************/

        [Description("Gets the origin point (centroid) of a PadFoundation.")]
        [Input("element", "PadFoundation element whose origin should be computed.")]
        [Output("origin", "Origin point (centroid) of the foundation external boundary, or null if invalid.")]
        public static BH.oM.Geometry.Point Origin(this PadFoundation element)
        {
            Polyline outline = Boundary(element);
            if (outline?.ControlPoints == null)
                return null;

            if (!outline.IsClosed(BH.oM.Geometry.Tolerance.Distance))
            {
                BH.Engine.Base.Compute.RecordError("Foundation outline is not properly closed.");
                return null;
            }

            var centerPoint = outline.Centroid();

            if (centerPoint == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not calculate centroid of foundation outline.");
                return null;
            }

            return centerPoint;
        }
    }
}


