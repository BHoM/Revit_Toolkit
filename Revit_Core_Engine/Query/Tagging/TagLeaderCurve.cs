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
using BH.Revit.Engine.Core.Objects;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a wrapper for line segments of a tag's leader.")]
        [Input("tag", "A LocatedTag object representing an existing tag in the model.")]
        [Output("tagLeaderCurve", "A wrapper for line segments of a tag's leader.")]
        public static TagLeaderCurve TagLeaderCurve(this PlacedTag tag)
        {
            if (!tag.Tag.HasLeader)
                return null;

            XYZ end = tag.Tag.TagLeaderEnd();
            XYZ elbow = tag.Tag.HasElbow() ? tag.Tag.FirstLeaderElbow() : end;
            XYZ start = tag.LeaderStartPoint ?? elbow.ClosestPoint(tag.TagLeaderStartCandidates());

            return new TagLeaderCurve() { Start = start, Elbow = elbow, End = end };
        }

        /***************************************************/
    }
}

