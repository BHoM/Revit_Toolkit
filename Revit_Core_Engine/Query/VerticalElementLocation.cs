/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the location line of a BHoM Pile or Column object in preparation to push to Revit. Includes validity checks and flipping reversed nodes.")]
        [Input("element", "A BHoM Pile or Column object to extract the line from.")]
        [Input("settings", "Revit adapter settings to be used while performing the operation.")]
        [Output("line", "Preprocessed location line of the BHoM Pile or Column object to be used on push to Revit.")]
        public static Line VerticalElementLocation(this IFramingElement element, RevitSettings settings)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError($"Cannot read location line because column is null. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            Line location = element.Location as Line;
            if (location == null)
            {
                BH.Engine.Base.Compute.RecordError($"Invalid location line. Only linear piles and columns are allowed in Revit. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            if (Math.Abs(location.Start.Z - location.End.Z) <= settings.DistanceTolerance)
            {
                BH.Engine.Base.Compute.RecordError($"Location line's start and end points have the same elevation. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            if (location.Start.Z > location.End.Z)
            {
                BH.Engine.Base.Compute.RecordNote($"The input location line's bottom was above its top. This line has been flipped to allow pushing to Revit. BHoM_Guid: {element.BHoM_Guid}");
                location = location.Flip();
            }

            return location;
        }

        /***************************************************/
    }
}

