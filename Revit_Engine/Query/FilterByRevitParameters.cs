/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static BH.Engine.Adapters.Revit.Query;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("criteria", "A compact list of filtering criteria: parameterNames, filterTypes, values")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static List<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, List<List<object>> criteria)
        {
            // Validate that criteria contains exactly 3 lists
            if (criteria == null || criteria.Count != 3)
            {
                Base.Compute.RecordError("Criteria must contain exactly three lists: parameterNames, filterTypes, values.");
                return null;
            }

            var filterCriteria = BuildFilterCriteria(
                criteria[0].Cast<string>().ToList(),
                criteria[1].Cast<string>().ToList(),
                criteria[2].ToList()
            );

            return FilterByRevitParameters(bHoMObjects, filterCriteria);
        }

        [Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("parameterNames", "A list of Revit parameter names to filter by.")]
        [Input("filterTypes", "The types of filtering to apply.")]
        [Input("values", "The values to filter the parameters against.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static List<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<string> parameterNames, IEnumerable<string> filterTypes, IEnumerable<object> values)
        {
            if (parameterNames == null || filterTypes == null || values == null)
            {
                Base.Compute.RecordError("One or more arguments are null.");
                return null;
            }

            var filterCriteria = BuildFilterCriteria(
                parameterNames.ToList(),
                filterTypes.ToList(),
                values.ToList()
            );

            return FilterByRevitParameters(bHoMObjects, filterCriteria);
        }

        [Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("parameterNames", "A list of parameter names to filter by.")]
        [Input("filterTypes", "A list of filter types corresponding to the parameters.")]
        [Input("values", "A list of values used for filtering.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static List<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<RevitFilterCriteria> criteria)
        {
            if (criteria == null || !criteria.Any())
                return bHoMObjects.ToList();

            IRevitParameterFilterService filterService = new RevitParameterFilterService();
            return filterService.Filter(bHoMObjects, criteria);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static IEnumerable<RevitFilterCriteria> BuildFilterCriteria(List<string> parameterNames, List<string> filterTypes, List<object> values)
        {
            if (parameterNames == null || filterTypes == null || values == null)
            {
                Base.Compute.RecordError("One or more arguments are null.");
                return null;
            }

            if (parameterNames.Count != filterTypes.Count || parameterNames.Count != values.Count)
            {
                Base.Compute.RecordError("All lists (parameterNames, filterTypes, values) must have the same number of elements.");
                return null;
            }                

            var filters = new List<RevitFilterCriteria>();

            for (int i = 0; i < parameterNames.Count; i++)
            {
                FilterType filterTypeEnum = FilterType.NoFilter;

                if (!string.IsNullOrEmpty(filterTypes[i]) && filterTypes[i] != "-")
                {
                    if (!Enum.TryParse(filterTypes[i], out filterTypeEnum))
                        BH.Engine.Base.Compute.RecordError($"Invalid filter type: {filterTypes[i]}");
                }

                filters.Add(new RevitFilterCriteria
                {
                    ParameterName = parameterNames[i],
                    ParameterValue = values[i],
                    FilterType = filterTypeEnum
                });
            }

            return filters;
        }
    }
}
