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
using BH.oM.Geometry;
using BH.Revit.Engine.Core.Objects;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Modify segments of an existing tag's leader so that they follow the input start & end directions.")]
        [Input("tag", "Representation of an existing tag in the model.")]
        [Input("startDir", "The starting direction of the tag's leader line.")]
        [Input("endDir", "The ending direction of the tag's leader line.")]
        [Input("startPoint", "The start point of the tag's leader line.")]
        [Input("endPoint", "The end point of the tag's leader line.")]
        [Input("iteration", "The number of times this method has tried to set the leader curve as close as possible to the desired path.")]
        public static void ApproximateLeaderCurve(this PlacedTag tag, XYZ startDir, XYZ endDir, XYZ startPoint = null, XYZ endPoint = null, int iteration = 0)
        {
            if (iteration == 5)
                return;

            iteration++;
            IndependentTag existingTag = tag.Tag;
            Transform vTransform = tag.ViewTransform;

            if (1 - Math.Abs(startDir.DotProduct(endDir)) > Tolerance.Angle)
            {
                if (startPoint == null)
                    startPoint = tag.CenterInXY;

                if (endPoint == null)
                    endPoint = tag.HostLocationInXY;

                XYZ elbowPnt = Core.Compute.VectorIntersection(startPoint, endPoint, startDir, endDir);
                /***************************************************/
                //var line1 = new BhLine { Start = startPoint.PointFromRevit(), End = (startPoint + startDir).PointFromRevit(), Infinite = true };
                //var line2 = new BhLine { Start = endPoint.PointFromRevit(), End = (endPoint + endDir).PointFromRevit(), Infinite = true };
                //XYZ elbowPnt = line1.LineIntersection(line2, true).ToRevit();
                /***************************************************/

                existingTag.SetLeaderElbow(vTransform.OfPoint(elbowPnt));
                startPoint = vTransform.OfPoint(startPoint);

                existingTag.Document.Regenerate();
                XYZ closestLeaderStartPnt = existingTag.FirstLeaderElbow().ClosestPoint(tag.TagLeaderStartCandidates());

                if (iteration == 1 || startPoint.DistanceTo(closestLeaderStartPnt) > Tolerance.Distance)
                {
                    XYZ nextStartPnt = vTransform.Inverse.OfPoint(closestLeaderStartPnt);

                    tag.ApproximateLeaderCurve(startDir, endDir, nextStartPnt, tag.HostLocationInXY, iteration);
                }
            }
            else if (Math.Abs(tag.CenterInXY.X - tag.HostLocationInXY.X) <= Tolerance.Distance
                || Math.Abs(tag.CenterInXY.Y - tag.HostLocationInXY.Y) <= Tolerance.Distance)
            {
                existingTag.SetLeaderElbow(tag.HostLocation.ClosestPoint(tag.TagLeaderStartCandidates()));
            }
        }

        /***************************************************/
    }
}