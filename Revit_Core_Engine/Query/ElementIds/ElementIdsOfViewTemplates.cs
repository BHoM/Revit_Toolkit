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
using BH.oM.Reflection.Attributes;
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

        [Description("Filters ElementIds of Revit view templates that have a given name. If view name is left blank, ElementIds of all view templates in the document will be filtered.")]
        [Input("document", "Revit document to be processed.")]
        [Input("viewTemplateName", "Name used to filter the Revit view templates. Optional: if left blank, ElementIds of all view templates in the document will be filtered.")]
        [Input("caseSensitive", "If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfViewTemplates(this Document document, string viewTemplateName = null, bool caseSensitive = true, IEnumerable <ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());

            if (!string.IsNullOrEmpty(viewTemplateName))
            {
                IEnumerable<ElementId> result;
                if (caseSensitive)
                    result = collector.OfClass(typeof(View)).Cast<View>().Where(x => x.IsTemplate && x.Name == viewTemplateName).Select(x => x.Id);
                else
                    result = collector.OfClass(typeof(View)).Cast<View>().Where(x => x.IsTemplate && x.Name.ToUpper() == viewTemplateName.ToUpper()).Select(x => x.Id);

                if (result.Count() == 0)
                    BH.Engine.Reflection.Compute.RecordWarning("Couldn't find any View Template named " + viewTemplateName + ".");
                else if (result.Count() != 1)
                    BH.Engine.Reflection.Compute.RecordWarning("More than one View Template named " + viewTemplateName + " has been found.");

                return result;
            }
            else
                return collector.OfClass(typeof(View)).Cast<View>().Where(x => x.IsTemplate).Select(x => x.Id);
        }
    }

    /***************************************************/
}


