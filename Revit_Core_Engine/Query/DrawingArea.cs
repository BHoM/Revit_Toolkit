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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
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

        [Description("Compute the outline of Title Block drawing area.")]
        [Input("document", "Revit current document to be processed.")]
        [Input("titleBlock", "Title Block symbol to get the outline drawing area from.")]
        [Output("outline", "Title Block drawing area.")]
        public static Outline DrawingArea(this Document document, FamilySymbol titleBlock)
        {
            Document familyDoc = document.EditFamily(titleBlock.Family);
            List<DetailLine> lines = new FilteredElementCollector(familyDoc).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Lines).WhereElementIsNotElementType().Select(x => x as DetailLine).ToList();

            BH.oM.Geometry.CompositeGeometry compositeGeom = new BH.oM.Geometry.CompositeGeometry();

            foreach (DetailLine dLine in lines)
            {
                BH.oM.Geometry.Line bhomLine = (dLine.Location as LocationCurve).Curve.IFromRevit() as BH.oM.Geometry.Line;
                compositeGeom.Elements.Add(bhomLine);
            }

            BH.oM.Geometry.Point centrePoint = compositeGeom.Bounds().Centre();

            var upLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint.Translate(Vector.YAxis * 10));
            var rightLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint.Translate(Vector.XAxis * 10));
            var downLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint.Translate(-Vector.YAxis * 10));
            var leftLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint.Translate(-Vector.XAxis * 10));

            List<BH.oM.Geometry.Point> upPoints = new List<BH.oM.Geometry.Point>();
            List<BH.oM.Geometry.Point> rightPoints = new List<BH.oM.Geometry.Point>();
            List<BH.oM.Geometry.Point> downPoints = new List<BH.oM.Geometry.Point>();
            List<BH.oM.Geometry.Point> leftPoints = new List<BH.oM.Geometry.Point>();

            foreach (BH.oM.Geometry.Line bhomLine in compositeGeom.Elements)
            {
                var upPoint = upLine.LineIntersection(bhomLine);
                if (upPoint != null)
                    upPoints.Add(upPoint);

                var rightPoint = rightLine.LineIntersection(bhomLine);
                if (rightPoint != null)
                    rightPoints.Add(rightPoint);

                var downPoint = downLine.LineIntersection(bhomLine);
                if (downPoint != null)
                    downPoints.Add(downPoint);

                var leftPoint = leftLine.LineIntersection(bhomLine);
                if (leftPoint != null)
                    leftPoints.Add(leftPoint);
            }

            var areaUpPoint = upPoints.OrderBy(x => x.Distance(centrePoint)).FirstOrDefault();
            var areaRightPoint = rightPoints.OrderBy(x => x.Distance(centrePoint)).FirstOrDefault();
            var areaDownPoint = downPoints.OrderBy(x => x.Distance(centrePoint)).FirstOrDefault();
            var areaLeftPoint = leftPoints.OrderBy(x => x.Distance(centrePoint)).FirstOrDefault();

            List<BH.oM.Geometry.Point> areaPoints = new List<BH.oM.Geometry.Point> { areaUpPoint, areaRightPoint, areaDownPoint, areaLeftPoint };

            var minX = areaPoints.Select(x => x.X).OrderBy(x => x).FirstOrDefault();
            var minY = areaPoints.Select(x => x.Y).OrderBy(x => x).FirstOrDefault();
            var maxX = areaPoints.Select(x => x.X).OrderByDescending(x => x).FirstOrDefault();
            var maxY = areaPoints.Select(x => x.Y).OrderByDescending(x => x).FirstOrDefault();

            var minPoint = BH.Engine.Geometry.Create.Point(minX, minY);
            var maxPoint = BH.Engine.Geometry.Create.Point(maxX, maxY);

            Outline drawingAreaOutline = new Outline(minPoint.ToRevit(), maxPoint.ToRevit());

            return drawingAreaOutline;
        }
    }

    /***************************************************/
}
