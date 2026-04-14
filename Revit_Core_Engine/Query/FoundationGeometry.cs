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
using BH.oM.Geometry.CoordinateSystem;
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

        [Description("Checks whether a Polyline represents a rectangle.")]
        [Input("polyline", "Polyline to check.")]
        [Output("isRectangular", "True if the polyline represents a rectangle; otherwise false.")]
        public static bool IsRectangular(this Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null)
                return false;

            if (polyline.ControlPoints.Count != 5)
                return false;

            // Rectangular shape check
            double diagonal1 = (polyline.ControlPoints[2] - polyline.ControlPoints[0]).Length();
            double diagonal2 = (polyline.ControlPoints[3] - polyline.ControlPoints[1]).Length();

            return Math.Abs(diagonal1 - diagonal2) <= BH.oM.Geometry.Tolerance.Distance;
        }

        /***************************************************/

        [Description("Computes the rotation angle of a local coordinate system in the XY plane.")]
        [Input("localCS", "Local coordinate system to compute the rotation angle for.")]
        [Output("angle", "Rotation angle in radians measured counter-clockwise from the global X-axis.")]
        public static double RotationAngleInXY(this Cartesian localCS)
        {
            if (localCS == null)
                return 0.0;

            if (Math.Abs(Math.Abs(localCS.Z.DotProduct(BH.oM.Geometry.Vector.ZAxis)) - 1) > BH.oM.Geometry.Tolerance.Angle)
            {
                BH.Engine.Base.Compute.RecordWarning("Local Z-axis is not parallel to global Z-axis. Rotation angle may not be accurate.");
            }

            return Math.Atan2(localCS.X.Y, localCS.X.X);
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

        [Description("Gets width and length dimensions of a rectangular Polyline.")]
        [Input("polyline", "Polyline representing a rectangle.")]
        [Output("dimensions", "Tuple containing (width, length) in the same units as the input polyline.")]
        public static (double width, double length) RectangleDimensions(this Polyline polyline)
        {
            if (!IsRectangular(polyline))
            {
                BH.Engine.Base.Compute.RecordWarning("Cannot get rectangle dimensions: polyline does not represent a rectangle.");
                return (double.NaN, double.NaN);
            }

            double side1 = (polyline.ControlPoints[1] - polyline.ControlPoints[0]).Length();
            double side2 = (polyline.ControlPoints[2] - polyline.ControlPoints[1]).Length();

            double width = Math.Min(side1, side2);
            double length = Math.Max(side1, side2);

            return (width, length);
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

        /***************************************************/

        [Description("Gets oriented bounding box dimensions for any Polyline based on its first edge orientation.")]
        [Input("polyline", "Polyline to compute oriented bounding box dimensions for.")]
        [Output("dimensions", "Tuple containing (width, length) along the shape's local axes.")]
        public static (double width, double length) NonRectangleDimensions(this Polyline polyline)
        {
            if (polyline?.ControlPoints == null || polyline.ControlPoints.Count < 3)
                return (double.NaN, double.NaN);

            Vector edgeVector = polyline.ControlPoints[1] - polyline.ControlPoints[0];
            Vector xAxis = new Vector { X = edgeVector.X, Y = edgeVector.Y, Z = 0 }.Normalise();
            Vector yAxis = new Vector { X = -xAxis.Y, Y = xAxis.X, Z = 0 };

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (var point in polyline.ControlPoints)
            {
                double localX = point.X * xAxis.X + point.Y * xAxis.Y;
                double localY = point.X * yAxis.X + point.Y * yAxis.Y;

                if (localX < minX) minX = localX;
                if (localX > maxX) maxX = localX;
                if (localY < minY) minY = localY;
                if (localY > maxY) maxY = localY;
            }

            double width = Math.Min(maxX - minX, maxY - minY);
            double length = Math.Max(maxX - minX, maxY - minY);

            return (width, length);
        }
    }
}







