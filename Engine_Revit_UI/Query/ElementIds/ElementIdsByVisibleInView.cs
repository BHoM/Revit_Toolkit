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
using System;
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

        [Description("Filters ElementIds of elements that are visible in a view.")]
        [Input("document", "Revit document to be processed.")]
        [Input("viewId", "ElementId of the Revit view in which the filtered elements are visible.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByVisibleInView(this Document document, int viewId, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            View view = document.GetElement(new ElementId(viewId)) as View;
            if (view != null)
            {
                if (ids != null && ids.Count() == 0)
                    return new HashSet<ElementId>();

                FilteredElementCollector collector = new FilteredElementCollector(document, view.Id);

                if (ids == null)
                    return collector.ToElementIds();
                else
                    return collector.ToElementIds().Intersect(ids);
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Couldn't find a View under ElementId {0}", viewId));
                return new HashSet<ElementId>();
            }
        }

        /***************************************************/
    }
}