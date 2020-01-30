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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Data.Requests;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterRequest>> LogicalOr(this Dictionary<ElementId, List<FilterRequest>> filterRequestDictionary1, Dictionary<ElementId, List<FilterRequest>> filterRequestDictionary2)
        {
            if (filterRequestDictionary1 == null || filterRequestDictionary2 == null)
                return null;

            if (filterRequestDictionary1.Count == 0)
                return new Dictionary<ElementId, List<FilterRequest>>(filterRequestDictionary2);

            if (filterRequestDictionary2.Count == 0)
                return new Dictionary<ElementId, List<FilterRequest>>(filterRequestDictionary1);

            Dictionary<ElementId, List<FilterRequest>> result = new Dictionary<ElementId, List<FilterRequest>>();
            foreach(KeyValuePair<ElementId, List<FilterRequest>> kvp in filterRequestDictionary1)
            {
                List<FilterRequest> requests = null;
                if (!result.TryGetValue(kvp.Key, out requests))
                {
                    requests = new List<FilterRequest>();
                    result.Add(kvp.Key, requests);
                }

                foreach (FilterRequest request in kvp.Value)
                {
                    if (!requests.Contains(request))
                        requests.Add(request);
                }
            }

            foreach (KeyValuePair<ElementId, List<FilterRequest>> kvp in filterRequestDictionary2)
            {
                List<FilterRequest> requests = null;
                if (!result.TryGetValue(kvp.Key, out requests))
                {
                    requests = new List<FilterRequest>();
                    result.Add(kvp.Key, requests);
                }

                foreach (FilterRequest request in kvp.Value)
                    if (!requests.Contains(request))
                        requests.Add(request);
            }

            return result;
        }

        /***************************************************/
    }
}
