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
        public static List<XYZ> AlignViewports(this Outline drawingArea, List<Viewport> viewports, out Outline totalOutline, double borderOffset = 0.1, double viewportOffset = 0.1, AlignViewDirection direction = AlignViewDirection.Right)
        {
            int rows = 1;
            Outline rowOutline;
            List<XYZ> viewportPoints = AlignViewportsInRow(drawingArea, viewports, out rowOutline, borderOffset, viewportOffset, direction);
            totalOutline = new Outline(rowOutline);

            while (totalOutline.MaximumPoint.X > drawingArea.MaximumPoint.X)
            {
                rows++;

                double totalOutlineMinX = double.MaxValue;
                double totalOutlineMinY = double.MaxValue;
                double totalOutlineMaxX = double.MinValue;
                double totalOutlineMaxY = double.MinValue;

                Outline newDrawingArea = new Outline(drawingArea);
                viewportPoints = new List<XYZ>();

                int chunkSize = (int)Math.Ceiling((double)viewports.Count / rows);
                List<List<Viewport>> listOfViewports = viewports
                    .Select((value, index) => new { Index = index, Value = value })
                    .GroupBy(x => x.Index / chunkSize)
                    .Select(group => group.Select(x => x.Value).ToList())
                    .ToList();

                foreach (List<Viewport> viewportsInRow in listOfViewports)
                {
                    viewportPoints.AddRange(AlignViewportsInRow(newDrawingArea, viewportsInRow, out rowOutline, borderOffset, viewportOffset, direction));

                    if (rowOutline.MinimumPoint.X < totalOutlineMinX)
                        totalOutlineMinX = rowOutline.MinimumPoint.X;
                    if (rowOutline.MinimumPoint.Y < totalOutlineMinY)
                        totalOutlineMinY = rowOutline.MinimumPoint.Y;
                    if (rowOutline.MaximumPoint.X > totalOutlineMaxX)
                        totalOutlineMaxX = rowOutline.MaximumPoint.X;
                    if (rowOutline.MaximumPoint.Y > totalOutlineMaxY)
                        totalOutlineMaxY = rowOutline.MaximumPoint.Y;

                    XYZ newDrawingAreaMaxPoint = new XYZ(totalOutlineMaxX, totalOutlineMinY, 0);
                    newDrawingArea = new Outline(newDrawingArea.MinimumPoint, newDrawingAreaMaxPoint);
                }

                XYZ totalOutlineMinPoint = new XYZ(totalOutlineMinX, totalOutlineMinY, 0);
                XYZ totalOutlineMaxPoint = new XYZ(totalOutlineMaxX, totalOutlineMaxY, 0);

                totalOutline = new Outline(totalOutlineMinPoint, totalOutlineMaxPoint);
            }

            return viewportPoints;
        }

        /***************************************************/

        [Description("Finds the location curve that should be assigned to the Revit FamilyInstance representing a framing element in order to make this instance's centroid overlap with the centreline of a given BHoM framing element, taken all offsets and justifications into account.")]
        [Input("framingElement", "BHoM framing element to align the Revit framing to.")]
        [Output("curve", "Location curve for the input Revit FamilyInstance that aligns its centreline to the input BHoM framing element.")]
        public static List<XYZ> AlignViewportsInRow(this Outline drawingArea, List<Viewport> viewports, out Outline totalOutline, double borderOffset = 0.1, double viewportOffset = 0.1, AlignViewDirection direction = AlignViewDirection.Right)
        {
            //topleft point of drawing area with border offset
            XYZ topLeftPoint = new XYZ(drawingArea.MinimumPoint.X + borderOffset, drawingArea.MaximumPoint.Y - borderOffset, 0);
            List<XYZ> viewportCenterPoints = new List<XYZ>();

            XYZ viewportsTotalMinPoint = new XYZ();
            XYZ viewportsTotalMaxPoint = new XYZ();

            for (int i = 0; i < viewports.Count; i++)
            {
                Viewport viewport = viewports[i];
                Outline viewportOutline = viewport.GetBoxOutline();
                double viewportWidth = viewportOutline.MaximumPoint.X - viewportOutline.MinimumPoint.X;
                double viewportHeight = viewportOutline.MaximumPoint.Y - viewportOutline.MinimumPoint.Y;
                double viewportCenterX;

                if (i == 0)
                {
                    viewportCenterX = topLeftPoint.X + viewportWidth / 2;
                    viewportsTotalMinPoint = new XYZ(topLeftPoint.X, topLeftPoint.Y - viewportHeight, 0);
                }
                else
                    viewportCenterX = topLeftPoint.X + viewportOffset + viewportWidth / 2;

                double viewportCenterY = topLeftPoint.Y - viewportHeight / 2;
                XYZ viewportPoint = new XYZ(viewportCenterX, viewportCenterY, 0);
                viewportCenterPoints.Add(viewportPoint);

                topLeftPoint = new XYZ(viewportCenterX + viewportWidth / 2, viewportCenterY + viewportHeight / 2, 0);
                viewportsTotalMaxPoint = new XYZ(viewportPoint.X + viewportWidth / 2, viewportPoint.Y + viewportHeight / 2, 0);
            }

            totalOutline = new Outline(viewportsTotalMinPoint, viewportsTotalMaxPoint);

            return viewportCenterPoints;
        }

        /***************************************************/
    }
}

