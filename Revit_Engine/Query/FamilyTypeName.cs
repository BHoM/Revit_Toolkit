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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the name of Revit family type correspondent to given BHoMObject. This value is stored in CustomData under key Revit_familyTypeName.")]
        [Input("bHoMObject", "BHoMObject to be queried.")]
        [Output("familyTypeName")]
        public static string FamilyTypeName(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object value = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.FamilyTypeName, out value))
            {
                if (value == null)
                    return null;

                return value.ToString();
            }

            return null;
        }

        /***************************************************/

        [Description("Gets Revit family type name from family type full name in format FamilyName: FamilyTypeName.")]
        [Input("familyTypeFullName", "Family type full name to be queried.")]
        [Output("familyTypeName")]
        public static string FamilyTypeName(this string familyTypeFullName)
        {
            if (string.IsNullOrWhiteSpace(familyTypeFullName))
                return null;

            int index = familyTypeFullName.IndexOf(":");
            if (index <= 0)
                return null;

            string result = familyTypeFullName.Substring(index+1);
            return result.Trim();
        }

        /***************************************************/
    }
}

