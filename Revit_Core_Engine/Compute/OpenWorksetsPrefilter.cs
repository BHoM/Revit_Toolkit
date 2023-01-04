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
using BH.oM.Base.Attributes;
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

        [Description("Executes initial filtering of elements to rule out closed worksets. Returns a collection of ElementIds or a null to be passed on to IElementIds method) for further filtering.")]
        [Input("document", "Revit document to be processed.")]
        [Output("prefilter", "Collection of all ElementIds that exist in the open worksets of the input document.")]
        public static IEnumerable<ElementId> OpenWorksetsPrefilter(this Document document)
        {
            List<WorksetId> open = document.OpenWorksetIds().Union(document.SystemWorksetIds()).ToList();
            List<WorksetId> all = document.AllWorksetIds().ToList();
            if (open.Count == all.Count)
                return null;

            FilteredElementCollector collector = new FilteredElementCollector(document);

            // If most worksets are closed, take the elements from open worksets, otherwise rule out the ones from closed worksets
            if (open.Count < all.Count / 2)
            {
                IEnumerable<WorksetId> filterWorksets = open;
                collector = collector.WherePasses(new LogicalOrFilter(filterWorksets.Select(x => new ElementWorksetFilter(x, false) as ElementFilter).ToList()));
            }
            else
            {
                IEnumerable<WorksetId> filterWorksets = all.Except(open);
                collector = collector.WherePasses(new LogicalAndFilter(filterWorksets.Select(x => new ElementWorksetFilter(x, true) as ElementFilter).ToList()));
            }

            return collector.ToElementIds();
        }

        /***************************************************/
    }
}

