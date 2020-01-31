/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
        /****              Public methods               ****/
        /***************************************************/

        //public static Dictionary<ElementId, List<IRequest>> RequestDictionary(this IRequest request, UIDocument uIDocument)
        //{
        //    if (uIDocument == null || request == null)
        //        return null;

        //    Dictionary<ElementId, List<IRequest>> result = new Dictionary<ElementId, List<IRequest>>();
        //    if (request is ILogicalRequest)
        //    {
        //        Dictionary<ElementId, List<IRequest>> requestDictionary = null;
        //        foreach (IRequest logicalRequest in (request as ILogicalRequest).Requests)
        //        {
        //            Dictionary<ElementId, List<IRequest>> tempDictionary = RequestDictionary(logicalRequest, uIDocument);
        //            if (requestDictionary == null)
        //                requestDictionary = tempDictionary;
        //            else
        //            {
        //                if (logicalRequest is LogicalAndRequest)
        //                    requestDictionary = Query.LogicalAnd(requestDictionary, tempDictionary);
        //                else if (logicalRequest is LogicalOrRequest)
        //                    requestDictionary = Query.LogicalOr(requestDictionary, tempDictionary);
        //            }
        //        }
        //        result = requestDictionary;
        //    }
        //    else
        //    {
        //        IEnumerable<ElementId> elementIDs = ElementIds(request, uIDocument);
        //        if (elementIDs != null)
        //        {
        //            foreach (ElementId id in elementIDs)
        //            {
        //                List<IRequest> requestList = null;
        //                if (!result.TryGetValue(id, out requestList))
        //                {
        //                    requestList = new List<IRequest>();
        //                    result.Add(id, requestList);
        //                }
        //                requestList.Add(request);
        //            }
        //        }
        //    }

        //    return result;
        //}


        /***************************************************/
        /****            Deprecated methods             ****/
        /***************************************************/

        //public static Dictionary<ElementId, List<FilterRequest>> FilterRequestDictionary(this FilterRequest filterRequest, UIDocument uIDocument)
        //{
        //    if (uIDocument == null || filterRequest == null)
        //        return null;

        //    Dictionary<ElementId, List<FilterRequest>> result = new Dictionary<ElementId, List<FilterRequest>>();

        //    IEnumerable<FilterRequest> requests = BH.Engine.Adapters.Revit.Query.FilterRequests(filterRequest);
        //    if (requests != null && requests.Count() > 0)
        //    {
        //        RequestType queryType = BH.Engine.Adapters.Revit.Query.RequestType(filterRequest);

        //        Dictionary<ElementId, List<FilterRequest>> filterRequestDictionary = null;
        //        foreach (FilterRequest request in requests)
        //        {
        //            Dictionary<ElementId, List<FilterRequest>> tempDictionary = FilterRequestDictionary(request, uIDocument);
        //            if (filterRequestDictionary == null)
        //            {
        //                filterRequestDictionary = tempDictionary;
        //            }
        //            else
        //            {
        //                if (queryType == RequestType.LogicalAnd)
        //                    filterRequestDictionary = Query.LogicalAnd(filterRequestDictionary, tempDictionary);
        //                else
        //                    filterRequestDictionary = Query.LogicalOr(filterRequestDictionary, tempDictionary);
        //            }
        //        }
        //        result = filterRequestDictionary;
        //    }
        //    else
        //    {
        //        IEnumerable<ElementId> elementIDs = ElementIds(filterRequest, uIDocument);
        //        if (elementIDs != null)
        //        {
        //            foreach (ElementId id in elementIDs)
        //            {
        //                List<FilterRequest> requestList = null;
        //                if (!result.TryGetValue(id, out requestList))
        //                {
        //                    requestList = new List<FilterRequest>();
        //                    result.Add(id, requestList);
        //                }
        //                requestList.Add(filterRequest);
        //            }
        //        }
        //    }

        //    return result;
        //}

        /***************************************************/
    }
}