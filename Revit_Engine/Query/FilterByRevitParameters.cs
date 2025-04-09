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
using BH.oM.Verification;
using BH.oM.Verification.Conditions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.Adapters.Revit.Parameters;
using BH.Engine.Verification;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        [Description("Filters a collection of BHoM objects by a collection of RevitParameter criterion.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("criteria", "The filtering criteria to apply.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<ValueCondition> criteria)
        {
            foreach (var crit in criteria)
            {
                if (crit == null) return bHoMObjects;
                bHoMObjects = bHoMObjects.Where(x => {
                    var typeId = x.GetRevitElementType().ElementId();
                    var isType = typeId == -1;
                    var validateCrit = crit.ValueSource as ParameterValueSource;
                    validateCrit.FromType = isType;
                    crit.ValueSource = validateCrit;
                    var result = x.VerifyCondition(crit);
                    return result.Passed != null && result.Passed.Value;
                });
            }
            return bHoMObjects.ToList();
        }

        [Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("criteria", "A compact list of filtering criteria: parameterNames, filterTypes, values")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, List<List<object>> criteria)
        {
            // Validate that criteria contains exactly 3 lists
            if (criteria == null || criteria.Count != 3)
            {
                Base.Compute.RecordError("Criteria must contain exactly three lists: parameterNames, filterTypes, values.");
                return null;
            }

            var filterCriteria = FiltersFromString(
                criteria[0].Cast<string>().ToList(),
                criteria[1].Cast<string>().ToList(),
                criteria[2].ToList()
            );

            return bHoMObjects.FilterByRevitParameters(filterCriteria);
        }

        [Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("parameterNames", "A list of Revit parameter names to filter by.")]
        [Input("filterTypes", "The types of filtering to apply.")]
        [Input("values", "The values to filter the parameters against.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<string> parameterNames, IEnumerable<string> filterTypes, IEnumerable<object> referenceValues)
        {
            if (parameterNames == null || filterTypes == null || referenceValues == null)
            {
                Base.Compute.RecordError("One or more arguments are null.");
                return null;
            }

            var filterCriteria = FiltersFromString(
                parameterNames.ToList(),
                filterTypes.ToList(),
                referenceValues.ToList()
            );

            return bHoMObjects.FilterByRevitParameters(filterCriteria);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static IEnumerable<ValueCondition> FiltersFromString(List<string> parameterNames, List<string> filterTypes, List<object> values)
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

            var filters = new List<ValueCondition>();

            for (int i = 0; i < parameterNames.Count; i++)
            {
                ValueComparisonType filterTypeEnum = ValueComparisonType.EqualTo;

                if (!string.IsNullOrEmpty(filterTypes[i]) && filterTypes[i] != "-")
                {
                    if (!Enum.TryParse(filterTypes[i], out filterTypeEnum))
                        BH.Engine.Base.Compute.RecordError($"Invalid filter type: {filterTypes[i]}");
                }

                if (string.IsNullOrEmpty(filterTypes[i]) || filterTypes[i] == "-")
                {
                    filters.Add(null);
                }

                else
                {
                    filters.Add(new ValueCondition
                    {
                        ValueSource = new ParameterValueSource() { ParameterName = parameterNames[i] },
                        ReferenceValue = values[i],
                        ComparisonType = filterTypeEnum
                    });
                }
            }
            return filters;
        }

    }
}
