/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.ComponentModel;

using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Pull Edges option for FilterRequest.")]
        [Input("filterRequest", "FilterRequest")]
        [Input("pullEdges", "Set to true if include geometry edges of Revit Element in CustomData of BHoMObject")]
        [Input("includeNonVisibleObjects", "Set to true if include non visible objects of Revit Element in geometry")]
        [Output("FilterRequest")]
        public static FilterRequest SetPullEdges(this FilterRequest filterRequest, bool pullEdges = true, bool includeNonVisibleObjects = false)
        {
            if (filterRequest == null)
                return null;

            FilterRequest aFilterRequest = Query.Duplicate(filterRequest);

            aFilterRequest.Equalities[Convert.FilterRequest.PullEdges] = pullEdges;
            aFilterRequest.Equalities[Convert.FilterRequest.IncludeNonVisibleObjects] = includeNonVisibleObjects;

            return aFilterRequest;
        }

        [Deprecated("2.2")]
        [Description("Sets Pull Edges option for FilterRequest.")]
        [Input("filterRequest", "FilterRequest")]
        [Input("pullEdges", "Set to true if include geometry edges of Revit Element in CustomData of BHoMObject")]
        [Output("FilterRequest")]
        public static FilterRequest SetPullEdges(this FilterRequest filterRequest, bool pullEdges)
        {
            return SetPullEdges(filterRequest, pullEdges, false);
        }

        /***************************************************/
    }
}
