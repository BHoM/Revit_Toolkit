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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<oM.Geometry.Line> ExtractBoundary(PadFoundation element)
        {
            if (element.Location == null)
                return null;

            List<BH.oM.Geometry.Line> boundary = new List<BH.oM.Geometry.Line>();

            if (element.Location is BH.oM.Geometry.PlanarSurface surface && surface.ExternalBoundary != null)
            {
                var externalCurve = surface.ExternalBoundary;

                if (externalCurve is BH.oM.Geometry.PolyCurve polyCurve)
                {
                    foreach (var curve in polyCurve.Curves)
                    {
                        if (curve is BH.oM.Geometry.Line line)
                        {
                            boundary.Add(line);
                        }
                    }
                }
                else if (externalCurve is BH.oM.Geometry.Line line)
                {
                    boundary.Add(line);
                }
            }

            if (boundary.Count == 0)
                return null;

            if (!IsRectangular(boundary))
                return null;

            return boundary;
        }

        /***************************************************/

        public static bool IsRectangular(List<oM.Geometry.Line> edges)
        {
            if (edges == null || edges.Count != 4)
                return false;

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

        public static double GetThicknessFromConstr(PadFoundation element)
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

        public static (double width, double length) GetRectangleDimensions(List<oM.Geometry.Line> boundary)
        {
            if (boundary == null || boundary.Count != 4)
                return (0, 0);

            double[] lengths = new double[4];
            for (int i = 0; i < 4; i++)
                lengths[i] = boundary[i].Length();

            double side1 = lengths[0];
            double side2 = lengths[1];

            double width = Math.Min(side1, side2);
            double length = Math.Max(side1, side2);

            return (width, length);
        }

        /***************************************************/

        public static FamilySymbol LoadPadRectangleTemplate(Document document, RevitSettings settings)
        {
            string familyName = "StructuralFoundations_RectangleProfile";
            string typeName = "Rectangle Foundation";

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

            List<BH.oM.Geometry.Line> boundary = ExtractBoundary(element);
            var (width, length) = GetRectangleDimensions(boundary);
            double depth = GetThicknessFromConstr(element);

            Parameter widthParam = foundationSymbol.LookupParameter("Width");
            widthParam?.Set(width);

            Parameter lengthParam = foundationSymbol.LookupParameter("Length");
            lengthParam?.Set(length);

            Parameter depthParam = foundationSymbol.LookupParameter("Depth");
            depthParam?.Set(depth);

            return foundationSymbol;
        }

        /***************************************************/

        public static XYZ GetFoundationOrigin(this PadFoundation element)
        {
            List<oM.Geometry.Line> boundary = ExtractBoundary(element);
            if (boundary == null) return null;

            oM.Geometry.Point centerPoint = boundary.Centroid();
            Cartesian localCS = new(centerPoint, Vector.XAxis, Vector.YAxis, Vector.ZAxis);
            TransformMatrix orientationMatrix = BH.Engine.Geometry.Create.OrientationMatrixGlobalToLocal(localCS);

            Transform transform = orientationMatrix.ToRevit().TryFixIfNonConformal();
            return transform.Origin;
        }
    }
}







