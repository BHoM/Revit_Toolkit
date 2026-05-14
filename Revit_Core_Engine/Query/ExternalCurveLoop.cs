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
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
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

        [Description("Returns the main, external boundary curve loop of a planar face.")]
        [Input("face", "A planar face with and external boundary curve loop that can contains internal holes, if any.")]
        [Output("externalCurveLoop", "The main, external boundary curve loop of a planar face.")]
        public static CurveLoop ExternalCurveLoop(this PlanarFace face)
        {
            List<CurveLoop> crvLoops = face.GetEdgesAsCurveLoops().ToList();
            CurveLoop externalLoop = crvLoops.FirstOrDefault(x => !x.IsOpen() && x.IsCounterclockwise(face.FaceNormal));

            //The face may violate conventional winding directions, or Revit may see a complex curve loop as 'Open' and refuses to calculate IsCounterclockwise
            if (externalLoop == null)
            {
                //Checking areas is slower but these problematic faces rarely exist, so performance hit isn't bad.
                double maxArea = double.MinValue;

                foreach (CurveLoop loop in crvLoops)
                {
                    List<BH.oM.Geometry.Point> controlPoints = new List<BH.oM.Geometry.Point>();
                    foreach (Curve curve in loop)
                        controlPoints.AddRange(curve.Tessellate().Select(x => x.PointFromRevit()));

                    double area = new Polyline { ControlPoints = controlPoints }.Area();
                    if (area > maxArea)
                    {
                        maxArea = area;
                        externalLoop = loop;
                    }
                }
            }

            return externalLoop;
        }

        /***************************************************/

        [Description("Converts a closed Revit curve loop to a BHoM Polyline of straight segments (tessellated), for XY footprint queries such as pad longest-edge alignment.")]
        [Input("curveLoop", "Closed curve loop, typically the external boundary of a horizontal face.")]
        [Output("polyline", "Closed polyline in model coordinates, or null if the loop is invalid or degenerate.")]
        public static Polyline ToClosedPlanPolyline(this CurveLoop curveLoop)
        {
            if (curveLoop == null || curveLoop.IsOpen())
                return null;

            double tol = BH.oM.Geometry.Tolerance.Distance;
            List<BH.oM.Geometry.Point> pts = new List<BH.oM.Geometry.Point>();

            foreach (Curve curve in curveLoop)
            {
                IList<XYZ> tess = curve.Tessellate();
                if (tess == null || tess.Count == 0)
                    continue;

                for (int i = 0; i < tess.Count; i++)
                {
                    if (pts.Count > 0 && i == 0)
                        continue;

                    pts.Add(tess[i].PointFromRevit());
                }
            }

            if (pts.Count < 3)
                return null;

            if ((pts[pts.Count - 1] - pts[0]).Length() <= tol)
                pts.RemoveAt(pts.Count - 1);

            if (pts.Count < 3)
                return null;

            List<BH.oM.Geometry.Line> lines = new List<BH.oM.Geometry.Line>();
            int n = pts.Count;
            for (int i = 0; i < n; i++)
            {
                BH.oM.Geometry.Point a = pts[i];
                BH.oM.Geometry.Point b = pts[(i + 1) % n];
                if ((a - b).Length() > tol)
                    lines.Add(BH.Engine.Geometry.Create.Line(a, b));
            }

            if (lines.Count < 3)
                return null;

            return BH.Engine.Geometry.Create.Polyline(lines);
        }

        /***************************************************/
    }
}




