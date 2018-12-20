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
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which combines other FilterQueries by logical and operator.")]
        [Input("filterQueries", "Filter Queries to be combined")]
        [Output("FilterQuery")]
        public static FilterQuery LogicalAndFilterQuery(IEnumerable<FilterQuery> filterQueries)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.LogicalAnd;
            aFilterQuery.Equalities[Convert.FilterQuery.FilterQueries] = filterQueries;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which combines two FilterQueries by logical and operator.")]
        [Input("filterQuery_1", "First FilterQuery to be combined")]
        [Input("filterQuery_2", "Second FilterQuery to be combined")]
        [Output("FilterQuery")]
        public static FilterQuery LogicalAndFilterQuery(FilterQuery filterQuery_1, FilterQuery filterQuery_2)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.LogicalAnd;
            aFilterQuery.Equalities[Convert.FilterQuery.FilterQueries] = new List<FilterQuery>() { filterQuery_1, filterQuery_2 };
            return aFilterQuery;
        }
    }
}
