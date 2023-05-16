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
using System.ComponentModel;

namespace BH.Revit.Engine.Core.Objects
{
    [Description("Represents an existing tag in the model with enough data to adjust its position and leader shape.")]
    public class PlacedTag
    {
        /***************************************************/
        /****             Public properties             ****/
        /***************************************************/

        [Description("The existing tag in the model.")]
        public virtual IndependentTag Tag { get; set; } = null;

        [Description("The center point of the tag.")]
        public virtual XYZ Center { get; set; } = null;

        [Description("The center point of the tag at the XY plane's origin.")]
        public virtual XYZ CenterInXY { get; set; } = null;

        [Description("The start point of this tag's leader line.")]
        public virtual XYZ LeaderStartPoint { get; set; } = null;

        [Description("The start point of this tag's leader line at the XY plane's origin.")]
        public virtual XYZ LeaderStartPointInXY { get; set; } = null;

        [Description("The location of this tag's host element.")]
        public virtual XYZ HostLocation { get; set; } = null;

        [Description("The location of this tag's host element at the XY plane's origin.")]
        public virtual XYZ HostLocationInXY { get; set; } = null;

        [Description("The bounding box of this tag in its owner view.")]
        public virtual BoundingBoxXYZ BoundingBox { get; set; } = null;

        [Description("The transform from the model's global coordinates system to the local system of the view owning this tag.")]
        public virtual Transform ViewTransform { get; set; } = Transform.Identity;

        /***************************************************/
    }
}

