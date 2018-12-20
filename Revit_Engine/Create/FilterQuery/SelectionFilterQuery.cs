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
        [Description("Creates FilterQuery which filters selected Revit elements")]
        [Output("FilterQuery")]
        public static FilterQuery SelectionFilterQuery()
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.IncludeSelected] = true;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters all elements by given ElementIds.")]
        [Input("elementIds", "ElementIds of elements to be filtered")]
        [Output("FilterQuery")]
        public static FilterQuery SelectionFilterQuery(IEnumerable<int> elementIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.ElementIds] = elementIds;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters all elements by given UniqueIds.")]
        [Input("uniqueIds", "UniqueIds of elements to be filtered")]
        [Output("FilterQuery")]
        public static FilterQuery SelectionFilterQuery(IEnumerable<string> uniqueIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.UniqueIds] = uniqueIds;
            return aFilterQuery;
        }
    }
}
