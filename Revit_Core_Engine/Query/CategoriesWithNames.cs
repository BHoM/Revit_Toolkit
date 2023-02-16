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

using Autodesk.Revit.DB;
using BH.Engine.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        [Description("Returns a dictionary of BuiltInCategories supported by BHoM (UI, filtering by category) and their names." +
                     "\nThe categories as well as  their names come from BH.oM.Revit.Enums.Category.")]
        [Output("categories", "Dictionary of BuiltInCategories supported by BHoM and their names.")]
        public static Dictionary<BuiltInCategory, string> CategoriesWithNames()
        {
            if (m_CategoriesWithNames == null)
                CollectCategories();

            return m_CategoriesWithNames;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void CollectCategories()
        {
            m_CategoriesWithNames = new Dictionary<BuiltInCategory, string>();
            foreach (BH.oM.Revit.Enums.Category category in Enum.GetValues(typeof(BH.oM.Revit.Enums.Category)))
            {
                BuiltInCategory builtInCategory = BH.Engine.Base.Compute.ParseEnum<BuiltInCategory>(category.ToString());
                if (builtInCategory != default(BuiltInCategory))
                    m_CategoriesWithNames.Add(builtInCategory, category.ToText());
            }
        }


        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<BuiltInCategory, string> m_CategoriesWithNames = null;

        /***************************************************/
    }
}
