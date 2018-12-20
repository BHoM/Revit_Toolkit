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

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterQuery>> FilterQueryDictionary(this FilterQuery filterQuery, UIDocument uIDocument)
        {
            if (uIDocument == null || filterQuery == null)
                return null;

            Dictionary<ElementId, List<FilterQuery>> aResult = new Dictionary<ElementId, List<FilterQuery>>();

            IEnumerable<FilterQuery> aFilterQueries = BH.Engine.Adapters.Revit.Query.FilterQueries(filterQuery);
            if (aFilterQueries != null && aFilterQueries.Count() > 0)
            {
                QueryType aQueryType = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery);

                Dictionary<ElementId, List<FilterQuery>> aFilterQueryDictionary = null;
                foreach (FilterQuery aFilterQuery in aFilterQueries)
                {
                    Dictionary<ElementId, List<FilterQuery>> aFilterQueryDictionary_Temp = FilterQueryDictionary(aFilterQuery, uIDocument);
                    if (aFilterQueryDictionary == null)
                    {
                        aFilterQueryDictionary = aFilterQueryDictionary_Temp;
                    }
                    else
                    {
                        if (aQueryType == QueryType.LogicalAnd)
                            aFilterQueryDictionary = Query.LogicalAnd(aFilterQueryDictionary, aFilterQueryDictionary_Temp);
                        else
                            aFilterQueryDictionary = Query.LogicalOr(aFilterQueryDictionary, aFilterQueryDictionary_Temp);
                    }
                }
                aResult = aFilterQueryDictionary;
            }
            else
            {
                IEnumerable<ElementId> aElementIds = ElementIds(filterQuery, uIDocument);
                if (aElementIds != null)
                {
                    foreach(ElementId aElementId in aElementIds)
                    {
                        List<FilterQuery> aFilterQueryList = null;
                        if (!aResult.TryGetValue(aElementId, out aFilterQueryList))
                        {
                            aFilterQueryList = new List<FilterQuery>();
                            aResult.Add(aElementId, aFilterQueryList);
                        }
                        aFilterQueryList.Add(filterQuery);
                    }
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}