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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Set the elbow angle of a tag's leader to the required value.")]
        [Input("tag", "Representation of an existing tag in the model.")]
        [Input("startDir", "The starting direction of the tag's leader line.")]
        [Input("elbowAngle", "The desired angle between segments of the leader line.")]
        public static void SetElbowAngle(this PlacedTag tag, XYZ startDir, double elbowAngle)
        {
            if (tag.Tag.HasLeader == false)
                return;

            startDir = startDir.Normalize();
            TagLeaderCurve leaderCurve = tag.TagLeaderCurve();
            XYZ leaderVect = leaderCurve.End - leaderCurve.Start;

            if (leaderVect.GetLength() < Tolerance.Distance)
                return;

            if (leaderVect.DotProduct(startDir) < 0)
                startDir = -startDir;

            Transform vInverseTransform = tag.ViewTransform.Inverse;
            XYZ viewNormal = tag.Tag.OwnerView().ViewDirection;
            Transform rotation = Transform.CreateRotation(viewNormal, elbowAngle);

            XYZ cand1 = rotation.OfVector(startDir);
            XYZ cand2 = rotation.Inverse.OfVector(startDir);
            XYZ endDir = cand1.DotProduct(leaderVect) > cand2.DotProduct(leaderVect) ? cand1 : cand2;

            if (tag.LeaderStartPointInXY != null)
            {
                tag.SetLeaderCurve(startDir, endDir);
            }
            else
            {
                tag.ApproximateLeaderCurve(vInverseTransform.OfVector(startDir), vInverseTransform.OfVector(endDir));
            }
        }
        /***************************************************/
    }
}



