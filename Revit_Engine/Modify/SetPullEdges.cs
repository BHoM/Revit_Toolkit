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

using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Sets Pull Edges option for FilterQuery.")]
        [Input("filterQuery", "FilterQuery")]
        [Input("pullEdges", "Set to true if include geometry edges of Revit Element in CustomData of BHoMObject")]
        [Output("FilterQuery")]
        public static FilterQuery SetPullEdges(this FilterQuery filterQuery, bool pullEdges)
        {
            if (filterQuery == null)
                return null;

            FilterQuery aFilterQuery = Query.Duplicate(filterQuery);

            if (aFilterQuery.Equalities.ContainsKey(Convert.FilterQuery.PullEdges))
                aFilterQuery.Equalities[Convert.FilterQuery.PullEdges] = pullEdges;
            else
                aFilterQuery.Equalities.Add(Convert.FilterQuery.PullEdges, pullEdges);

            return aFilterQuery;
        }

        /***************************************************/
    }
}
