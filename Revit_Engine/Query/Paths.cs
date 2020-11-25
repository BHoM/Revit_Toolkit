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

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns all file paths in FamilyLibrary that meet given Revit category, family and family type requirements.")]
        [Input("familyLibrary", "FamilyLibrary to be queried.")]
        [Input("categoryName", "Name of Revit category to be sought for. Optional: if null, all items to be taken.")]
        [Input("familyName", "Name of Revit family to be sought for. Optional: if null, all items to be taken.")]
        [Input("typeName", "Name of Revit family type to be sought for. Optional: if null, all items to be taken.")]
        [Output("paths")]
        public static IEnumerable<string> Paths(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null, string typeName = null)
        {
            if (familyLibrary == null || familyLibrary.Dictionary == null || familyLibrary.Dictionary.Keys.Count == 0)
                return null;

            List<string> pathList = new List<string>();

            List<Dictionary<string, Dictionary<string, string>>> categoryDictionary = new List<Dictionary<string, Dictionary<string, string>>>();

            if (string.IsNullOrEmpty(categoryName))
                categoryDictionary = familyLibrary.Dictionary.Values.ToList();
            else
            {
                Dictionary<string, Dictionary<string, string>> catDict = null;
                if (familyLibrary.Dictionary.TryGetValue(categoryName, out catDict))
                    categoryDictionary.Add(catDict);
            }

            List<Dictionary<string, string>> typeDictionary = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(typeName))
            {
                foreach (Dictionary<string, Dictionary<string, string>> dictionary_Category in categoryDictionary)
                    typeDictionary.AddRange(dictionary_Category.Values);
            }
            else
            {
                foreach (Dictionary<string, Dictionary<string, string>> catDict in categoryDictionary)
                {
                    Dictionary<string, string> familyDictionary = null;
                    if (catDict.TryGetValue(typeName, out familyDictionary))
                        typeDictionary.Add(familyDictionary);
                }
            }

            if (string.IsNullOrEmpty(familyName))
            {
                typeDictionary.ForEach(x => pathList.AddRange(x.Values));
            }
            else
            {
                foreach (Dictionary<string, string> familyDictionary in typeDictionary)
                {
                    string path = null;
                    if (familyDictionary.TryGetValue(familyName, out path))
                        pathList.Add(path);
                }
            }

            return pathList.Distinct();
        }

        /***************************************************/
    }
}

