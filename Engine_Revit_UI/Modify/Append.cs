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

using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;


namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<BuiltInCategory> Append(this IEnumerable<BuiltInCategory> currentSet, IEnumerable<BuiltInCategory> newItems, bool allowDuplicates = false)
        {
            if (newItems == null && currentSet == null)
                return null;

            if (newItems == null)
                return new List<BuiltInCategory>(currentSet);

            if (currentSet == null || currentSet.Count() == 0)
                return new List<BuiltInCategory>(newItems);

            List<BuiltInCategory> builtInCategoryList = new List<BuiltInCategory>(newItems);

            foreach (BuiltInCategory builtInCategory in currentSet)
            {
                if (allowDuplicates || !builtInCategoryList.Contains(builtInCategory))
                    builtInCategoryList.Add(builtInCategory);
            }

            return builtInCategoryList;
        }

        /***************************************************/

        public static Dictionary<FilterRequest, List<Element>> Append(this Dictionary<FilterRequest, List<Element>> currentSet, Dictionary<FilterRequest, List<Element>> newItems)
        {
            if (currentSet == null && newItems == null)
                return null;

            if (currentSet == null)
                return new Dictionary<FilterRequest, List<Element>>(newItems);

            if (newItems == null || newItems.Count == 0)
                return new Dictionary<FilterRequest, List<Element>>(currentSet);

            Dictionary<FilterRequest, List<Element>> result = new Dictionary<FilterRequest, List<Element>>(currentSet);
            foreach (KeyValuePair<FilterRequest, List<Element>> kvp in newItems)
            {
                if (kvp.Value == null)
                    continue;

                List<Element> elements = null;
                if (!result.TryGetValue(kvp.Key, out elements))
                {
                    result.Add(kvp.Key, kvp.Value);
                }
                else
                {
                    foreach (Element aElement in kvp.Value)
                        if (elements.Find(x => x.Id == aElement.Id) == null)
                            elements.Add(aElement);
                }
            }

            return result;
        }


        /***************************************************/
        /**** Deprecated Methods                        ****/
        /***************************************************/

        [DeprecatedAttribute("3.0", "AppendBuiltInCategories had been renamed to Append", null, "Append")]
        public static IEnumerable<BuiltInCategory> AppendBuiltInCategories(this IEnumerable<BuiltInCategory> builtInCategories_ToBeAppended, IEnumerable<BuiltInCategory> builtInCategories, bool AllowDuplicates = false)
        {
            return Append(builtInCategories, builtInCategories_ToBeAppended, AllowDuplicates);
        }

        /***************************************************/
    }
}