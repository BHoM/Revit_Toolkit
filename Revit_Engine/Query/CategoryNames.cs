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

using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets all Revit Category Names from FamilyLibrary for given Family name and Family Type name.")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Input("familyName", "Family name")]
        [Input("familyTypeName", "Family Type name")]
        [Output("CategoryNames")]
        public static List<string> CategoryNames(this FamilyLibrary familyLibrary, string familyName, string familyTypeName = null)
        {
            if (familyLibrary == null || familyLibrary.Dictionary == null)
                return null;

            List<string> categoryNames = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> kvp in familyLibrary.Dictionary)
            {
                Dictionary<string, Dictionary<string, string>> typeDictionary = kvp.Value;

                List<Dictionary<string, string>> familyDictionary = new List<Dictionary<string, string>>();
                if(string.IsNullOrEmpty(familyTypeName))
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair_Type in typeDictionary)
                        familyDictionary.Add(keyValuePair_Type.Value);
                }
                else if(kvp.Value.ContainsKey(familyTypeName))
                {
                    familyDictionary.Add(typeDictionary[familyTypeName]);
                }

                if(string.IsNullOrEmpty(familyName))
                {
                    if (familyDictionary.Count > 0)
                        categoryNames.Add(kvp.Key);
                }
                else
                {
                    foreach (Dictionary<string, string> dict in familyDictionary)
                    {
                        if (dict.ContainsKey(familyName))
                        {
                            categoryNames.Add(kvp.Key);
                            break;
                        }
                    }
                }
            }

            return categoryNames;
        }

        /***************************************************/
    }
}


