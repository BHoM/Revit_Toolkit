/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
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

        [Description("Returns all RevitFilePreviews inside FamilyLibrary that meet Revit category, family and family type name requirements.")]
        [Input("familyLibrary", "FamilyLibrary to be queried.")]
        [Input("categoryName", "Name of Revit category to be sought for. Optional: if null, all items to be taken.")]
        [Input("familyName", "Name of Revit family to be sought for. Optional: if null, all items to be taken.")]
        [Input("familyTypeName", "Name of Revit family type to be sought for. Optional: if null, all items to be taken.")]
        [Output("revitFilePreviews")]
        public static List<RevitFilePreview> RevitFilePreviews(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null, string familyTypeName = null)
        {
            IEnumerable<RevitFilePreview> files = familyLibrary?.Files;
            if (files == null)
                return null;

            if (!string.IsNullOrWhiteSpace(categoryName))
                files = files.Where(x => x.CategoryName == categoryName);

            if (!string.IsNullOrWhiteSpace(familyName))
                files = files.Where(x => x.FamilyName == familyName);

            if (!string.IsNullOrWhiteSpace(familyTypeName))
                files = files.Where(x => x.FamilyTypeNames.Any(y => y == familyTypeName));

            return files.ToList();
        }

        /***************************************************/
    }
}




