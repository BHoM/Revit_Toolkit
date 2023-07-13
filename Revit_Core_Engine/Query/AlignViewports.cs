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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Base;
using System;
using System.ComponentModel;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using BH.oM.Revit.Enums;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds the location curve that should be assigned to the Revit FamilyInstance representing a framing element in order to make this instance's centroid overlap with the centreline of a given BHoM framing element, taken all offsets and justifications into account.")]
        [Input("framingElement", "BHoM framing element to align the Revit framing to.")]
        [Output("curve", "Location curve for the input Revit FamilyInstance that aligns its centreline to the input BHoM framing element.")]
        public static List<XYZ> AlignViewports(this Outline drawingArea, List<Viewport> viewports, double borderOffset, double viewportOffset, ViewportAlignmentDirection direction, out Outline totalOutline)
        {
            List<XYZ> viewportPoints = new List<XYZ>();
            totalOutline = null;

            //add border offset to drawing area
            XYZ borderOffsetVec = new XYZ(borderOffset, borderOffset, 0);
            drawingArea.MinimumPoint += borderOffsetVec;
            drawingArea.MaximumPoint -= borderOffsetVec;
            
            if (direction == ViewportAlignmentDirection.Horizontal)
            {
                //group viewports in rows
                List<Viewport> pendingViewports = viewports;
                List<List<Viewport>> listOfViewports = new List<List<Viewport>>();

                while (pendingViewports.Count > 0)
                {
                    List<Viewport> rowViewports = pendingViewports;
                    AlignViewportsInRow(drawingArea, rowViewports, viewportOffset, direction, out Outline rowOutline);

                    while (rowOutline.MaximumPoint.X > drawingArea.MaximumPoint.X)
                    {
                        rowViewports = rowViewports.Take(rowViewports.Count - 1).ToList();
                        AlignViewportsInRow(drawingArea, rowViewports, viewportOffset, direction, out rowOutline);
                    }

                    listOfViewports.Add(rowViewports);
                    pendingViewports = pendingViewports.Skip(rowViewports.Count).ToList();
                }

                //get viewport center points
                totalOutline = null;
                Outline newDrawingArea = new Outline(drawingArea);

                foreach (List<Viewport> viewportsInRow in listOfViewports)
                {
                    viewportPoints.AddRange(AlignViewportsInRow(newDrawingArea, viewportsInRow, viewportOffset, direction, out Outline rowOutline));
                    totalOutline = totalOutline.Add(rowOutline);

                    XYZ newDrawingAreaMaxPoint = new XYZ(totalOutline.MaximumPoint.X, totalOutline.MinimumPoint.Y - viewportOffset, 0);
                    newDrawingArea = new Outline(newDrawingArea.MinimumPoint, newDrawingAreaMaxPoint);
                }
            }
            else if (direction == ViewportAlignmentDirection.Vertical)
            {
                //group viewports in rows
                List<Viewport> pendingViewports = viewports;
                List<List<Viewport>> listOfViewports = new List<List<Viewport>>();

                while (pendingViewports.Count > 0)
                {
                    List<Viewport> rowViewports = pendingViewports;
                    AlignViewportsInRow(drawingArea, rowViewports, viewportOffset, direction, out Outline rowOutline);

                    while (rowOutline.MinimumPoint.Y < drawingArea.MinimumPoint.Y)
                    {
                        rowViewports = rowViewports.Take(rowViewports.Count - 1).ToList();
                        AlignViewportsInRow(drawingArea, rowViewports, viewportOffset, direction, out rowOutline);
                    }

                    listOfViewports.Add(rowViewports);
                    pendingViewports = pendingViewports.Skip(rowViewports.Count).ToList();
                }

                //get viewport center points
                totalOutline = null;
                Outline newDrawingArea = new Outline(drawingArea);

                foreach (List<Viewport> viewportsInRow in listOfViewports)
                {
                    viewportPoints.AddRange(AlignViewportsInRow(newDrawingArea, viewportsInRow, viewportOffset, direction, out Outline rowOutline));
                    totalOutline = totalOutline.Add(rowOutline);

                    XYZ newDrawingAreaMinPoint = new XYZ(totalOutline.MaximumPoint.X + viewportOffset, totalOutline.MinimumPoint.Y, 0);
                    newDrawingArea = new Outline(newDrawingAreaMinPoint, newDrawingArea.MaximumPoint);
                }
            }

            return viewportPoints;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Return center points of viewports in the given direction.")]
        private static List<XYZ> AlignViewportsInRow(this Outline drawingArea, List<Viewport> viewports, double viewportOffset, ViewportAlignmentDirection direction, out Outline totalOutline)
        {
            XYZ topLeftPoint = new XYZ(drawingArea.MinimumPoint.X, drawingArea.MaximumPoint.Y, 0);
            List<Outline> viewportOutlines = viewports.Select(x => x.GetBoxOutline()).ToList();
            List<Outline> alignedOutlines = viewportOutlines.OrganizeOutlinesInRow(topLeftPoint, viewportOffset, direction);
            List<XYZ> viewportCenterPoints = alignedOutlines.Select(x => x.CenterPoint()).ToList();
            totalOutline = alignedOutlines.Bounds();

            return viewportCenterPoints;
        }

        /***************************************************/

        private static List<Outline> OrganizeOutlinesInRow(this List<Outline> outlines, XYZ topLeftPoint, double offset, ViewportAlignmentDirection direction)
        {
            List<Outline> outlinesInRow = new List<Outline>();

            for (int i = 0; i < outlines.Count; i++)
            {
                Outline outline = outlines[i];
                double outlineWidth = outline.MaximumPoint.X - outline.MinimumPoint.X;
                double outlineHeight = outline.MaximumPoint.Y - outline.MinimumPoint.Y;

                if (direction == ViewportAlignmentDirection.Horizontal)
                {
                    double outlineTopLeftX;

                    if (i == 0)
                        outlineTopLeftX = topLeftPoint.X;
                    else
                        outlineTopLeftX = topLeftPoint.X + offset;

                    double outlineTopLeftY = topLeftPoint.Y;

                    XYZ outlineNewCenterPoint = new XYZ(outlineTopLeftX + outlineWidth / 2, outlineTopLeftY - outlineHeight / 2, 0);
                    outline = outline.MovedToCenterPoint(outlineNewCenterPoint);
                    outlinesInRow.Add(outline);

                    topLeftPoint = new XYZ(outlineTopLeftX + outlineWidth, outlineTopLeftY, 0);
                }
                else if (direction == ViewportAlignmentDirection.Vertical)
                {
                    double outlineTopLeftX = topLeftPoint.X;
                    double outlineTopLeftY;

                    if (i == 0)
                        outlineTopLeftY = topLeftPoint.Y;
                    else
                        outlineTopLeftY = topLeftPoint.Y - offset;

                    XYZ outlineNewCenterPoint = new XYZ(outlineTopLeftX + outlineWidth / 2, outlineTopLeftY - outlineHeight / 2, 0);
                    outline = outline.MovedToCenterPoint(outlineNewCenterPoint);
                    outlinesInRow.Add(outline);

                    topLeftPoint = new XYZ(outlineTopLeftX, outlineTopLeftY - outlineHeight, 0);
                }
            }

            return outlinesInRow;
        }

        /***************************************************/

        private static XYZ CenterPoint(this Outline outline)
        {
            double centerX = outline.MinimumPoint.X + (outline.MaximumPoint.X - outline.MinimumPoint.X) / 2;
            double centerY = outline.MinimumPoint.Y + (outline.MaximumPoint.Y - outline.MinimumPoint.Y) / 2;

            return new XYZ(centerX, centerY, 0);
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

        private static Outline Bounds(this List<Outline> outlines)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;

            foreach (Outline outline in outlines)
            {
                minX = Math.Min(outline.MinimumPoint.X, minX);
                minY = Math.Min(outline.MinimumPoint.Y, minY);
                maxX = Math.Max(outline.MaximumPoint.X, maxX);
                maxY = Math.Max(outline.MaximumPoint.Y, maxY);
            }

            XYZ minPoint = new XYZ(minX, minY, 0);
            XYZ maxPoint = new XYZ(maxX, maxY, 0);

            return new Outline(minPoint, maxPoint);
        }

        /***************************************************/

        private static Outline Add(this Outline outline, Outline outlineToAdd)
        {
            if (outline == null)
                return new Outline(outlineToAdd);

            double minX = Math.Min(outline.MinimumPoint.X, outlineToAdd.MinimumPoint.X);
            double minY = Math.Min(outline.MinimumPoint.Y, outlineToAdd.MinimumPoint.Y);
            double maxX = Math.Max(outline.MaximumPoint.X, outlineToAdd.MaximumPoint.X);
            double maxY = Math.Max(outline.MaximumPoint.Y, outlineToAdd.MaximumPoint.Y);

            XYZ minPoint = new XYZ(minX, minY, 0);
            XYZ maxPoint = new XYZ(maxX, maxY, 0);

            return new Outline(minPoint, maxPoint);
        }

        /***************************************************/
    }
}

