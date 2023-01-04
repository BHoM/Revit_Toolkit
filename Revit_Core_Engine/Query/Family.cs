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
using BH.oM.Adapters.Revit.Settings;
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

        [Description("Searches a given Revit document for a family that corresponds to the input BHoM family wrapper.")]
        [Input("family", "BHoM family wrapper to find the corresponding Revit family for.")]
        [Input("document", "Revit document to parse for the correspondent family.")]
        [Input("builtInCategories", "Revit built-in categories to be taken into account when parsing the document.")]
        [Input("familyLoadSettings", "Settings specifying whether and how should the Revit families be dynamically loaded.")]
        [Output("family", "Revit family correspondent to the input BHoM family wrapper.")]
        public static Family Family(this oM.Adapters.Revit.Elements.Family family, Document document, IEnumerable<BuiltInCategory> builtInCategories, FamilyLoadSettings familyLoadSettings = null)
        {
            if (family == null || string.IsNullOrWhiteSpace(family.Name) || document == null)
                return null;

            //Find Existing Family in Document
            FilteredElementCollector collector = new FilteredElementCollector(document).OfClass(typeof(Family));
            if (builtInCategories != null && builtInCategories.Any(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID))
                collector = collector.WherePasses(new LogicalOrFilter(builtInCategories.Where(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID).Select(x => new ElementCategoryFilter(x) as ElementFilter).ToList()));

            Family revitFamily = collector.FirstOrDefault(x => x.Name == family.Name) as Family;
            if (revitFamily != null)
                return revitFamily;

            //Find ElementType in FamilyLibrary
            if (familyLoadSettings != null)
            {
                if (builtInCategories != null && builtInCategories.Any(x => x != Autodesk.Revit.DB.BuiltInCategory.INVALID))
                {
                    foreach (BuiltInCategory builtInCategory in builtInCategories)
                    {
                        if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                            continue;

                        revitFamily = familyLoadSettings.LoadFamily(document, builtInCategory.CategoryName(document), family.Name);
                        if (revitFamily != null)
                            return revitFamily;
                    }
                }
                else
                    return familyLoadSettings.LoadFamily(document, null, family.Name);
            }

            return null;
        }

        /***************************************************/
    }
}

