/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool IsContaining(this Element element, XYZ point, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();
            double tolerance = settings.DistanceTolerance;

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox == null || !bbox.IsContaining(point, tolerance))
                return false;

            return element.Solids(new Options(), settings).Any(x => x.IsContaining(point, tolerance));
        }

        /***************************************************/

        public static bool IsContaining(this Solid solid, XYZ point, double tolerance = BH.oM.Geometry.Tolerance.Distance)
        {
            if (solid == null || solid.Volume < tolerance)
                return false;

            SolidCurveIntersectionOptions sco = new SolidCurveIntersectionOptions();
            sco.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;

            XYZ[] vectors = { XYZ.BasisX, XYZ.BasisY, XYZ.BasisZ };
            foreach (XYZ vector in vectors)
            {
                Line l = Line.CreateBound(point - vector, point + vector);
                SolidCurveIntersection sci = solid.IntersectWithCurve(l, sco);
                if (sci == null)
                    continue;

                for (int i = 0; i < sci.SegmentCount; i++)
                {
                    Curve c = sci.GetCurveSegment(i);
                    IntersectionResult ir = c.Project(point);
                    if (ir != null && ir.Distance <= tolerance)
                        return true;
                }
            }

            return false;
        }

        /***************************************************/

        public static bool IsContaining(this BoundingBoxXYZ bbox, XYZ point, double tolerance = BH.oM.Geometry.Tolerance.Distance)
        {
            XYZ max = bbox.Max;
            XYZ min = bbox.Min;

            return (point.X >= min.X - tolerance && point.X <= max.X + tolerance &&
                    point.Y >= min.Y - tolerance && point.Y <= max.Y + tolerance &&
                    point.Z >= min.Z - tolerance && point.Z <= max.Z + tolerance);
        }

        /***************************************************/
    }
}

