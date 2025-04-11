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
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //TODO: move to Verification_Engine
        public static IEnumerable<IBHoMObject> FilterByCondition(this IEnumerable<IBHoMObject> bHoMObjects, ICondition condition)
        {
            return bHoMObjects.Where(x => x?.IPasses(condition) == true);
        }


        //[Description("Filters a collection of BHoM objects by a collection of RevitParameter criterion.")]
        //[Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        //[Input("criteria", "The filtering criteria to apply.")]
        //[Output("A list of BHoM objects that match the specified criteria.")]
        //public static IEnumerable<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, ILogicalCollectionCondition criteria)
        //{

        //    if (criteria == null) return bHoMObjects;

        //    bHoMObjects = bHoMObjects.Where(item => {

        //        var typeId = item.GetRevitElementType().ElementId();
        //        var isType = typeId == -1;

        //        var validateCrit = criteria.Conditions.Cast<ValueCondition>();

        //        validateCrit = validateCrit.Select(c =>
        //        {
        //            ParameterValueSource z = (ParameterValueSource)c.ValueSource;
        //            z.FromType = isType;
        //            c.ValueSource = z;
        //            return c;
        //        });

        //        criteria.Conditions = validateCrit.Cast<ICondition>().ToList();

        //        return item.VerifyCondition(criteria).Passed != null && item.VerifyCondition(criteria).Passed.Value;
        //    });

        //    return bHoMObjects.ToList();
        //}

        //[Description("Filters a collection of BHoM objects by multiple Revit parameter criteria.")]
        //[Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        //[Input("criteria", "A compact list of filtering criteria: parameterNames, filterTypes, values")]
        //[Output("A list of BHoM objects that match the specified criteria.")]
        //public static IEnumerable<IBHoMObject> FilterByRevitParameters(this IEnumerable<IBHoMObject> bHoMObjects, List<List<object>> criteria)
        //{
        //    // Validate that criteria contains exactly 3 lists
        //    if (criteria == null)
        //    {
        //        Base.Compute.RecordError("Criteria must contain exactly three rows: parameterNames, filterTypes, values.");
        //        return null;
        //    }

        //    List<string> parameterNames = criteria.Select(x => x[0]).Cast<string>().ToList();
        //    List<string> filterTypes = criteria.Select(x => x[1]).Cast<string>().ToList();
        //    List<object> values = criteria.Select(x => x[2]).ToList();


        //    var filterCriteria = FiltersFromString(parameterNames, filterTypes, values);

        //    return bHoMObjects.FilterByRevitParameters(filterCriteria);
        //}

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

            var filterCriteria = FiltersFromString(
                parameterNames.ToList(),
                comparisonTypes.ToList(),
                referenceValues.ToList()
            );

            return bHoMObjects.FilterByCondition(filterCriteria);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static ILogicalCollectionCondition FiltersFromString(List<string> parameterNames, List<ValueComparisonType> comparisonTypes, List<object> values)
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
    }
}
