/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using System;
using BH.oM.Geometry;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public Plane Plane(this PolyCurve polyCurve, double tolerance = Tolerance.Distance)
        {
            if (polyCurve == null)
                return null;

            List<Point> aPointList = Geometry.Query.ControlPoints(polyCurve);

            if (aPointList == null || aPointList.Count < 2)
                return null;

            if (!Geometry.Query.IsCoplanar(aPointList, tolerance))
                return null;

            return Plane(aPointList);

        }

        /***************************************************/

        static public Plane Plane(this Polyline polyline, double tolerance = Tolerance.Distance)
        {
            if (polyline == null)
                return null;

            if (polyline.ControlPoints == null || polyline.ControlPoints.Count < 2)
                return null;

            if (!Geometry.Query.IsCoplanar(polyline.ControlPoints, tolerance))
                return null;

            return Plane(polyline.ControlPoints);

        }

        /***************************************************/

        static public Plane Plane(this IEnumerable<Point> points)
        {
            if (points == null || points.Count() < 2)
                return null;

            Point aPoint_1 = points.ElementAt(0);
            Point aPoint_2 = null;
            Point aPoint_3 = null;

            for (int i = 1; i < points.Count() - 1; i++)
            {
                aPoint_2 = points.ElementAt(i);
                for (int j = 1; j < points.Count() - 1; j++)
                {
                    aPoint_3 = points.ElementAt(j);

                    Vector aVector = Geometry.Query.CrossProduct(aPoint_2 - aPoint_1, aPoint_3 - aPoint_1);
                    if (Geometry.Query.Length(aVector) > Tolerance.Distance)
                        return Geometry.Create.Plane(aPoint_1, aPoint_2, aPoint_3);
                }
            }

            return null;
        }

        /***************************************************/
    }
}