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
using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.ICurve to a Revit CurveLoop.")]
        [Input("curve", "BH.oM.Geometry.ICurve to be converted.")]
        [Output("curveLoop", "Revit CurveLoop resulting from converting the input BH.oM.Geometry.ICurve.")]
        public static CurveLoop ToRevitCurveLoop(this BH.oM.Geometry.ICurve curve)
        {
            CurveLoop curveLoop = new CurveLoop();
            foreach (Curve revitCurve in curve.IToRevitCurves())
            {
                curveLoop.Append(revitCurve);
            }

            return curveLoop;
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.PolyCurve to a Revit CurveLoop by cleaning it: removing short segments, sorting for contiguity, projecting to a best-fit plane, and closing the loop.")]
        [Input("curve", "BH.oM.Geometry.PolyCurve to be cleaned and converted.")]
        [Output("curveLoop", "Revit CurveLoop resulting from cleaning the input BH.oM.Geometry.PolyCurve, or null if a valid loop could not be produced.")]
        public static CurveLoop ToValidCurveLoop(this BH.oM.Geometry.PolyCurve curve)
        {
            double bhomLengthTolerance = BH.oM.Adapters.Revit.Tolerance.ShortCurve;
            double revitLengthTolerance = bhomLengthTolerance / 0.3048;
            double bhomVertexTolerance = BH.oM.Adapters.Revit.Tolerance.Vertex;
            double revitVertexTolerance = bhomVertexTolerance / 0.3048;

            // Remove segments that are too short for Revit's tolerance
            List<BH.oM.Geometry.ICurve> subParts = curve.SubParts()
                .Where(x => x.ILength() > bhomLengthTolerance)
                .ToList();

            if (subParts.Count == 0)
                return null;

            // Sort segments into a contiguous end-to-end chain
            BH.oM.Geometry.PolyCurve polyCurve = new BH.oM.Geometry.PolyCurve { Curves = subParts };
            try { polyCurve = polyCurve.SortCurves(bhomVertexTolerance); }
            catch
            {
                BH.Engine.Base.Compute.RecordWarning("Could not sort the sub-curves of the input PolyCurve into a contiguous chain. The original segment order will be used, which may result in a non-contiguous CurveLoop.");
            }

            subParts = polyCurve.SubParts().Where(x => x.ILength() > bhomLengthTolerance).ToList();
            if (subParts.Count < 2)
                return null;

            // Project all segments onto a best-fit plane to ensure planarity
            polyCurve = new BH.oM.Geometry.PolyCurve { Curves = subParts };
            BH.oM.Geometry.Plane fitPlane = polyCurve.FitPlane();
            if (fitPlane != null)
            {
                polyCurve = polyCurve.Project(fitPlane);
                subParts = polyCurve.SubParts().Where(x => x.ILength() > bhomLengthTolerance).ToList();
            }

            if (subParts.Count < 2)
                return null;

            // Close the loop with an explicit line segment if the gap exceeds vertex tolerance
            BH.oM.Geometry.Point loopStart = subParts.First().IStartPoint();
            BH.oM.Geometry.Point loopEnd = subParts.Last().IEndPoint();
            double closingGap = loopStart.Distance(loopEnd);

            if (closingGap > bhomVertexTolerance && closingGap > bhomLengthTolerance)
                subParts.Add(new BH.oM.Geometry.Line { Start = loopEnd, End = loopStart });

            // Build the CurveLoop, snapping Line start points to the previous end for exact contiguity
            CurveLoop loop = new CurveLoop();
            XYZ lastEnd = null;
            foreach (BH.oM.Geometry.ICurve seg in subParts)
            {
                foreach (Curve revitCurve in seg.IToRevitCurves())
                {
                    if (revitCurve == null)
                        continue;

                    XYZ curveStart = revitCurve.GetEndPoint(0);
                    XYZ curveEnd = revitCurve.GetEndPoint(1);

                    if (curveStart.DistanceTo(curveEnd) <= revitLengthTolerance)
                        continue;

                    Curve toAppend = revitCurve;
                    if (lastEnd != null && revitCurve is Line)
                    {
                        double gap = curveStart.DistanceTo(lastEnd);
                        if (gap > 0 && gap <= revitVertexTolerance * 2 && lastEnd.DistanceTo(curveEnd) > revitLengthTolerance)
                            toAppend = Line.CreateBound(lastEnd, curveEnd);
                    }

                    try
                    {
                        loop.Append(toAppend);
                        lastEnd = toAppend.GetEndPoint(1);
                    }
                    catch { }
                }
            }

            return loop.Count() >= 3 ? loop : null;
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.PolyCurve to a Revit CurveLoop made entirely of line segments by tessellating all sub-curves. Serves as a last resort to produce a valid Revit boundary when other conversions fail.")]
        [Input("curve", "BH.oM.Geometry.PolyCurve to be tessellated and converted.")]
        [Output("curveLoop", "Revit CurveLoop of line segments resulting from tessellating the input BH.oM.Geometry.PolyCurve, or null if a valid loop could not be produced.")]
        public static CurveLoop ToTessellatedCurveLoop(this BH.oM.Geometry.PolyCurve curve)
        {
            double bhomLengthTolerance = BH.oM.Adapters.Revit.Tolerance.ShortCurve;
            double revitLengthTolerance = bhomLengthTolerance / 0.3048;
            double revitVertexTolerance = BH.oM.Adapters.Revit.Tolerance.Vertex / 0.3048;

            // Tessellate all sub-curves into a single polyline approximation
            BH.oM.Geometry.Polyline polyline = curve.CollapseToPolyline(BH.oM.Geometry.Tolerance.Angle);
            if (polyline == null || polyline.ControlPoints.Count < 3)
                return null;

            // Remove collinear vertices and short segments
            polyline = polyline.CleanPolyline(BH.oM.Geometry.Tolerance.Angle, bhomLengthTolerance);
            if (polyline == null || polyline.ControlPoints.Count < 3)
                return null;

            // Project to a best-fit plane to enforce planarity
            BH.oM.Geometry.Plane fitPlane = polyline.FitPlane();
            if (fitPlane != null)
            {
                polyline = polyline.Project(fitPlane);
                polyline = polyline.CleanPolyline(BH.oM.Geometry.Tolerance.Angle, bhomLengthTolerance);
                if (polyline == null || polyline.ControlPoints.Count < 3)
                    return null;
            }

            // Ensure the polyline is closed
            List<BH.oM.Geometry.Point> pts = polyline.ControlPoints.ToList();
            if (pts.First().Distance(pts.Last()) > bhomLengthTolerance)
            {
                pts.Add(pts.First());
                polyline = new BH.oM.Geometry.Polyline { ControlPoints = pts };
            }

            // Build the CurveLoop from line segments with endpoint snapping for exact contiguity
            CurveLoop loop = new CurveLoop();
            XYZ lastEnd = null;
            foreach (BH.oM.Geometry.Line seg in polyline.SubParts())
            {
                XYZ start = seg.Start.ToRevit();
                XYZ end = seg.End.ToRevit();

                if (lastEnd != null && start.DistanceTo(lastEnd) > 0 && start.DistanceTo(lastEnd) <= revitVertexTolerance * 2)
                    start = lastEnd;

                if (start.DistanceTo(end) <= revitLengthTolerance)
                    continue;

                try
                {
                    Line revitLine = Line.CreateBound(start, end);
                    loop.Append(revitLine);
                    lastEnd = end;
                }
                catch { }
            }

            return loop.Count() >= 3 ? loop : null;
        }

        /***************************************************/
    }
}









