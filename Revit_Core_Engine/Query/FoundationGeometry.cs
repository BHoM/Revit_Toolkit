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
           
            var global = new BH.oM.Geometry.Vector { X = gx.X, Y = gx.Y, Z = 0 }.Normalise();
            var local = new BH.oM.Geometry.Vector { X = lx.X, Y = lx.Y, Z = 0 }.Normalise();

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

            //check if family is already loaded 
            Family existingFamily = new FilteredElementCollector(document)
                .OfClass(typeof(Family))
                .FirstOrDefault(x => x.Name == familyName) as Family;

            if (existingFamily != null)
            {
                FamilySymbol symbol = existingFamily.GetFamilySymbolIds()
                    .Select(id => document.GetElement(id) as FamilySymbol)
                    .FirstOrDefault(x => x != null && x.Name == typeName);

                if (symbol != null)
                {
                    if (!symbol.IsActive)
                        symbol.Activate();

                    return symbol;
                }
            }

            //try loading from  library 
            FamilySymbol loadedFamily = settings.FamilyLoadSettings?.LoadFamilySymbol(document, "Structural Foundations", familyName, typeName);
            if (loadedFamily != null)
                return loadedFamily;

            // load from the default resource path
            string path = System.IO.Path.Combine(@"C:\ProgramData\BHoM\Resources\Revit\Families", $"{familyName}.rfa");
            if (System.IO.File.Exists(path))
            {
                Family family;
                if (document.LoadFamily(path, out family) && family != null)
                {
                    FamilySymbol symbol = family.GetFamilySymbolIds()
                        .Select(id => document.GetElement(id) as FamilySymbol)
                        .FirstOrDefault(x => x != null && x.Name == typeName);

                    if (symbol != null)
                    {
                        if (!symbol.IsActive)
                            symbol.Activate();

                        return symbol;
                    }
                }
            }

            return null;
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
            widthParam?.Set(width.FromSI(SpecTypeId.Length));

            Parameter lengthParam = foundationSymbol.LookupParameter("BHE_Length");
            lengthParam?.Set(length.FromSI(SpecTypeId.Length));

            Parameter depthParam = foundationSymbol.LookupParameter("BHE_Depth");
            depthParam?.Set(depth.FromSI(SpecTypeId.Length));

            return foundationSymbol;
        }

        /***************************************************/

        public static BH.oM.Geometry.Point GetFoundationOrigin(this PadFoundation element)
        {
            Polyline outline = ExtractBoundary(element);
            // Validate the outline shape
            if (outline?.ControlPoints == null)
                return null;

            var points = outline.ControlPoints;
            int pointCount = points.Count;

            if (pointCount != 5)
            {
                BH.Engine.Base.Compute.RecordError($"Foundation outline should have exactly 5 points for a rectangle, but has {pointCount}.");
                return null;
            }

            // Check if the shape is closed 
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







