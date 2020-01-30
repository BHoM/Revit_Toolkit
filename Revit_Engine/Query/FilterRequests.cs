/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.oM.Data.Requests;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets child FilterRequests for given FiletrRequest (FilterRequest can be combined by logical operation - LogicalAndSelectionFilterRequest, LogicalOrSelectionFilterRequest).")]
        [Input("filterRequest", "FilterRequest")]
        [Output("FilterRequests")]
        public static IEnumerable<FilterRequest> FilterRequests(this FilterRequest filterRequest)
        {
            if (filterRequest == null)
                return null;

            if (!filterRequest.Equalities.ContainsKey(Convert.FilterRequest.FilterRequests))
                return null;

            if (filterRequest.Equalities[Convert.FilterRequest.FilterRequests] is IEnumerable<FilterRequest>)
                return (IEnumerable<FilterRequest>)filterRequest.Equalities[Convert.FilterRequest.FilterRequests];

            if (filterRequest.Equalities[Convert.FilterRequest.FilterRequests] is IEnumerable<object>)
            {
                IEnumerable<object> objs = filterRequest.Equalities[Convert.FilterRequest.FilterRequests] as IEnumerable<object>;
                if (objs != null)
                    return objs.Cast<FilterRequest>();
            }

            return null;
        }

        /***************************************************/
    }
}

