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
using BH.oM.Base.Attributes;
using System;
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

        [Description("Filters ElementIds of elements that intersect or are contained in a Scope Box with the given name.")]
        [Input("document", "Revit document to be processed.")]
        [Input("boxName", "Name of the Revit Scope Box to be used as a geometrical filter.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByScopeBox(this Document document, string boxName, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;
            
            if (ids != null && !ids.Any())
                return new List<ElementId>();

            Solid box = new FilteredElementCollector(document).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_VolumeOfInterest).FirstOrDefault(x => x.Name == boxName)?.ToSolid();
            if (box == null && document.IsLinked)
            {
                Document hostDoc = document.HostDocument();
                box = new FilteredElementCollector(hostDoc).OfCategory(Autodesk.Revit.DB.BuiltInCategory.OST_VolumeOfInterest).FirstOrDefault(x => x.Name == boxName)?.ToSolid();
                if (box != null)
                {
                    Transform linkTransform = document.LinkTransform();
                    box = SolidUtils.CreateTransformed(box, linkTransform.Inverse);
                    BH.Engine.Base.Compute.RecordNote($"The Scope Box named {boxName} used to filter the elements was found in the document hosting the link document it was originally requested with.");
                }
            }

            if (box == null)
            {
                BH.Engine.Base.Compute.RecordError($"Couldn't find a Scope Box named {boxName}.");
                return new HashSet<ElementId>();
            }

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            ElementIntersectsSolidFilter filter = new ElementIntersectsSolidFilter(box);

            return collector.WherePasses(filter).ToElementIds();
        }

        /***************************************************/
    }
}

