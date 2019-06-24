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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Data.Requests;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterRequest>> LogicalOr(this Dictionary<ElementId, List<FilterRequest>> filterQueryDictionary_1, Dictionary<ElementId, List<FilterRequest>> filterQueryDictionary_2)
        {
            if (filterQueryDictionary_1 == null || filterQueryDictionary_2 == null)
                return null;

            if (filterQueryDictionary_1.Count == 0)
                return new Dictionary<ElementId, List<FilterRequest>>(filterQueryDictionary_2);

            if (filterQueryDictionary_2.Count == 0)
                return new Dictionary<ElementId, List<FilterRequest>>(filterQueryDictionary_1);


            Dictionary<ElementId, List<FilterRequest>> aResult = new Dictionary<ElementId, List<FilterRequest>>();
            foreach(KeyValuePair<ElementId, List<FilterRequest>> aKeyValuePair in filterQueryDictionary_1)
            {
                List<FilterRequest> aFilterRequestList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aFilterRequestList))
                {
                    aFilterRequestList = new List<FilterRequest>();
                    aResult.Add(aKeyValuePair.Key, aFilterRequestList);
                }

                foreach(FilterRequest aFilterRequest in aKeyValuePair.Value)
                    if (!aFilterRequestList.Contains(aFilterRequest))
                        aFilterRequestList.Add(aFilterRequest);
            }

            foreach (KeyValuePair<ElementId, List<FilterRequest>> aKeyValuePair in filterQueryDictionary_2)
            {
                List<FilterRequest> aFilterRequestList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aFilterRequestList))
                {
                    aFilterRequestList = new List<FilterRequest>();
                    aResult.Add(aKeyValuePair.Key, aFilterRequestList);
                }

                foreach (FilterRequest aFilterRequest in aKeyValuePair.Value)
                    if (!aFilterRequestList.Contains(aFilterRequest))
                        aFilterRequestList.Add(aFilterRequest);
            }

            return aResult;
        }

        /***************************************************/

    }
}