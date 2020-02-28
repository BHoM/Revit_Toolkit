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
 
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get all Elements as ElementId that are visible in a list of Views")]
        [Input("document", "Revit Document where views are collected")]
        [Input("viewName", "Revit view where visible elements are collected")]
        [Input("caseSensitive", "Optional, sets the View name to be case sensitive or not")]
        [Input("ids", "Optional, allows the filter to narrow the search from an existing enumerator")]
        [Output("elementIdsByVisibleInView", "An enumerator for easy iteration of ElementIds collected")]
        public static IEnumerable<ElementId> ElementIdsByVisibleInView(this Document document, string viewName, bool caseSensitive = true, IEnumerable<ElementId> ids = null)
        {
            if (document == null || viewName == null)
                return null;

            IEnumerable<Element> viewCollector = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_Views);
            View view = null;

            if (caseSensitive)
                view = viewCollector.Where(x => x.Name == viewName).FirstOrDefault() as View;
            else
                view = viewCollector.Where(x => x.Name.ToUpper() == viewName.ToUpper()).FirstOrDefault() as View;            

            if(viewCollector != null)
            {
                if (ids != null && ids.Count() == 0)
                    return new List<ElementId>();

                FilteredElementCollector collector = new FilteredElementCollector(document, view.Id);
                if (ids == null)
                {
                    return collector.ToElementIds();
                }
                else
                {
                    HashSet<ElementId> result = new HashSet<ElementId>(collector.ToElementIds());
                    result.IntersectWith(ids);
                    return result;
                }
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("Couldn't find a View named " + viewName + ".");
                return null;
            }
        }

        /***************************************************/

    }
}