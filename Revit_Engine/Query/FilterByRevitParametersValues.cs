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

using BH.oM.Adapters.Revit;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters a collection of BHoM objects based on specified Revit parameter names and their corresponding values.")]
        [Input("bHoMObjects", "Collection of BHoM objects to filter.")]
        [Input("parameterNames", "Collection of Revit parameter names to filter by.")]
        [Input("parameterValues", "Collection of values corresponding to the Revit parameter names.")]
        [Output("Filtered list of BHoM objects that match the specified parameter names and values.")]
        public static List<IBHoMObject> FilterByRevitParametersValues(this IEnumerable<IBHoMObject> bHoMObjects, IEnumerable<string> parameterNames, IEnumerable<object> parameterValues)
        {
            Dictionary<string, object> pNameValuePairs = parameterNames
                .Zip(parameterValues, (name, value) => new { name, value })
                .ToDictionary(x => x.name, x => x.value);

            return bHoMObjects.Where(bHoMObject
                => pNameValuePairs.All(p => bHoMObject.GetRevitParameterValue(p.Key) == p.Value)
            ).ToList();
        }

        /***************************************************/
    }
}








