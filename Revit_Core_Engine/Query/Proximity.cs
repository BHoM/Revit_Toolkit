/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        [Description("Calculates a pair of closest points on two elements as well as the distance between them." +
                     "\nThis is a backend method, working with the Revit object model and in Revit (imperial) units.")]
        [Input("element1", "First element in the pair.")]
        [Input("element2", "Second element in the pair.")]
        [Input("options", "Options to be applied when querying element solids.")]
        [Input("tolerance", "Distance at which the solution is considered convergent. By default set to 1e-3 ft as a practical value.")]
        [MultiOutput(0, "point1", "Point on first element that is closest to the second element.")]
        [MultiOutput(1, "point2", "Point on second element that is closest to the first element.")]
        [MultiOutput(2, "distance", "Distance between the found points.")]
        public static Output<XYZ, XYZ, double> Proximity(this Element element1, Element element2, Options options = null, double tolerance = 1e-3)
        {
            if (options == null)
            {
                options = new Options();
                options.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Medium;
                options.IncludeNonVisibleObjects = false;
            }

            options.ComputeReferences = false;
            List<Solid> solids1 = element1.Solids(options);
            List<Solid> solids2 = element2.Solids(options);

            Output<XYZ, XYZ, double> proximity = solids1.Proximity(solids2, tolerance);
            if (double.IsNaN(proximity.Item3))
                BH.Engine.Base.Compute.RecordWarning($"Can't compute the distance between elements {element1.Id.IntegerValue} and {element2.Id.IntegerValue}.");

            return proximity;
        }

        /***************************************************/

        [Description("Calculates a pair of closest points on two sets of solids as well as the distance between them." +
                     "\nThis is a backend method, working with the Revit object model and in Revit (imperial) units.")]
        [Input("solids1", "First set of solids in the pair.")]
        [Input("solids2", "Second set of solids in the pair.")]
        [Input("tolerance", "Distance at which the solution is considered convergent. By default set to 1e-3 ft as a practical value.")]
        [MultiOutput(0, "point1", "Point on first set of solids that is closest to the second set of solids.")]
        [MultiOutput(1, "point2", "Point on second set of solids that is closest to the first set of solids.")]
        [MultiOutput(2, "distance", "Distance between the found points.")]
        public static Output<XYZ, XYZ, double> Proximity(this IEnumerable<Solid> solids1, IEnumerable<Solid> solids2, double tolerance = 1e-3)
        {
            Output<XYZ, XYZ, double> min = new Output<XYZ, XYZ, double> { Item3 = double.MaxValue };
            foreach (Solid solid1 in solids1)
            {
                foreach (Solid solid2 in solids2)
                {
                    Output<XYZ, XYZ, double> proximity = solid1.Proximity(solid2, tolerance);
                    if (double.IsNaN(proximity.Item3))
                        continue;

                    if (proximity.Item3 < tolerance)
                        return proximity;

                    if (proximity.Item3 < min.Item3)
                        min = proximity;
                }
            }

            if (min.Item3 == double.MaxValue)
                min.Item3 = double.NaN;

            return min;
        }

        /***************************************************/

        [Description("Calculates a pair of closest points on two solids as well as the distance between them." +
                     "\nThis is a backend method, working with the Revit object model and in Revit (imperial) units.")]
        [Input("solid1", "First solid in the pair.")]
        [Input("solid2", "Second solid in the pair.")]
        [Input("tolerance", "Distance at which the solution is considered convergent. By default set to 1e-3 ft as a practical value.")]
        [MultiOutput(0, "point1", "Point on first solid that is closest to the second solid.")]
        [MultiOutput(1, "point2", "Point on second solid that is closest to the first solid.")]
        [MultiOutput(2, "distance", "Distance between the found points.")]
        public static Output<XYZ, XYZ, double> Proximity(this Solid solid1, Solid solid2, double tolerance = 1e-3)
        {
            if (solid1.DoesIntersect(solid2))
                return new Output<XYZ, XYZ, double> { Item3 = 0 };

            List<Face> faces1 = solid1.Faces.Cast<Face>().Where(x => x.Area > tolerance).ToList();
            List<Face> faces2 = solid2.Faces.Cast<Face>().Where(x => x.Area > tolerance).ToList();

            Output<XYZ, XYZ, double> min = new Output<XYZ, XYZ, double> { Item3 = double.MaxValue };
            foreach (Face face1 in faces1)
            {
                BoundingBoxUV bbox = face1.GetBoundingBox();
                XYZ centre = (face1.Evaluate(bbox.Min) + face1.Evaluate(bbox.Max)) / 2;

                foreach (Face face2 in faces2)
                {
                    Output<XYZ, XYZ, double> proximity = face1.Proximity(face2, centre, tolerance);
                    if (double.IsNaN(proximity.Item3))
                        continue;

                    if (proximity.Item3 < tolerance)
                        return proximity;

                    if (proximity.Item3 < min.Item3)
                        min = proximity;
                }
            }

            foreach (Face fc2 in faces2)
            {
                BoundingBoxUV bbox = fc2.GetBoundingBox();
                XYZ centre = (fc2.Evaluate(bbox.Min) + fc2.Evaluate(bbox.Max)) / 2;

                foreach (Face fc1 in faces1)
                {
                    Output<XYZ, XYZ, double> proximity = fc2.Proximity(fc1, centre, tolerance);
                    if (double.IsNaN(proximity.Item3))
                        continue;

                    if (proximity.Item3 < tolerance)
                        return proximity;

                    if (proximity.Item3 < min.Item3)
                        min = proximity;
                }
            }

            if (min.Item3 == double.MaxValue)
                min.Item3 = double.NaN;

            return min;
        }

        /***************************************************/

        [Description("Calculates a pair of closest points on two curves as well as the distance between them." +
                     "\nThis is a backend method, working with the Revit object model and in Revit (imperial) units.")]
        [Input("curve1", "First curve in the pair.")]
        [Input("curve2", "Second curve in the pair.")]
        [MultiOutput(0, "point1", "Point on first curve that is closest to the second curve.")]
        [MultiOutput(1, "point2", "Point on second curve that is closest to the first curve.")]
        [MultiOutput(2, "distance", "Distance between the found points.")]
        public static Output<XYZ, XYZ, double> Proximity(this Curve curve1, Curve curve2)
        {
            BH.oM.Geometry.Line line1 = curve1.IFromRevit() as BH.oM.Geometry.Line;
            BH.oM.Geometry.Line line2 = curve2.IFromRevit() as BH.oM.Geometry.Line;

            if (line1 != null && line2 != null && line1.IsParallel(line2) == 1)
            {
                Output<oM.Geometry.Point, oM.Geometry.Point> points = line1.CurveProximity(line2);
                XYZ point1 = points.Item1.ToRevit();
                XYZ point2 = points.Item2.ToRevit();
                return new Output<XYZ, XYZ, double> { Item1 = point1, Item2 = point2, Item3 = point1.DistanceTo(point2) };
            }

            IList<ClosestPointsPairBetweenTwoCurves> result = new List<ClosestPointsPairBetweenTwoCurves>();
            try
            {
                curve1.ComputeClosestPoints(curve2, true, true, false, out result);
            }
            catch
            {
                return null;
            }

            ClosestPointsPairBetweenTwoCurves closestPoints = result.FirstOrDefault();

            return new Output<XYZ, XYZ, double> { Item1 = closestPoints.XYZPointOnFirstCurve, Item2 = closestPoints.XYZPointOnSecondCurve, Item3 = closestPoints.Distance };
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Output<XYZ, XYZ, double> Proximity(this Face face1, Face face2, XYZ start, double tolerance)
        {
            double difference;
            XYZ current = start;
            Face face = face2;

            double distance = 0;
            XYZ point1 = current;
            XYZ point2 = current;

            do
            {
                (double, XYZ) prox = face.Proximity(current, tolerance);
                if (prox.Item2 == null)
                    return new Output<XYZ, XYZ, double> { Item3 = double.NaN };

                current = prox.Item2;
                if (face == face1)
                {
                    face = face2;
                    point1 = current;
                }
                else
                {
                    face = face1;
                    point2 = current;
                }

                difference = Math.Abs(prox.Item1 - distance);
                distance = prox.Item1;
            }
            while (difference > tolerance);

            return new Output<XYZ, XYZ, double> { Item1 = point1, Item2 = point2, Item3 = distance };
        }

        /***************************************************/

        private static (double, XYZ) Proximity(this Face face, XYZ point, double tolerance)
        {
            IntersectionResult ir = null;
            try
            {
                ir = face.Project(point);
            }
            catch (Exception e)
            {

            }

            if (ir != null)
                return (ir.Distance, ir.XYZPoint);

            (double, XYZ) min = (double.MaxValue, null);
            IEnumerable<Curve> edges = face.GetEdgesAsCurveLoops().SelectMany(x => x.Cast<Curve>());
            foreach (Curve edge in edges)
            {
                ir = null;
                try
                {
                    ir = edge.Project(point);
                }
                catch (Exception e)
                {

                }

                if (ir == null)
                    continue;

                if (ir.Distance < min.Item1)
                    min = (ir.Distance, ir.XYZPoint);

                if (min.Item1 <= tolerance)
                    break;
            }

            return min;
        }

        /***************************************************/
    }
}
