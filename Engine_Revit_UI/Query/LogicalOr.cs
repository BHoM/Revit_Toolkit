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

using BH.oM.DataManipulation.Queries;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterQuery>> LogicalOr(this Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary_1, Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary_2)
        {
            if (filterQueryDictionary_1 == null || filterQueryDictionary_2 == null)
                return null;

            if (filterQueryDictionary_1.Count == 0)
                return new Dictionary<ElementId, List<FilterQuery>>(filterQueryDictionary_2);

            if (filterQueryDictionary_2.Count == 0)
                return new Dictionary<ElementId, List<FilterQuery>>(filterQueryDictionary_1);


            Dictionary<ElementId, List<FilterQuery>> aResult = new Dictionary<ElementId, List<FilterQuery>>();
            foreach(KeyValuePair<ElementId, List<FilterQuery>> aKeyValuePair in filterQueryDictionary_1)
            {
                List<FilterQuery> aFilterQueryList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aFilterQueryList))
                {
                    aFilterQueryList = new List<FilterQuery>();
                    aResult.Add(aKeyValuePair.Key, aFilterQueryList);
                }

                foreach(FilterQuery aFilterQuery in aKeyValuePair.Value)
                    if (!aFilterQueryList.Contains(aFilterQuery))
                        aFilterQueryList.Add(aFilterQuery);
            }

            foreach (KeyValuePair<ElementId, List<FilterQuery>> aKeyValuePair in filterQueryDictionary_2)
            {
                List<FilterQuery> aFilterQueryList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aFilterQueryList))
                {
                    aFilterQueryList = new List<FilterQuery>();
                    aResult.Add(aKeyValuePair.Key, aFilterQueryList);
                }

                foreach (FilterQuery aFilterQuery in aKeyValuePair.Value)
                    if (!aFilterQueryList.Contains(aFilterQuery))
                        aFilterQueryList.Add(aFilterQuery);
            }

            return aResult;
        }

        /***************************************************/

    }
}