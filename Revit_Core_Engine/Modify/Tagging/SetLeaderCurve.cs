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

        [Description("Set the elbow point of a tag's leader so that it forms a required angle.")]
        [Input("tag", "A tag whose leader needs to follow specific directions to form a required angle.")]
        [Input("startDir", "The starting direction of the leader line segment.")]
        [Input("endDir", "The ending direction of the leader line segment.")]
        public static void SetLeaderCurve(this PlacedTag tag, XYZ startDir, XYZ endDir)
        {
            IndependentTag existingTag = tag.Tag;

            if (1 - Math.Abs(startDir.DotProduct(endDir)) > Tolerance.Angle)
            {
                var endPoint = tag.HostLocation;
                var startPoint = tag.LeaderStartPoint;
                XYZ elbowPnt = Core.Compute.VectorIntersection(startPoint, endPoint, startDir, endDir);

                existingTag.SetLeaderElbow(elbowPnt);
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