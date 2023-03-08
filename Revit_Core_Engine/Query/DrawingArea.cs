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
        [Input("titleBlock", "Title Block symbol to get the drawing area outline from.")]
        [Output("outline", "The Title Block's drawing area.")]
        public static Outline DrawingArea(this FamilySymbol titleBlock)
        {
            Document document = titleBlock.Document;
            Document familyDoc = document.EditFamily(titleBlock.Family);
            List<DetailLine> lines = new FilteredElementCollector(familyDoc).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Lines).WhereElementIsNotElementType().Where(x => x is DetailLine).Cast<DetailLine>().ToList();

            var compositeGeom = new CompositeGeometry();

            foreach (DetailLine dLine in lines)
            {
                var bhomLine = dLine.GeometryCurve.IFromRevit() as BH.oM.Geometry.Line;

                if (bhomLine != null)
                    compositeGeom.Elements.Add(bhomLine);
            }

            BH.oM.Geometry.Point centrePoint = compositeGeom.Bounds().Centre();
            
            var leftLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint - Vector.XAxis * 10);
            var rightLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint + Vector.XAxis * 10);
            var upLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint + Vector.YAxis * 10);
            var downLine = BH.Engine.Geometry.Create.Line(centrePoint, centrePoint - Vector.YAxis * 10);

            double leftBound = double.MinValue;
            double rightBound = double.MaxValue;
            double downBound = double.MinValue;
            double upBound = double.MaxValue;

            foreach (BH.oM.Geometry.Line bhomLine in compositeGeom.Elements)
            {
                var leftPoint = leftLine.LineIntersection(bhomLine);
                if (leftPoint != null && leftPoint.X > leftBound)
                    leftBound = leftPoint.X;

                var rightPoint = rightLine.LineIntersection(bhomLine);
                if (rightPoint != null && rightPoint.X < rightBound)
                    rightBound = rightPoint.X;

                var downPoint = downLine.LineIntersection(bhomLine);
                if (downPoint != null && downPoint.Y > downBound)
                    downBound = downPoint.Y;

                var upPoint = upLine.LineIntersection(bhomLine);
                if (upPoint != null && upPoint.Y < upBound)
                    upBound = upPoint.Y;
            }

            var minPoint = BH.Engine.Geometry.Create.Point(leftBound, downBound);
            var maxPoint = BH.Engine.Geometry.Create.Point(rightBound, upBound);

            return new Outline(minPoint.ToRevit(), maxPoint.ToRevit());
        }
    }

    /***************************************************/
}
