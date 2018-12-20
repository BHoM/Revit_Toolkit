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

using System.Linq;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets child FilterQueries for given FiletrQuery (FilterQuery can be combined by logical operation - LogicalAndSelectionFilter, LogicalOrSelectionFilter).")]
        [Input("filterQuery", "FilterQuery")]
        [Output("FilterQueries")]
        public static IEnumerable<FilterQuery> FilterQueries(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.FilterQueries))
                return null;

            if (filterQuery.Equalities[Convert.FilterQuery.FilterQueries] is IEnumerable<FilterQuery>)
                return (IEnumerable<FilterQuery>)filterQuery.Equalities[Convert.FilterQuery.FilterQueries];

            if (filterQuery.Equalities[Convert.FilterQuery.FilterQueries] is IEnumerable<object>)
            {
                IEnumerable<object> aObjects = filterQuery.Equalities[Convert.FilterQuery.FilterQueries] as IEnumerable<object>;
                if (aObjects != null)
                    return aObjects.Cast<FilterQuery>();
            }

            return null;
        }

        /***************************************************/
    }
}
