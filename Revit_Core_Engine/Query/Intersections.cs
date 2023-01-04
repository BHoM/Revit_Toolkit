/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the segment of a curve that are inside a given solid. For no intersection, an empty list of curves is returned.")]
        [Input("curve", "Curve to get the intersection for.")]
        [Input("solid", "Solid to get the intersection for.")]
        [Output("curves", "Segments of the input curve that are inside the input solid.")]
        public static List<Curve> Intersections(this Curve curve, Solid solid)
        {
            if (curve == null || solid == null)
            {
                return null;
            }

            List<Curve> curves = new List<Curve>();
            SolidCurveIntersection intersection = solid.IntersectWithCurve(curve, new SolidCurveIntersectionOptions());

            if (intersection == null)
                return curves;

            foreach(Curve intersectionCurve in intersection)
            {
                curves.Add(intersectionCurve);
            }

            return curves;
        }

        /***************************************************/

        [Description("Intersection result of two curves. For no intersection, an empty list is returned.")]
        [Input("curve1", "First curve to check the intersection for.")]
        [Input("curve2", "Second curve to check the intersection for.")]
        [Output("points", "List of intersecting points.")]
        public static List<XYZ> Intersections(this Curve curve1, Curve curve2)
        {
            if (curve1 == null || curve2 == null)
            {
                return null;
            }

            List<XYZ> points = new List<XYZ>();
            curve1.Intersect(curve2, out IntersectionResultArray intersectionResultArray);

            if (intersectionResultArray == null)
            {
                return points;
            }

            foreach (IntersectionResult intersectionResult in intersectionResultArray)
            {
                XYZ intersectionPoint = intersectionResult.XYZPoint;
                points.Add(intersectionPoint);
            }

            return points;
        }

        /***************************************************/

        [Description("Intersection of curve and CurveLoop. For no intersection, an empty list is returned.")]
        [Input("curve", "Curve to check the intersection for.")]
        [Input("curveLoop", "CurveLoop to check the intersection for.")]
        [Output("points", "List of intersecting points.")]
        public static List<XYZ> Intersections(this Curve curve, CurveLoop curveLoop)
        {
            if (curve == null || curveLoop == null)
            {
                return null;
            }

            List<XYZ> intersectionPoints = new List<XYZ>();
            foreach (Curve bCurve in curveLoop)
            {
                List<XYZ> points = curve.Intersections(bCurve);
                intersectionPoints.AddRange(points);
            }

            return intersectionPoints;
        }

        /***************************************************/

        [Description("UV parameters of intersection points of two curves. For no intersection, an empty list is returned.")]
        [Input("curve1", "First curve to check the intersection for.")]
        [Input("curve2", "Second curve to check the intersection for.")]
        [Output("uvPoints", "List of UV parameters points.")]
        public static List<UV> UVIntersections(this Curve curve1, Curve curve2)
        {
            if (curve1 == null || curve2 == null)
            {
                return null;
            }

            List<UV> uvPoints = new List<UV>();
            curve1.Intersect(curve2, out IntersectionResultArray intersectionResultArray);

            if (intersectionResultArray == null)
            {
                return uvPoints;
            }

            foreach (IntersectionResult intersectionResult in intersectionResultArray)
            {
                UV intersectionPoint = intersectionResult.UVPoint;
                uvPoints.Add(intersectionPoint);
            }

            return uvPoints;
        }

        /***************************************************/

        [Description("UV parameters of intersection points of curve and CurveLoop. For no intersection, an empty list is returned.")]
        [Input("curve", "Curve to check the intersection for.")]
        [Input("curveLoop", "CurveLoop to check the intersection for.")]
        [Output("points", "List of intersecting points.")]
        public static List<UV> UVIntersections(this Curve curve, CurveLoop curveLoop)
        {
            if (curve == null || curveLoop == null)
            {
                return null;
            }

            List<UV> intersectionUVPoints = new List<UV>();
            foreach (Curve bCurve in curveLoop)
            {
                List<UV> points = curve.UVIntersections(bCurve);
                intersectionUVPoints.AddRange(points);
            }

            return intersectionUVPoints;
        }

        /***************************************************/

    }
}
