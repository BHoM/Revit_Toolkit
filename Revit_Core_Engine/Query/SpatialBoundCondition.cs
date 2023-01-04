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
using Autodesk.Revit.DB.Structure;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Check if a room/space/area is bounded, unplaced, redundant, or not enclosed.")]
        [Input("elem", "A spatial element to be check if it's bounded, unplaced, redundant, or not enclosed.")]
        [Output("BoundCondition", "The bound condition of the spatial element to be checked.")]
        public static BoundCondition? SpatialBoundCondition(this SpatialElement elem)
        {
            if (elem == null)
                return null;

            if (elem.Area == 0)
            {
                if (elem.Location == null)
                {
                    return BoundCondition.Unplaced;
                }
                else if (elem.GetBoundarySegments(new SpatialElementBoundaryOptions()).Count > 0)
                {
                    return BoundCondition.Redundant;
                }
                else
                {
                    return BoundCondition.NotEnclosed;
                }
            }
            return BoundCondition.Bounded;
        }

        /***************************************************/
    }
}

