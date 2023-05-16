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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns potential leader start points of a tag. Revit will choose one of these for the tag leader's start point based on the location of this tag's elbow or arrow point.")]
        [Input("tag", "A LocatedTag object representing an existing tag in the model.")]
        [Output("points", "Potential leader start points of a tag.")]
        public static List<XYZ> TagLeaderStartCandidates(this PlacedTag tag)
        {
            var candidates = new List<XYZ>();
            XYZ XVector = tag.Tag.Document.ActiveView.RightDirection;
            XYZ YVector = tag.Tag.Document.ActiveView.UpDirection;

            candidates.Add(tag.Center + XVector);
            candidates.Add(tag.Center - XVector);
            candidates.Add(tag.Center + YVector);
            candidates.Add(tag.Center - YVector);

            return candidates;
        }

        /***************************************************/
    }
}

