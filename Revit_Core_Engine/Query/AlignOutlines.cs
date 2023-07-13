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
using BH.oM.Revit.Enums;
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

        [Description("Aligns outlines in the bounding outline in given direction.")]
        [Input("boudingOutline", "Bounding outline in which outline will be aligned.")]
        [Input("outlines", "Outlines to aligned in the bounding outline.")]
        [Input("offset", "Distance between each outline.")]
        [Input("direction", "Direction of the outline alignment.")]
        [Output("alignedOutlines", "Outlines aligned in the bounding outline.")]
        public static List<Outline> AlignOutlines(this Outline boudingOutline, List<Outline> outlines, double offset, OutlineAlignment direction)
        {
            List<Outline> alignedOutlines = new List<Outline>();
            XYZ topLeftPoint = new XYZ(boudingOutline.MinimumPoint.X, boudingOutline.MaximumPoint.Y, 0);
            XYZ topRightPoint = new XYZ(boudingOutline.MaximumPoint.X, boudingOutline.MaximumPoint.Y, 0);
            List<List<Outline>> listOfOutlines = DivideOutlinesInRows(boudingOutline, outlines, offset, direction);

            if (direction == OutlineAlignment.HorizontalFromLeftToRight)
            {
                XYZ newTopLeftPoint = topLeftPoint;
                foreach (List<Outline> outlinesInRow in listOfOutlines)
                {
                    List<Outline> alignedOutlinesInRow = outlinesInRow.OutlinesInRow(newTopLeftPoint, offset, direction);
                    alignedOutlines.AddRange(alignedOutlinesInRow);
                    Outline rowOutlinesBound = alignedOutlinesInRow.Bounds();

                    newTopLeftPoint = new XYZ(rowOutlinesBound.MinimumPoint.X, rowOutlinesBound.MinimumPoint.Y - offset, 0);
                }
            }
            else if (direction == OutlineAlignment.VerticalFromLeftToRight)
            {
                XYZ newTopLeftPoint = topLeftPoint;
                foreach (List<Outline> outlinesInRow in listOfOutlines)
                {
                    List<Outline> alignedOutlinesInRow = outlinesInRow.OutlinesInRow(newTopLeftPoint, offset, direction);
                    alignedOutlines.AddRange(alignedOutlinesInRow);
                    Outline rowOutlinesBound = alignedOutlinesInRow.Bounds();

                    newTopLeftPoint = new XYZ(rowOutlinesBound.MaximumPoint.X + offset, rowOutlinesBound.MaximumPoint.Y, 0);
                }
            }
            else if (direction == OutlineAlignment.HorizontalFromRightToLeft)
            {
                XYZ newTopRightPoint = topRightPoint;
                foreach (List<Outline> outlinesInRow in listOfOutlines)
                {
                    List<Outline> alignedOutlinesInRow = outlinesInRow.OutlinesInRow(newTopRightPoint, offset, direction);
                    alignedOutlines.AddRange(alignedOutlinesInRow);
                    Outline rowOutlinesBound = alignedOutlinesInRow.Bounds();

                    newTopRightPoint = new XYZ(rowOutlinesBound.MaximumPoint.X, rowOutlinesBound.MinimumPoint.Y - offset, 0);
                }
            }
            else if (direction == OutlineAlignment.VerticalFromRightToLeft)
            {
                XYZ newTopRightPoint = topRightPoint;
                foreach (List<Outline> outlinesInRow in listOfOutlines)
                {
                    List<Outline> alignedOutlinesInRow = outlinesInRow.OutlinesInRow(newTopRightPoint, offset, direction);
                    alignedOutlines.AddRange(alignedOutlinesInRow);
                    Outline rowOutlinesBound = alignedOutlinesInRow.Bounds();

                    newTopRightPoint = new XYZ(rowOutlinesBound.MinimumPoint.X - offset, rowOutlinesBound.MaximumPoint.Y, 0);
                }
            }

            return alignedOutlines;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static List<List<Outline>> DivideOutlinesInRows(this Outline drawingArea, List<Outline> outlines, double offset, OutlineAlignment direction)
        {
            List<List<Outline>> listOfOutlines = new List<List<Outline>>();
            XYZ topLeftPoint = new XYZ(drawingArea.MinimumPoint.X, drawingArea.MaximumPoint.Y, 0);
            double drawingAreaWidth = drawingArea.MaximumPoint.X - drawingArea.MinimumPoint.X;
            double drawingAreaHeight = drawingArea.MaximumPoint.Y - drawingArea.MinimumPoint.Y;

            if (direction == OutlineAlignment.HorizontalFromLeftToRight || direction == OutlineAlignment.HorizontalFromRightToLeft)
            {
                List<Outline> pendingOutlines = outlines;

                while (pendingOutlines.Count > 0)
                {
                    List<Outline> rowOutlines = pendingOutlines;
                    List<Outline> alignedRowOutlines = rowOutlines.OutlinesInRow(topLeftPoint, offset, direction);
                    Outline rowOutline = alignedRowOutlines.Bounds();
                    double rowOutlineWidth = rowOutline.MaximumPoint.X - rowOutline.MinimumPoint.X;

                    while (rowOutlineWidth > drawingAreaWidth)
                    {
                        rowOutlines = rowOutlines.Take(rowOutlines.Count - 1).ToList();
                        alignedRowOutlines = rowOutlines.OutlinesInRow(topLeftPoint, offset, direction);
                        rowOutline = alignedRowOutlines.Bounds();
                        rowOutlineWidth = rowOutline.MaximumPoint.X - rowOutline.MinimumPoint.X;
                    }

                    listOfOutlines.Add(rowOutlines);
                    pendingOutlines = pendingOutlines.Skip(rowOutlines.Count).ToList();
                }
            }
            else if (direction == OutlineAlignment.VerticalFromLeftToRight || direction == OutlineAlignment.VerticalFromRightToLeft)
            {
                List<Outline> pendingOutlines = outlines;

                while (pendingOutlines.Count > 0)
                {
                    List<Outline> rowOutlines = pendingOutlines;
                    List<Outline> alignedRowOutlines = rowOutlines.OutlinesInRow(topLeftPoint, offset, direction);
                    Outline rowOutline = alignedRowOutlines.Bounds();
                    double rowOutlineHeight = rowOutline.MaximumPoint.Y - rowOutline.MinimumPoint.Y;

                    while (rowOutlineHeight > drawingAreaHeight)
                    {
                        rowOutlines = rowOutlines.Take(rowOutlines.Count - 1).ToList();
                        alignedRowOutlines = rowOutlines.OutlinesInRow(topLeftPoint, offset, direction);
                        rowOutline = alignedRowOutlines.Bounds();
                        rowOutlineHeight = rowOutline.MaximumPoint.Y - rowOutline.MinimumPoint.Y;
                    }

                    listOfOutlines.Add(rowOutlines);
                    pendingOutlines = pendingOutlines.Skip(rowOutlines.Count).ToList();
                }
            }

            return listOfOutlines;
        }

        /***************************************************/

        private static List<Outline> OutlinesInRow(this List<Outline> outlines, XYZ startingPoint, double offset, OutlineAlignment direction)
        {
            List<Outline> outlinesInRow = new List<Outline>();

            for (int i = 0; i < outlines.Count; i++)
            {
                Outline outline = outlines[i];
                double outlineWidth = outline.MaximumPoint.X - outline.MinimumPoint.X;
                double outlineHeight = outline.MaximumPoint.Y - outline.MinimumPoint.Y;

                if (direction == OutlineAlignment.HorizontalFromLeftToRight)
                {
                    double outlineTopLeftX;

                    if (i == 0)
                        outlineTopLeftX = startingPoint.X;
                    else
                        outlineTopLeftX = startingPoint.X + offset;

                    double outlineTopLeftY = startingPoint.Y;

                    XYZ outlineNewCenterPoint = new XYZ(outlineTopLeftX + outlineWidth / 2, outlineTopLeftY - outlineHeight / 2, 0);
                    outline = outline.MovedToCenterPoint(outlineNewCenterPoint);
                    outlinesInRow.Add(outline);

                    startingPoint = new XYZ(outlineTopLeftX + outlineWidth, outlineTopLeftY, 0);
                }
                else if (direction == OutlineAlignment.VerticalFromLeftToRight)
                {
                    double outlineTopLeftX = startingPoint.X;
                    double outlineTopLeftY;

                    if (i == 0)
                        outlineTopLeftY = startingPoint.Y;
                    else
                        outlineTopLeftY = startingPoint.Y - offset;

                    XYZ outlineNewCenterPoint = new XYZ(outlineTopLeftX + outlineWidth / 2, outlineTopLeftY - outlineHeight / 2, 0);
                    outline = outline.MovedToCenterPoint(outlineNewCenterPoint);
                    outlinesInRow.Add(outline);

                    startingPoint = new XYZ(outlineTopLeftX, outlineTopLeftY - outlineHeight, 0);
                }
                else if (direction == OutlineAlignment.HorizontalFromRightToLeft)
                {
                    double outlineTopRightX;

                    if (i == 0)
                        outlineTopRightX = startingPoint.X;
                    else
                        outlineTopRightX = startingPoint.X - offset;

                    double outlineTopRightY = startingPoint.Y;

                    XYZ outlineNewCenterPoint = new XYZ(outlineTopRightX - outlineWidth / 2, outlineTopRightY - outlineHeight / 2, 0);
                    outline = outline.MovedToCenterPoint(outlineNewCenterPoint);
                    outlinesInRow.Add(outline);

                    startingPoint = new XYZ(outlineTopRightX - outlineWidth, outlineTopRightY, 0);
                }
                else if (direction == OutlineAlignment.VerticalFromRightToLeft)
                {
                    double outlineTopRightX = startingPoint.X;
                    double outlineTopRightY;

                    if (i == 0)
                        outlineTopRightY = startingPoint.Y;
                    else
                        outlineTopRightY = startingPoint.Y - offset;

                    XYZ outlineNewCenterPoint = new XYZ(outlineTopRightX - outlineWidth / 2, outlineTopRightY - outlineHeight / 2, 0);
                    outline = outline.MovedToCenterPoint(outlineNewCenterPoint);
                    outlinesInRow.Add(outline);

                    startingPoint = new XYZ(outlineTopRightX, outlineTopRightY - outlineHeight, 0);
                }
            }

            return outlinesInRow;
        }

        /***************************************************/

        private static Outline MovedToCenterPoint(this Outline outline, XYZ centerPoint)
        {
            double outlineWidth = outline.MaximumPoint.X - outline.MinimumPoint.X;
            double outlineHeight = outline.MaximumPoint.Y - outline.MinimumPoint.Y;
            XYZ halfDiagonal = new XYZ(outlineWidth / 2, outlineHeight / 2, 0);

            XYZ newMin = centerPoint - halfDiagonal;
            XYZ newMax = centerPoint + halfDiagonal;

            return new Outline(newMin, newMax);
        }

        /***************************************************/
    }
}

