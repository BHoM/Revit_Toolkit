/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
 
using BH.oM.Adapters.Revit.Requests;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Groups and sorts IRequests by their estimated execution time in order to execute fastest first. Order from slowest to fastest: IParameterRequest, IlogicalRequests, others. ")]
        [Input("requests", "A collection of IRequests to be sorted.")]
        [Output("sortedRequests")]
        public static List<IRequest> SortByPerformance(this List<IRequest> requests)
        {
            List<IRequest> allRequests = new List<IRequest>();
            List<IRequest> logicalRequests = new List<IRequest>();
            List<IRequest> eachElementRequests = new List<IRequest>();
            List<IRequest> parameterRequests = new List<IRequest>();

            foreach (IRequest request in requests)
            {
                if (request is IParameterRequest)
                    parameterRequests.Add(request);
                else if (request is ILogicalRequest)
                    logicalRequests.Add(request);
                else if (request is FilterByUsage || request is FilterModelElements || request is FilterByScopeBox)
                    eachElementRequests.Add(request);
                else
                    allRequests.Add(request);
            }

            allRequests.AddRange(logicalRequests);
            allRequests.AddRange(eachElementRequests);
            allRequests.AddRange(parameterRequests);

            return allRequests;
        }

        /***************************************************/
    }
}
