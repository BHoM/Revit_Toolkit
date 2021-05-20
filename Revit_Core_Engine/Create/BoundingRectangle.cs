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

using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.Engine.Geometry;
using BH.Engine.Spatial;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using Line = BH.oM.Geometry.Line;
using Point = BH.oM.Geometry.Point;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a rectangular CurveLoop by querying an element's bounding box and getting its bottom perimeter.")]
        [Input("element", "The element to query the bounding box for its rectangular bound.")]
        [Input("offset", "The offset value to apply to the resulting bounding box, enlarging or shrinking it.")]
        [Output("RectangleBounding", "A rectangular CurveLoop that fits the element in the horizontal plane.")]
        public static CurveLoop BoundingRectangle(Element element, double offset = 0)
        {
            BoundingBoxXYZ bb = element.get_BoundingBox(element.Document.ActiveView);

            Point p1 = new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z).PointFromRevit();
            Point p2 = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z).PointFromRevit();
            Point p3 = new XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z).PointFromRevit();
            Point p4 = new XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z).PointFromRevit();

            List<Point> points = new List<Point>();
            points.Add(p1);
            points.Add(p2);
            points.Add(p3);
            points.Add(p4);
            points.Add(p1);

            Polyline polyline = BH.Engine.Geometry.Create.Polyline(points).Offset(offset);
            
            return polyline.ToRevitCurveLoop();
        }

        /***************************************************/

    }
}