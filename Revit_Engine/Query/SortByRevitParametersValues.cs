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

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sorts a collection of BHoM objects by multiple Revit parameter values in the given order.")]
        [Input("bHoMObjects", "The collection of BHoM objects to sort.")]
        [Input("parameterNames", "A list of Revit parameter names used for sorting.")]
        [Output("A sorted list of BHoM objects based on the given Revit parameters.")]
        public static List<IBHoMObject> SortByRevitParametersValues(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<string> parameterNames)
        {
            if (bHoMObjects == null)
            {
                Base.Compute.RecordError("bHoMObjects cannot be null.");
                return null;
            }
                
            if (parameterNames == null || !parameterNames.Any())
                return bHoMObjects.ToList();

            IOrderedEnumerable<IBHoMObject> sortedObjects = null;

            foreach (var parameterName in parameterNames)
            {
                sortedObjects = sortedObjects == null
                    ? bHoMObjects.OrderBy(obj => GetComparableValue(obj, parameterName))
                    : sortedObjects.ThenBy(obj => GetComparableValue(obj, parameterName));
            }

            return sortedObjects?.ToList() ?? bHoMObjects.ToList();
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static object GetComparableValue(IBHoMObject obj, string parameterName)
        {
            object value = obj.GetRevitParameterValue(parameterName);

            if (value == null)
                return null;

            return value is IComparable ? value : value.ToString();
        }
    }
}








