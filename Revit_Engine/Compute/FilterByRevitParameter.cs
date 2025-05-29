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

using BH.Engine.Verification;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Verification;
using BH.oM.Verification.Conditions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters a collection of BHoM objects by a single Revit parameter criterion.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("parameterName", "The name of the parameter to filter by.")]
        [Input("filterType", "The type of filter to apply.")]
        [Input("value", "The value to filter by.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameter(this IEnumerable<IBHoMObject> bHoMObjects, string parameterName, ValueComparisonType comparisonType, object value)
        {
            return FilterByRevitParameters(bHoMObjects,
                new List<string>() { parameterName },
                new List<ValueComparisonType>() { comparisonType },
                new List<object>() { value });
        }

        /***************************************************/

        [Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("parameterNames", "A list of Revit parameter names to filter by.")]
        [Input("filterTypes", "The types of filtering to apply.")]
        [Input("values", "The values to filter the parameters against.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<string> parameterNames, IEnumerable<ValueComparisonType> comparisonTypes, IEnumerable<object> referenceValues)
        {
            if (parameterNames == null || comparisonTypes == null || referenceValues == null)
            {
                Base.Compute.RecordError("One or more arguments are null.");
                return null;
            }

            ILogicalCollectionCondition filterCriteria = CombineFilters(
                parameterNames.ToList(),
                comparisonTypes.ToList(),
                referenceValues.ToList()
            );

            return bHoMObjects.FilterByCondition(filterCriteria);
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static ILogicalCollectionCondition CombineFilters(List<string> parameterNames, List<ValueComparisonType> comparisonTypes, List<object> values)
        {
            if (parameterNames == null || comparisonTypes == null || values == null)
            {
                Base.Compute.RecordError("One or more arguments are null.");
                return null;
            }

            if (parameterNames.Count != comparisonTypes.Count || parameterNames.Count != values.Count)
            {
                Base.Compute.RecordError("All lists (parameterNames, filterTypes, values) must have the same number of elements.");
                return null;
            }

            LogicalAndCondition filters = new LogicalAndCondition();

            for (int i = 0; i < parameterNames.Count; i++)
            {
                filters.Conditions.Add(new ValueCondition
                {
                    ValueSource = new ParameterValueSource() { ParameterName = parameterNames[i] },
                    ReferenceValue = values[i],
                    ComparisonType = comparisonTypes[i]
                });
            }

            return filters;
        }

        /***************************************************/
    }
}
