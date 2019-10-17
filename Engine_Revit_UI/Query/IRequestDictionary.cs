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

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit.Enums;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<IRequest>> IRequestDictionary(this IRequest request, UIDocument uIDocument)
        {
            if (uIDocument == null || request == null)
                return null;

            Dictionary<ElementId, List<IRequest>> aResult = new Dictionary<ElementId, List<IRequest>>();

            IEnumerable<IRequest> aRequests = BH.Engine.Adapters.Revit.Query.IRequests(request);
            if (aRequests != null && aRequests.Count() > 0)
            {
                RequestType aQueryType = RequestType.Undefined;

                if (request is FilterRequest)
                    aQueryType = BH.Engine.Adapters.Revit.Query.RequestType((FilterRequest)request);

                Dictionary<ElementId, List<IRequest>> aRequestDictionary = null;
                foreach (IRequest aRequest in aRequests)
                {
                    Dictionary<ElementId, List<IRequest>> aFilterRequestDictionary_Temp = IRequestDictionary(aRequest, uIDocument);
                    if (aRequestDictionary == null)
                    {
                        aRequestDictionary = aFilterRequestDictionary_Temp;
                    }
                    else if(aRequest is FilterRequest)
                    {
                        if (aQueryType == RequestType.LogicalAnd)
                            aRequestDictionary = Query.LogicalAnd(aRequestDictionary, aFilterRequestDictionary_Temp);
                        else
                            aRequestDictionary = Query.LogicalOr(aRequestDictionary, aFilterRequestDictionary_Temp);
                    }
                }
                aResult = aRequestDictionary;
            }
            else
            {
                IEnumerable<ElementId> aElementIds = ElementIds(request as dynamic, uIDocument);
                if (aElementIds != null)
                {
                    foreach(ElementId aElementId in aElementIds)
                    {
                        List<IRequest> aRequestList = null;
                        if (!aResult.TryGetValue(aElementId, out aRequestList))
                        {
                            aRequestList = new List<IRequest>();
                            aResult.Add(aElementId, aRequestList);
                        }
                        aRequestList.Add(request);
                    }
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}