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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;


namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static string CategoryName(this BuiltInCategory builtInCategory, Document document)
        {
            if (document == null || document.Settings == null || document.Settings.Categories == null)
                return null;

            foreach (Category category in document.Settings.Categories)
                if (category.Id.IntegerValue == (int)builtInCategory)
                    return category.Name;

            return null;
        }

        /***************************************************/

        public static string CategoryName(this Document document, string familyName)
        {
            if (document == null || string.IsNullOrEmpty(familyName))
                return null;

            List<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>().ToList();

            ElementType elementType = elementTypes.Find(x => x.FamilyName == familyName && x.Category != null);

            if (elementType == null)
                return null;

            return elementType.Category.Name;
        }

        /***************************************************/
    }
}

