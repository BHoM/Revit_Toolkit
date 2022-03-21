/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System.ComponentModel;


namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the physical bounds (i.e. the combined bounds of all volumetric solids) of a given Revit element.")]
        [Input("element", "Revit element to extract the bounds from.")]
        [Input("transform", "Optional transform of the bounding box's coordinate system.")]
        [Output("bounds", "Physical bounds of the input Revit element.")]
        public static BoundingBoxXYZ PhysicalBounds(this Element element, Transform transform = null)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("Could not extract physical bounds from a null Revit element.");
                return null;
            }

            Options options = new Options();
            options.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Fine;
            options.ComputeReferences = false;
            options.IncludeNonVisibleObjects = false;

            return element?.Solids(options)?.Bounds(transform);
        }

        /***************************************************/
    }
}
