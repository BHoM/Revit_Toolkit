/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

        [Description("Intersection result of two curves. If there is no intersection, an empty list is returned.")]
        [Input("curve1", "First curve to check the intersection for.")]
        [Input("curve2", "Second curve to check the intersection for.")]
        [Output("points", "List of intersecting points.")]
        public static List<XYZ> Intersection(this Curve curve1, Curve curve2)
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

        [Description("Intersection of curve and curveloop. If there is no intersection, an empty list is returned.")]
        [Input("curve", "Curve to check the intersection for.")]
        [Input("curveLoop", "CurveLoop to check the intersection for.")]
        [Output("points", "List of intersecting points.")]
        public static List<XYZ> Intersection(this Curve curve, CurveLoop curveLoop)
        {
            if (curve == null || curveLoop == null)
            {
                return null;
            }

            List<XYZ> intersectionPoints = new List<XYZ>();
            foreach (Curve bCurve in curveLoop)
            {
                List<XYZ> points = bCurve.Intersection(curve);
                intersectionPoints.AddRange(points);
            }

            return intersectionPoints;
        }

        /***************************************************/

    }
}