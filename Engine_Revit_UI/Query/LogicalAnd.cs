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

using BH.oM.Data.Requests;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<IRequest>> LogicalAnd(this Dictionary<ElementId, List<IRequest>> requestDictionary_1, Dictionary<ElementId, List<IRequest>> requestDictionary_2)
        {
            if (requestDictionary_1 == null || requestDictionary_2 == null)
                return null;

            Dictionary<ElementId, List<IRequest>> aResult = new Dictionary<ElementId, List<IRequest>>();

            if (requestDictionary_1.Count() == 0 || requestDictionary_2.Count() == 0)
                return aResult;

            foreach(KeyValuePair<ElementId, List<IRequest>> aKeyValuePair in requestDictionary_1)
            {
                if (aKeyValuePair.Value == null || aKeyValuePair.Value.Count == 0)
                    continue;

                List<IRequest> aRequestList = null;
                if (requestDictionary_2.TryGetValue(aKeyValuePair.Key, out aRequestList))
                {
                    if (aRequestList == null || aRequestList.Count == 0)
                        continue;

                    List<IRequest> aRequestList_Temp = new List<IRequest>(aKeyValuePair.Value);
                    aRequestList_Temp.AddRange(aRequestList);
                    aResult.Add(aKeyValuePair.Key, aRequestList_Temp);
                }
            }
            return aResult;
        }

        /***************************************************/

    }
}