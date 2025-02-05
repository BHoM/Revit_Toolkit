/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

namespace BH.oM.Adapters.Revit.Enums
{
    /***************************************************/

    [Description("Enumerator defining the way in which two strings are compared.")]
    public enum TextComparisonType
    {
        [Description("Check if input string and reference string are the same.")]
        Equal,
        [Description("Check if input string and reference string are different.")]
        NotEqual,
        [Description("Check if the input string contains the reference string.")]
        Contains,
        [Description("Check if the input string does not contain the reference string.")]
        ContainsNot,
        [Description("Check if the input string starts with the reference string.")]
        StartsWith,
        [Description("Check if the input string does not start with the reference string ")]
        NotStartsWith,
        [Description("Check if the input string ends with the reference string.")]
        EndsWith,
        [Description("Check if the input string does not end with the reference string")]
        NotEndsWith,
        [Description("Check if the input string is greater than the reference string.")]
        Greater,
        [Description("Check if the input string is greater or equal to the reference string")]
        GreaterOrEqual,
        [Description("Check if the input string is smaller to the reference string")]
        Less,
        [Description("Check if the input string is smaller or equal to the reference string")]
        LessOrEqual
    }

    /***************************************************/
}





