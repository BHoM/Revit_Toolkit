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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the Revit built-in category matching a given category name.")]
        [Input("document", "Revit document to query the categories from.")]
        [Input("categoryName", "Name of the sought built-in Revit category.")]
        [Input("caseSensitive", "If true, the category name matching is case sensitive, otherwise not.")]
        [Output("Revit built-in category matching the input name.")]
        public static BuiltInCategory BuiltInCategory(this Document document, string categoryName, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(categoryName) || document?.Settings?.Categories == null)
                return Autodesk.Revit.DB.BuiltInCategory.INVALID;

            //TODO: use LabelUtils?
            foreach (Category category in document.Settings.Categories)
            {
                if ((caseSensitive && category.Name == categoryName) || (!caseSensitive && category.Name.ToUpper() == categoryName.ToUpper()))
                    return (BuiltInCategory)category.Id.IntegerValue;
            }

            return Autodesk.Revit.DB.BuiltInCategory.INVALID;
        }

        /***************************************************/

        [Description("Returns Revit category, to which a given BHoM IInstance belongs.")]
        [Input("instance", "BHoM IInstance to extract the category information from.")]
        [Input("document", "Revit document to query the categories from.")]
        [Input("caseSensitive", "If true, the category name matching is case sensitive, otherwise not.")]
        [Output("Revit built-in category, to which the input BHoM IInstance belongs.")]
        public static BuiltInCategory BuiltInCategory(this IInstance instance, Document document, bool caseSensitive = true)
        {
            return (instance?.Properties).BuiltInCategory(document, caseSensitive);
        }

        /***************************************************/

        [Description("Returns Revit category, reference to which is stored in a given BHoM InstanceProperties.")]
        [Input("properties", "BHoM InstanceProperties to extract the category information from.")]
        [Input("document", "Revit document to query the categories from.")]
        [Input("caseSensitive", "If true, the category name matching is case sensitive, otherwise not.")]
        [Output("Revit built-in category, reference to which is stored in the input BHoM InstanceProperties.")]
        public static BuiltInCategory BuiltInCategory(this InstanceProperties properties, Document document, bool caseSensitive = true)
        {
            return document.BuiltInCategory(properties?.CategoryName, caseSensitive);
        }

        /***************************************************/
    }
}

