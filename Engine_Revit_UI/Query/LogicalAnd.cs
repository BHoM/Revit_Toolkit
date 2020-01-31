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

//using System.Linq;
//using System.Collections.Generic;

//using Autodesk.Revit.DB;

//using BH.oM.Data.Requests;


//namespace BH.UI.Revit.Engine
//{
//    public static partial class Query
//    {
//        /***************************************************/
//        /****              Public methods               ****/
//        /***************************************************/

//        public static HashSet<ElementId> LogicalAnd(this HashSet<ElementId> set1, HashSet<ElementId> set2)
//        {
//            if (set1 == null || set2 == null)
//                return null;
//            else
//            {
//                HashSet<ElementId> result = new HashSet<ElementId>(set1);
//                result.IntersectWith(set2);
//                return result;
//            }
//        }


//        /***************************************************/
//        /****            Deprecated methods             ****/
//        /***************************************************/

//        //public static Dictionary<ElementId, List<FilterRequest>> LogicalAnd(this Dictionary<ElementId, List<FilterRequest>> filterRequestDictionary1, Dictionary<ElementId, List<FilterRequest>> filterRequestDictionary2)
//        //{
//        //    if (filterRequestDictionary1 == null || filterRequestDictionary2 == null)
//        //        return null;

//        //    Dictionary<ElementId, List<FilterRequest>> result = new Dictionary<ElementId, List<FilterRequest>>();

//        //    if (filterRequestDictionary1.Count() == 0 || filterRequestDictionary2.Count() == 0)
//        //        return result;

//        //    foreach (KeyValuePair<ElementId, List<FilterRequest>> kvp in filterRequestDictionary1)
//        //    {
//        //        if (kvp.Value == null || kvp.Value.Count == 0)
//        //            continue;

//        //        List<FilterRequest> requests = null;
//        //        if (filterRequestDictionary2.TryGetValue(kvp.Key, out requests))
//        //        {
//        //            if (requests == null || requests.Count == 0)
//        //                continue;

//        //            List<FilterRequest> requestsTemp = new List<FilterRequest>(kvp.Value);
//        //            requestsTemp.AddRange(requests);
//        //            result.Add(kvp.Key, requestsTemp);
//        //        }
//        //    }

//        //    return result;
//        //}

//        /***************************************************/
//    }
//}
