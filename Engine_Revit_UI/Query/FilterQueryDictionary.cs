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

        public static Dictionary<ElementId, List<FilterRequest>> FilterRequestDictionary(this FilterRequest filterQuery, UIDocument uIDocument)
        {
            if (uIDocument == null || filterQuery == null)
                return null;

            Dictionary<ElementId, List<FilterRequest>> aResult = new Dictionary<ElementId, List<FilterRequest>>();

            IEnumerable<FilterRequest> aFilterQueries = BH.Engine.Adapters.Revit.Query.FilterQueries(filterQuery);
            if (aFilterQueries != null && aFilterQueries.Count() > 0)
            {
                QueryType aQueryType = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery);

                Dictionary<ElementId, List<FilterRequest>> aFilterRequestDictionary = null;
                foreach (FilterRequest aFilterRequest in aFilterQueries)
                {
                    Dictionary<ElementId, List<FilterRequest>> aFilterRequestDictionary_Temp = FilterRequestDictionary(aFilterRequest, uIDocument);
                    if (aFilterRequestDictionary == null)
                    {
                        aFilterRequestDictionary = aFilterRequestDictionary_Temp;
                    }
                    else
                    {
                        if (aQueryType == QueryType.LogicalAnd)
                            aFilterRequestDictionary = Query.LogicalAnd(aFilterRequestDictionary, aFilterRequestDictionary_Temp);
                        else
                            aFilterRequestDictionary = Query.LogicalOr(aFilterRequestDictionary, aFilterRequestDictionary_Temp);
                    }
                }
                aResult = aFilterRequestDictionary;
            }
            else
            {
                IEnumerable<ElementId> aElementIds = ElementIds(filterQuery, uIDocument);
                if (aElementIds != null)
                {
                    foreach(ElementId aElementId in aElementIds)
                    {
                        List<FilterRequest> aFilterRequestList = null;
                        if (!aResult.TryGetValue(aElementId, out aFilterRequestList))
                        {
                            aFilterRequestList = new List<FilterRequest>();
                            aResult.Add(aElementId, aFilterRequestList);
                        }
                        aFilterRequestList.Add(filterQuery);
                    }
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}