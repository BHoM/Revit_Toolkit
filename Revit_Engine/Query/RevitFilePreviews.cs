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

        [Description("Returns all RevitFilePreviews for FamilyLibrary of given Category, Family and Family Type Name")]
        [Input("familyLibrary", "FamilyLibrary")]
        [Input("categoryName", "Family Category Name")]
        [Input("familyName", "Family Name")]
        [Input("familyTypeName", "FamilyTypeName")]
        [Output("RevitFilePreviews")]
        public static List<RevitFilePreview> RevitFilePreviews(this FamilyLibrary familyLibrary, string categoryName = null, string familyName = null, string familyTypeName = null)
        {
            if (familyLibrary == null || familyLibrary.Dictionary == null || familyLibrary.Dictionary.Keys.Count == 0)
                return null;

            IEnumerable<string> aPaths = familyLibrary.Paths(categoryName, familyName, familyTypeName);
            if (aPaths == null)
                return null;

            List<RevitFilePreview> aResult = new List<RevitFilePreview>();
            if (aPaths.Count() == 0)
                return aResult;

            foreach (string aPath in aPaths)
                aResult.Add(Create.RevitFilePreview(aPath));


            return aResult;
        }

        /***************************************************/
    }
}
