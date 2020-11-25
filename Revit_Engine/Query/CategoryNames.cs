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

using BH.oM.Adapters.Revit;
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

        [ToBeRemoved("4.0", "The method does not bring any added value, only contaminates the code base.")]
        [Description("Gets all Revit category names from FamilyLibrary for given family name and family type name.")]
        [Input("familyLibrary", "FamilyLibrary to be queried.")]
        [Input("familyName", "Family name to be sought for.")]
        [Input("familyTypeName", "Family type name to be sought for.")]
        [Output("categoryNames")]
        public static List<string> CategoryNames(this FamilyLibrary familyLibrary, string familyName, string familyTypeName = null)
        {
            if (familyLibrary?.Files == null)
                return null;

            IEnumerable<RevitFilePreview> files = familyLibrary.Files.Where(x => x.FamilyName == familyName);
            if (!string.IsNullOrWhiteSpace(familyTypeName))
                files = files.Where(x => x.FamilyTypeNames.Any(y => y == familyTypeName));

            return files.Select(x => x.CategoryName).Distinct().ToList();
        }

        /***************************************************/
    }
}


