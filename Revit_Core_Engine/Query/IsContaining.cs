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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Checks whether the given XYZ point is inside the given Revit element.")]
        [Input("element", "Revit element to be checked whether it contains the XYZ point.")]
        [Input("point", "XYZ point to be checked whether it is inside the element.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("inside", "True if the input XYZ point is inside the input Revit element, otherwise false.")]
        public static bool IsContaining(this Element element, XYZ point, RevitSettings settings)
        {
            if (point == null || element == null)
                return false;

            settings = settings.DefaultIfNull();
            double tolerance = settings.DistanceTolerance;

            BoundingBoxXYZ bbox = element.get_BoundingBox(null);
            if (bbox == null || !bbox.IsContaining(point, tolerance))
                return false;

            return element.Solids(new Options(), settings).Any(x => x.IsContaining(point, tolerance));
        }

        /***************************************************/

        [Description("Checks whether the given XYZ point is inside the given Revit solid.")]
        [Input("solid", "Revit solid to be checked whether it contains the XYZ point.")]
        [Input("point", "XYZ point to be checked whether it is inside the solid.")]
        [Input("tolerance", "Distance tolerance to be used while performing the query.")]
        [Output("inside", "True if the input XYZ point is inside the input Revit solid, otherwise false.")]
        public static bool IsContaining(this Solid solid, XYZ point, double tolerance = BH.oM.Geometry.Tolerance.Distance)
        {
            if (point == null || solid == null || solid.Volume < tolerance)
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

        [Description("Checks whether the given XYZ point is inside the given Revit bounding box.")]
        [Input("bbox", "Revit bounding box to be checked whether it contains the XYZ point.")]
        [Input("point", "XYZ point to be checked whether it is inside the bounding box.")]
        [Input("tolerance", "Distance tolerance to be used while performing the query.")]
        [Output("inside", "True if the input XYZ point is inside the input Revit bounding box, otherwise false.")]
        public static bool IsContaining(this BoundingBoxXYZ bbox, XYZ point, double tolerance = BH.oM.Geometry.Tolerance.Distance)
        {
            if (point == null || bbox == null)
                return false;

            XYZ max = bbox.Max;
            XYZ min = bbox.Min;

            return (point.X >= min.X - tolerance && point.X <= max.X + tolerance &&
                    point.Y >= min.Y - tolerance && point.Y <= max.Y + tolerance &&
                    point.Z >= min.Z - tolerance && point.Z <= max.Z + tolerance);
        }

        /***************************************************/
    }
}


