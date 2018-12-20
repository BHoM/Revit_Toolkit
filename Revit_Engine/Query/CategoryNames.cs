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

using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets all Revit Category Names from FamilyLibrary for given Family name and Family Type name.")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Input("familyName", "Family name")]
        [Input("familyTypeName", "Family Type name")]
        [Output("CategoryNames")]
        public static List<string> CategoryNames(this FamilyLibrary familyLibrary, string familyName, string familyTypeName = null)
        {
            if (familyLibrary == null)
                return null;

            List<string> aCategoryNameList = new List<string>();

            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> aKeyValuePair_Category in familyLibrary.Dictionary)
            {
                Dictionary<string, Dictionary<string, string>> aDictionary_Type = aKeyValuePair_Category.Value;

                List<Dictionary<string, string>> aDictionaryList_Family = new List<Dictionary<string, string>>();
                if(string.IsNullOrEmpty(familyTypeName))
                {
                    foreach (KeyValuePair<string, Dictionary<string, string>> aKeyValuePair_Type in aDictionary_Type)
                        aDictionaryList_Family.Add(aKeyValuePair_Type.Value);
                }
                else if(aKeyValuePair_Category.Value.ContainsKey(familyTypeName))
                {
                    aDictionaryList_Family.Add(aDictionary_Type[familyTypeName]);
                }

                if(string.IsNullOrEmpty(familyName))
                {
                    if (aDictionaryList_Family.Count > 0)
                        aCategoryNameList.Add(aKeyValuePair_Category.Key);
                }
                else
                {
                    foreach(Dictionary<string, string> aDictionary_Family in aDictionaryList_Family)
                        if(aDictionary_Family.ContainsKey(familyName))
                        {
                            aCategoryNameList.Add(aKeyValuePair_Category.Key);
                            break;
                        }
                }
            }

            return aCategoryNameList;
        }

        /***************************************************/
    }
}

