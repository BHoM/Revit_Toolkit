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

using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get the ElementId of all Views, with option to narrow search by View name only")]
        [Input("document", "Revit Document where ElementIds are collected")]
        [Input("viewName", "Optional, narrows the search by a View name. If blank it returns all Views")]
        [Input("caseSensitive", "Optional, sets the View name to be case sensitive or not")]
        [Input("ids", "Optional, allows the filter to narrow the search from an existing enumerator")]
        [Output("elementIdsOfViews", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsOfViews(this Document document, string viewName = null, bool caseSensitive = true, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());

            if (!string.IsNullOrEmpty(viewName))
            {
                IEnumerable<ElementId> result;
                if (caseSensitive)
                    result = collector.OfClass(typeof(View)).Where(x => x.Name == viewName).Select(x => x.Id);
                else
                    result = collector.OfClass(typeof(View)).Where(x => x.Name.ToUpper() == viewName.ToUpper()).Select(x => x.Id);

                if (result.Count() == 0)
                    BH.Engine.Reflection.Compute.RecordWarning("Couldn't find any View named " + viewName + ".");
                else if (result.Count() != 1)
                    BH.Engine.Reflection.Compute.RecordWarning("More than one View named " + viewName + " has been found.");

                return result;
            }
            else
                return collector.OfClass(typeof(View)).Select(x => x.Id);
        }

        /***************************************************/
    }
}