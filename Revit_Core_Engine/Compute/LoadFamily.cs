/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Loads the Revit family of given category and name into a given document.")]
        [Input("familyLoadSettings", "Family load settings object containing the family library to be searched.")]
        [Input("document", "Revit document to load the family into.")]
        [Input("categoryName", "Category, to which the sought family belongs.")]
        [Input("familyName", "Name of the sought family.")]
        [Output("family","Family loaded to the Revit document based on the input criteria and settings.")]
        public static Family LoadFamily(this FamilyLoadSettings familyLoadSettings, Document document, string categoryName, string familyName)
        {
            if (familyLoadSettings?.FamilyLibrary == null || document == null)
                return null;

            IEnumerable<string> paths = familyLoadSettings.FamilyLibrary.Paths(categoryName, familyName, null);

            string path = paths?.FirstOrDefault();
            if (path == null)
                return null;

            Family family;
            document.LoadFamily(path, new FamilyLoadOptions(familyLoadSettings), out family);

            return family;
        }

        /***************************************************/
    }
}



