/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Requests;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates IRequest that filters Revit family types by names of theirs and their parent family, with option to loose the search by leaving one or both of the input names blank.")]
        [InputFromProperty("familyName")]
        [InputFromProperty("familyTypeName")]
        [InputFromProperty("caseSensitive")]
        [Output("request", "Created request.")]
        public static FilterFamilyTypeByName FilterFamilyTypeByName(string familyName, string familyTypeName, bool caseSensitive)
        {
            return new FilterFamilyTypeByName { FamilyName = familyName, FamilyTypeName = familyTypeName, CaseSensitive = caseSensitive };
        }

        /***************************************************/
    }
}

