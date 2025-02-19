﻿/*
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
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.Verification.Conditions;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        [Description("Filters a collection of BHoM objects by a single Revit parameter criterion.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("criterion", "The filtering criterion to apply.")]
        [Output("A list of BHoM objects that match the specified criterion.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameter(this IEnumerable<IBHoMObject> bHoMObjects, ValueCondition criterion)
        {
            return bHoMObjects.FilterByRevitParameters(new List<ValueCondition> { criterion });
        }


        [Description("Filters a collection of BHoM objects by a single Revit parameter criterion.")]
        [Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        [Input("parameterName", "The name of the parameter to filter by.")]
        [Input("filterType", "The type of filter to apply.")]
        [Input("value", "The value to filter by.")]
        [Output("A list of BHoM objects that match the specified criteria.")]
        public static IEnumerable<IBHoMObject> FilterByRevitParameter(this IEnumerable<IBHoMObject> bHoMObjects, string parameterName, object filterType, object value)
        {

            return FilterByRevitParameters(bHoMObjects, 
                new List<string> () { parameterName}, 
                new List<string>() {(string)filterType}, 
                new List<object>() { value});
        }

    }
}