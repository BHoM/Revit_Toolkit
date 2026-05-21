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
using BH.oM.Adapters.Revit.Settings;
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
        public static Polyline FoundationGeometryOutline(this PadFoundation element)
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
        public static double FoundationGeometryThickness(this PadFoundation element)
        {
            oM.Physical.Constructions.Construction construction = element?.Construction as oM.Physical.Constructions.Construction;
            if (construction?.Layers == null)
                return double.NaN;

            return construction.Layers.Sum(layer => layer.Thickness);
        }

        /***************************************************/

        [Description("Returns the centroid of the PadFoundation outer boundary as a Point.")]
        [Input("element", "PadFoundation element to compute the centroid for.")]
        [Output("centroid", "Centroid of the PadFoundation outer boundary point.")]
        public static Point FoundationGeometryCentroid(this PadFoundation element)
        {
            return element?.FoundationGeometryOutline()?.Centroid();
        }

        /***************************************************/

        [Description("Checks whether a Polyline represents a rectangle.")]
        [Input("polyline", "Polyline to check.")]
        [Input("settings", "Revit adapter settings containing tolerance values. If null, default tolerance will be used.")]
        [Output("isRectangular", "True if the polyline represents a rectangle; otherwise false.")]
        public static bool IsRectangle(this Polyline polyline, RevitSettings settings = null)
        {
            List<Point> pts = polyline?.ControlPoints;
            if (pts == null || pts.Count != 5)
                return false;

            double tol = settings?.DistanceTolerance ?? BH.oM.Geometry.Tolerance.Distance;

            if (polyline.IIsClosed(tol) != true)
                return false;

            double diagonal1 = pts[2].Distance(pts[0]);
            double diagonal2 = pts[3].Distance(pts[1]);

            return Math.Abs(diagonal1 - diagonal2) <= tol;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Vector LongestEdgeDirection(this Polyline polyline, double tolerance, double angleTolerance = Tolerance.Angle)
        {
            List<Line> edges = polyline.SubParts().Where(x => x != null && x.Length() > tolerance).ToList();
            Line longest = edges.OrderByDescending(x => x.Length()).First();
            Vector longestDir = longest.Direction();
            longestDir.Z = 0;

            Dictionary<Vector, double> dirLen = new Dictionary<Vector, double>();
            foreach (Line edge in edges)
            {
                Vector direction = edge.Direction();
                direction.Z = 0;

                Vector dir = dirLen.Keys.FirstOrDefault(x => x.IsParallel(direction, angleTolerance) != 0);
                if (dir != null)
                    dirLen[dir] += edge.Length();
                else
                    dirLen[direction] = edge.Length();
            }

            return longestDir;
        }

        /***************************************************/
    }
}


