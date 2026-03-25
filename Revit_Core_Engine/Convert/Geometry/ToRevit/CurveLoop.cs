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
        [Input("plane", "Optional plane to project the curve onto before conversion. If null, a best-fit plane is computed. Returns null if no plane can be determined.")]
        [Input("forceClose", "If true, any gap between consecutive segments larger than Revit's short-curve tolerance is bridged with an explicit line. Default is false.")]
        [Output("curveLoop", "Revit CurveLoop resulting from cleaning the input BH.oM.Geometry.PolyCurve, or null if a valid loop could not be produced.")]
        public static CurveLoop ToRevitCurveLoop(this BH.oM.Geometry.PolyCurve curve, BH.oM.Geometry.Plane plane = null, bool forceClose = false)
        {
            double bhomLengthTolerance = BH.oM.Adapters.Revit.Tolerance.ShortCurve;
            double revitLengthTolerance = bhomLengthTolerance / 0.3048;
            double bhomVertexTolerance = BH.oM.Adapters.Revit.Tolerance.Vertex;

            List<BH.oM.Geometry.ICurve> subParts = curve.SubParts().ToList();
            if (subParts.Count == 0)
                return null;

            BH.oM.Geometry.PolyCurve polyCurve = new BH.oM.Geometry.PolyCurve { Curves = subParts };
            try
            {
                // Sort all segments first, before removing short ones, to avoid creating gaps:
                // a short segment may bridge two otherwise-disconnected segments, and removing it
                // first would prevent SortCurves from finding a contiguous chain.
                polyCurve = polyCurve.SortCurves(bhomVertexTolerance);
            }
            catch
            {
                BH.Engine.Base.Compute.RecordWarning("Could not sort the sub-curves of the input PolyCurve into a contiguous chain. The original segment order will be used, which may result in a non-contiguous CurveLoop.");
            }

            // Remove segments that are too short for Revit's tolerance after sorting
            subParts = polyCurve.SubParts().Where(x => x.ILength() > bhomLengthTolerance).ToList();
            if (subParts.Count < 2)
                return null;

            polyCurve = new BH.oM.Geometry.PolyCurve { Curves = subParts };
            BH.oM.Geometry.Plane fitPlane = plane ?? polyCurve.FitPlane();

            // Project all segments onto a best-fit plane to ensure planarity
            if (fitPlane == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Could not determine a fit plane for the input PolyCurve. The curve loop cannot be created.");
                return null;
            }

            polyCurve = polyCurve.Project(fitPlane);
            subParts = polyCurve.SubParts().Where(x => x.ILength() > bhomLengthTolerance).ToList();

            if (subParts.Count < 2)
                return null;

            // Handle self-intersecting curves: collapse to a polyline, split at intersections,
            // and keep only the largest resulting loop
            polyCurve = new BH.oM.Geometry.PolyCurve { Curves = subParts };
            if (polyCurve.IsSelfIntersecting(bhomLengthTolerance))
            {
                BH.oM.Geometry.Polyline collapsed = polyCurve.CollapseToPolyline(BH.oM.Geometry.Tolerance.Angle);
                if (collapsed != null)
                {
                    if (!collapsed.IsClosed(bhomLengthTolerance))
                        collapsed = collapsed.Close(bhomLengthTolerance);

                    List<BH.oM.Geometry.Polyline> parts = collapsed.SplitAtSelfIntersections(bhomLengthTolerance);
                    if (parts != null && parts.Count > 0)
                    {
                        BH.oM.Geometry.Polyline largest = parts.OrderByDescending(x => x.Bounds().DiagonalLength()).First();

                        subParts = largest.SubParts().Cast<BH.oM.Geometry.ICurve>()
                            .Where(x => x.ILength() > bhomLengthTolerance).ToList();

                        if (subParts.Count < 2)
                            return null;
                    }
                }
            }

            // Close the loop with an explicit line segment if the gap exceeds short-curve tolerance
            BH.oM.Geometry.Point loopStart = subParts.First().IStartPoint();
            BH.oM.Geometry.Point loopEnd = subParts.Last().IEndPoint();
            double closingGap = loopStart.Distance(loopEnd);

            if (closingGap > bhomLengthTolerance)
                subParts.Add(new BH.oM.Geometry.Line { Start = loopEnd, End = loopStart });

            XYZ lastEnd = null;
            CurveLoop loop = new CurveLoop();

            // Build the CurveLoop, snapping Line start points to the previous end for exact contiguity
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

                    double gap = lastEnd != null ? curveStart.DistanceTo(lastEnd) : 0;

                    if (forceClose && lastEnd != null && gap > revitLengthTolerance)
                    {
                        try
                        {
                            loop.Append(Line.CreateBound(lastEnd, curveStart));
                            lastEnd = curveStart;
                            gap = 0;
                        }
                        catch
                        {
                            BH.Engine.Base.Compute.RecordWarning("Failed to append a bridging line to the CurveLoop. The curve loop cannot be created.");
                            return null;
                        }
                    }

                    bool needsSnap = gap > 0 && gap <= revitLengthTolerance;

                    if (needsSnap && !(revitCurve is Line))
                    {
                        // Tessellate the non-Line curve and snap its first point to lastEnd
                        List<XYZ> tessellatedPts = revitCurve.Tessellate().ToList();
                        if (tessellatedPts.Count >= 2)
                        {
                            tessellatedPts[0] = lastEnd;
                            for (int i = 1; i < tessellatedPts.Count; i++)
                            {
                                if (tessellatedPts[i - 1].DistanceTo(tessellatedPts[i]) > revitLengthTolerance)
                                {
                                    try
                                    {
                                        loop.Append(Line.CreateBound(tessellatedPts[i - 1], tessellatedPts[i]));
                                        lastEnd = tessellatedPts[i];
                                    }
                                    catch
                                    {
                                        BH.Engine.Base.Compute.RecordWarning("Failed to append a tessellated curve segment to the CurveLoop. The curve loop cannot be created.");
                                        return null;
                                    }
                                }
                            }
                            continue;
                        }
                    }

                    Curve toAppend = revitCurve;
                    if (needsSnap && revitCurve is Line && lastEnd.DistanceTo(curveEnd) > revitLengthTolerance)
                        toAppend = Line.CreateBound(lastEnd, curveEnd);

                    try
                    {
                        loop.Append(toAppend);
                        lastEnd = toAppend.GetEndPoint(1);
                    }
                    catch
                    {
                        BH.Engine.Base.Compute.RecordWarning("Failed to append a curve segment to the CurveLoop. The curve loop cannot be created.");
                        return null;
                    }
                }
            }

            return loop.Count() >= 3 ? loop : null;
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.PolyCurve to a Revit CurveLoop made entirely of line segments by tessellating all sub-curves. Serves as a last resort to produce a valid Revit boundary when other conversions fail.")]
        [Input("curve", "BH.oM.Geometry.PolyCurve to be tessellated and converted.")]
        [Input("plane", "Optional plane to project the curve onto before conversion. If null, a best-fit plane is computed. Returns null if no plane can be determined.")]
        [Output("curveLoop", "Revit CurveLoop of line segments resulting from tessellating the input BH.oM.Geometry.PolyCurve, or null if a valid loop could not be produced.")]
        public static CurveLoop ToTessellatedRevitCurveLoop(this BH.oM.Geometry.PolyCurve curve, BH.oM.Geometry.Plane plane = null)
        {
            double bhomLengthTolerance = BH.oM.Adapters.Revit.Tolerance.ShortCurve;
            double revitLengthTolerance = bhomLengthTolerance / 0.3048;
            double revitVertexTolerance = BH.oM.Adapters.Revit.Tolerance.Vertex / 0.3048;

            // Sort segments first so that CollapseToPolyline produces a connected polyline;
            // out-of-order subparts would otherwise produce jumps in the tessellation
            BH.oM.Geometry.PolyCurve sortedCurve = new BH.oM.Geometry.PolyCurve { Curves = curve.SubParts().ToList() };
            try { sortedCurve = sortedCurve.SortCurves(BH.oM.Adapters.Revit.Tolerance.Vertex); }
            catch
            {
                BH.Engine.Base.Compute.RecordWarning("Could not sort the sub-curves of the input PolyCurve into a contiguous chain. The tessellated curve loop cannot be created.");
                return null;
            }

            // Tessellate all sub-curves into a single polyline approximation
            BH.oM.Geometry.Polyline polyline = sortedCurve.CollapseToPolyline(BH.oM.Geometry.Tolerance.Angle);
            if (polyline == null || polyline.ControlPoints.Count < 3)
                return null;

            // Remove collinear vertices and short segments
            polyline = polyline.CleanPolyline(BH.oM.Geometry.Tolerance.Angle, bhomLengthTolerance);
            if (polyline == null || polyline.ControlPoints.Count < 3)
                return null;

            // Project to a best-fit plane to enforce planarity
            BH.oM.Geometry.Plane fitPlane = plane ?? polyline.FitPlane();
            if (fitPlane == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Could not determine a fit plane for the input PolyCurve. The tessellated curve loop cannot be created.");
                return null;
            }

            polyline = polyline.Project(fitPlane);
            polyline = polyline.CleanPolyline(BH.oM.Geometry.Tolerance.Angle, bhomLengthTolerance);
            if (polyline == null || polyline.ControlPoints.Count < 3)
                return null;

            // Ensure the polyline is closed
            List<BH.oM.Geometry.Point> pts = polyline.ControlPoints.ToList();
            if (pts.First().Distance(pts.Last()) > bhomLengthTolerance)
            {
                pts.Add(pts.First());
                polyline = new BH.oM.Geometry.Polyline { ControlPoints = pts };
            }

            // Handle self-intersecting polylines: the polyline is already planar and closed,
            // so split directly and keep only the largest resulting loop
            if (polyline.IsSelfIntersecting(bhomLengthTolerance))
            {
                List<BH.oM.Geometry.Polyline> parts = polyline.SplitAtSelfIntersections(bhomLengthTolerance);
                if (parts == null || parts.Count == 0)
                    return null;
                polyline = parts.OrderByDescending(x => x.Bounds().DiagonalLength()).First();
                polyline = polyline.CleanPolyline(BH.oM.Geometry.Tolerance.Angle, bhomLengthTolerance);
                if (polyline == null || polyline.ControlPoints.Count < 3)
                    return null;
            }

            // Build the CurveLoop from line segments with endpoint snapping for exact contiguity
            CurveLoop loop = new CurveLoop();
            XYZ lastEnd = null;
            foreach (BH.oM.Geometry.Line seg in polyline.SubParts())
            {
                XYZ start = seg.Start.ToRevit();
                XYZ end = seg.End.ToRevit();

                if (lastEnd != null && start.DistanceTo(lastEnd) > 0 && start.DistanceTo(lastEnd) <= revitVertexTolerance)
                    start = lastEnd;

                if (start.DistanceTo(end) <= revitLengthTolerance)
                    continue;

                try
                {
                    Line revitLine = Line.CreateBound(start, end);
                    loop.Append(revitLine);
                    lastEnd = end;
                }
                catch
                {
                    BH.Engine.Base.Compute.RecordWarning("Failed to append a line segment to the CurveLoop. The curve loop cannot be created.");
                    return null;
                }
            }

            return loop.Count() >= 3 ? loop : null;
        }

        /***************************************************/
    }
}









