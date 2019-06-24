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
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> AppendBuiltInCategories(this IEnumerable<BuiltInCategory> builtInCategories_ToBeAppended, IEnumerable<BuiltInCategory> builtInCategories, bool AllowDuplicates = false)
        {
            if (builtInCategories_ToBeAppended == null && builtInCategories == null)
                return null;

            if (builtInCategories_ToBeAppended == null)
                return new List<BuiltInCategory>(builtInCategories);

            if (builtInCategories == null || builtInCategories.Count() == 0)
                return new List<BuiltInCategory>(builtInCategories_ToBeAppended);

            List<BuiltInCategory> aBuiltInCategoryList = new List<BuiltInCategory>(builtInCategories_ToBeAppended);

            foreach (BuiltInCategory aBuiltInCategory in builtInCategories)
            {
                if (AllowDuplicates || !aBuiltInCategoryList.Contains(aBuiltInCategory))
                    aBuiltInCategoryList.Add(aBuiltInCategory);
            }

            return aBuiltInCategoryList;
        }

        /***************************************************/

        public static Dictionary<FilterRequest, List<Element>> Append(this Dictionary<FilterRequest, List<Element>> filterQueryDictionary_ToBeAppended, Dictionary<FilterRequest, List<Element>> filterQueryDictionary)
        {
            if (filterQueryDictionary_ToBeAppended == null && filterQueryDictionary == null)
                return null;

            if (filterQueryDictionary_ToBeAppended == null)
                return new Dictionary<FilterRequest, List<Element>>(filterQueryDictionary);

            if (filterQueryDictionary == null || filterQueryDictionary.Count == 0)
                return new Dictionary<FilterRequest, List<Element>>(filterQueryDictionary_ToBeAppended);

            Dictionary<FilterRequest, List<Element>> aResult = new Dictionary<FilterRequest, List<Element>>(filterQueryDictionary_ToBeAppended);
            foreach (KeyValuePair<FilterRequest, List<Element>> aKeyValuePair in filterQueryDictionary)
            {
                if (aKeyValuePair.Value == null)
                    continue;

                List<Element> aElementList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aElementList))
                {
                    aResult.Add(aKeyValuePair.Key, aKeyValuePair.Value);
                }
                else
                {
                    foreach (Element aElement in aKeyValuePair.Value)
                        if (aElementList.Find(x => x.Id == aElement.Id) == null)
                            aElementList.Add(aElement);
                }
            }

            return aResult;
        }

    }
}