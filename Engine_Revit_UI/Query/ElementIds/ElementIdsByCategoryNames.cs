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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Interface;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;



namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get Elements as ElementIds by Category names, including Family Instances and Family Types")]
        [Input("document", "Revit Document where ElementIds are collected")]
        [Input("categoryNames", "List of Category names to be used as filter")]
        [Input("ids", "Optional, allows the filter to narrow the search from an existing enumerator")]
        [Output("elementIdsByCategoryNames", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsByCategoryNames(this Document document, List<string> categoryNames, IEnumerable<ElementId> ids = null)
        {
            if (document == null || categoryNames.Count == 0)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();            

            foreach (string name in categoryNames)
            {
                BuiltInCategory builtInCategory = document.BuiltInCategory(name);
                if (builtInCategory == Autodesk.Revit.DB.BuiltInCategory.INVALID)
                {
                    BH.Engine.Reflection.Compute.RecordError("Couldn't find a Category named " + name + ".");
                    continue;
                }

                FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());                
                result.UnionWith(collector.OfCategory(builtInCategory).ToElementIds());                                
            }
            return result;       
        }

        /***************************************************/

        [Description("Get Elements as ElementIds by Category name, including Family Instances and Family Types")]
        [Input("document", "Revit Document where ElementIds are collected")]
        [Input("categoryName", "Category name to be used as filter")]
        [Input("ids", "Optional, allows the filter to narrow the search from an existing enumerator")]
        [Output("elementIdsByCategoryNames", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsByCategoryNames(this Document document, string categoryName, IEnumerable<ElementId> ids = null)
        {              
            return ElementIdsByCategoryNames(document, new List<string> { categoryName }, ids);
        }

        /***************************************************/

    }
}