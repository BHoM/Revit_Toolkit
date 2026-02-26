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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Geometry.CoordinateSystem;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Polyline ExtractBoundary(this PadFoundation element)
        {
            if (element.Location == null)
                return null;

            if (element.Location.InternalBoundaries.Count != 0)
            {
                BH.Engine.Base.Compute.RecordError("InternalBoundaries error");
            }

            List<ICurve> segments = element.Location.ExternalBoundary.ISubParts().ToList();
            if (segments.Any(x => !(x is BH.oM.Geometry.Line)))
            {
                BH.Engine.Base.Compute.RecordError("");
            }

            return BH.Engine.Geometry.Create.Polyline(segments.Cast<BH.oM.Geometry.Line>().ToList());
        }

        /***************************************************/

        public static bool IsRectangular(Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null)
                return false;

            var points = polyline.ControlPoints;
            int pointCount = points.Count;

            //Reecrtangle check  closing point
            if (pointCount != 5)
                return false;

            List<BH.oM.Geometry.Line> edges = new List<BH.oM.Geometry.Line>();
            for (int i = 0; i < 4; i++)
            {
                edges.Add(new BH.oM.Geometry.Line { Start = points[i], End = points[(i + 1) % pointCount] });
            }

            double[] lengths = new double[4];
            for (int i = 0; i < 4; i++)
                lengths[i] = edges[i].Length();

            double tolerance = 0.001;
            if (Math.Abs(lengths[0] - lengths[2]) > tolerance || Math.Abs(lengths[1] - lengths[3]) > tolerance)
                return false;

            BH.oM.Geometry.Point p1 = edges[0].Start;
            BH.oM.Geometry.Point p2 = edges[0].End;
            BH.oM.Geometry.Point p3 = edges[1].End;
            BH.oM.Geometry.Point p4 = edges[2].End;

            var d1 = (p1 - p3).Length();
            var d2 = (p2 - p4).Length();

            return Math.Abs(d1 - d2) <= tolerance;
        }

        /***************************************************/

        public static double ComputeRotationAngle(this Cartesian localCS)
        {
            if (localCS == null)
                return 0.0;

            Basis basis = (Basis)localCS;

            var gx = BH.oM.Geometry.Vector.XAxis; // Global X Axis
            var lx = basis.X; // Local X Axis
            //if (1 - Math.Abs(gx.DotProduct(lx)) <= Tolerance.Angle)
            //    gx = Vector.YAxis;

            var global = new BH.oM.Geometry.Vector { X = gx.X, Y = gx.Y, Z = 0 }.Normalise();
            var local = new BH.oM.Geometry.Vector { X = lx.X, Y = lx.Y, Z = 0 }.Normalise();

            //return Math.Atan2(Vector.ZAxis, global.DotProduct(local));
            return Math.Atan2(global.CrossProduct(local).Z, global.DotProduct(local));
        }

        /***************************************************/

        public static double GetThicknessFromConstr(this PadFoundation element)
        {
            BH.oM.Physical.Constructions.Construction constr = element.Construction as oM.Physical.Constructions.Construction;

            if (constr == null || constr.Layers == null || constr.Layers.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("Invalid input");
                return double.NaN;
            }

            double totalDepth = 0.0;
            foreach (var layer in constr.Layers)
            {
                totalDepth += layer.Thickness;
            }
            return totalDepth;
        }

        /***************************************************/

        public static (double width, double length) GetRectangleDimensions(this Polyline polyline)
        {
            if (polyline == null || polyline.ControlPoints == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Cannot get rectangle dimensions: polyline or control points are null.");
            }

            var points = polyline.ControlPoints;
            int pointCount = points.Count;

            if (pointCount != 5)
            {
                BH.Engine.Base.Compute.RecordWarning("Cannot get rectangle dimensions: polyline or control points are null.");
            }

            double[] lengths = new double[4];
            for (int i = 0; i < 4; i++)
            {
                var line = new BH.oM.Geometry.Line { Start = points[i], End = points[(i + 1) % pointCount] };
                lengths[i] = line.Length();
            }

            double side1 = lengths[0];
            double side2 = lengths[1];

            double width = Math.Min(side1, side2);
            double length = Math.Max(side1, side2);

            return (width, length);
        }

        /***************************************************/

        public static FamilySymbol LoadPadRectangleTemplate(Document document, RevitSettings settings)
        {
            string familyName = "BHE_StructuralFoundations_Pad-Rectangular";
            string typeName = "1000x1000x500 DP";

            return settings.FamilyLoadSettings.LoadFamilySymbol(document, "Structural Foundations", familyName, typeName);
        }

        /***************************************************/

        public static FamilySymbol GenerateFoundationType(this PadFoundation element, Document document, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            FamilySymbol baseSymbol = LoadPadRectangleTemplate(document, settings);
            if (baseSymbol == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not load rectangular foundation template.");
                return null;
            }

            string newTypeName = $"PadFoundation_{Guid.NewGuid().ToString().Substring(0, 8)}";
            FamilySymbol foundationSymbol = baseSymbol.Duplicate(newTypeName) as FamilySymbol;
            if (foundationSymbol == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not duplicate foundation family symbol.");
                return null;
            }

            foundationSymbol.Activate();

            Polyline outline = ExtractBoundary(element);
            if (outline == null || !IsRectangular(outline))
            {
                BH.Engine.Base.Compute.RecordError("Foundation outline must be rectangular.");
                return null;
            }

            var (width, length) = GetRectangleDimensions(outline);
            double depth = GetThicknessFromConstr(element);

            Parameter widthParam = foundationSymbol.LookupParameter("BHE_Width");
            widthParam?.Set(width);

            Parameter lengthParam = foundationSymbol.LookupParameter("BHE_Length");
            lengthParam?.Set(length);

            Parameter depthParam = foundationSymbol.LookupParameter("BHE_Depth");
            depthParam?.Set(depth);

            return foundationSymbol;
        }

        /***************************************************/

        public static XYZ GetFoundationOrigin(this PadFoundation element)
        {
            Polyline outline = ExtractBoundary(element);
            if (outline == null)
                return null;

            // Validate the outline shape
            if (outline.ControlPoints == null)
                return null;

            var points = outline.ControlPoints;
            int pointCount = points.Count;

            if (pointCount != 5)
            {
                BH.Engine.Base.Compute.RecordError($"Foundation outline should have exactly 5 points for a rectangle, but has {pointCount}.");
                return null;
            }

            // Check if the shape is closed 
            if (points[0].Distance(points[4]) > BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Base.Compute.RecordError("Foundation outline is not properly closed.");
                return null;
            }

            List<oM.Geometry.Line> outlineLines = new List<oM.Geometry.Line>();
            for (int i = 0; i < 4; i++) 
            {
                outlineLines.Add(new oM.Geometry.Line { Start = points[i], End = points[i + 1] });
            }

            //;check
            oM.Geometry.Point centerPoint = new oM.Geometry.Point();

            try
            {
                centerPoint = outlineLines.Centroid();
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError("Error msg:" + ex.Message);
            }
            //oM.Geometry.Point centerPoint = outlineLines.Centroid();
            if (centerPoint == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not calculate centroid of foundation outline.");
                return null;
            }

            return centerPoint.ToRevit();
        }
    }
}







