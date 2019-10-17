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

using BH.oM.Data.Requests;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Revit.Requests;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets child FilterRequests for given FiletrRequest (FilterRequest can be combined by logical operation - LogicalAndSelectionFilterRequest, LogicalOrSelectionFilterRequest).")]
        [Input("filterRequest", "FilterRequest")]
        [Output("FilterRequests")]
        public static IEnumerable<IRequest> IRequests(this IRequest request)
        {
            if (request == null)
                return null;

            if (request is LinkRequest)
            {
                List<IRequest> aRequestList = new List<IRequest>();
                LinkRequest aLinkRequest = (LinkRequest)request;
                if (aLinkRequest.Request != null)
                    aRequestList.Add(aLinkRequest.Request);
                return aRequestList;
            }
            else if(request is FilterRequest)
            {
                FilterRequest aFilterRequest = (FilterRequest)request;

                if (!aFilterRequest.Equalities.ContainsKey(Convert.FilterRequest.FilterRequests))
                    return null;

                if (aFilterRequest.Equalities[Convert.FilterRequest.FilterRequests] is IEnumerable<FilterRequest>)
                    return (IEnumerable<FilterRequest>)aFilterRequest.Equalities[Convert.FilterRequest.FilterRequests];

                if (aFilterRequest.Equalities[Convert.FilterRequest.FilterRequests] is IEnumerable<object>)
                {
                    IEnumerable<object> aObjects = aFilterRequest.Equalities[Convert.FilterRequest.FilterRequests] as IEnumerable<object>;
                    if (aObjects != null)
                        return aObjects.Cast<FilterRequest>();
                }
            }

            return null;
        }

        /***************************************************/
    }
}
