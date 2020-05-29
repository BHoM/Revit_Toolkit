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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of Revit views that have a given name. If view name is left blank, ElementIds of all views in the document will be filtered.")]
        [Input("document", "Revit document to be processed.")]
        [Input("viewName", "Name used to filter the Revit views. Optional: if left blank, ElementIds of all views in the document will be filtered.")]
        [Input("caseSensitive", "If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
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

        [Description("Filters ElementIds of Revit views of given view type.")]
        [Input("document", "Revit document to be processed.")]
        [Input("viewType", "Revit view type used to filter the Revit views.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfViews(this Document document, ViewType viewType, IEnumerable<ElementId> ids = null)
        {
            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            return collector.OfClass(typeof(View)).Cast<View>().Where(x => !x.IsTemplate).Where(x => x.ViewType == viewType).Select(x => x.Id);
        }

        /***************************************************/

        [Description("Filters ElementIds of Revit views that implement a given view template.")]
        [Input("document", "Revit document to be processed.")]
        [Input("templateId", "Integer representing ElementId of Revit view template implemented by filtered views.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfViews(this Document document, int templateId, IEnumerable<ElementId> ids = null)
        {
            View viewTemplate = document.GetElement(new ElementId(templateId)) as View;
            if (viewTemplate == null || !viewTemplate.IsTemplate)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Couldn't find a View Template under ElementId {0}", templateId));
                return new List<ElementId>();
            }

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());
            return collector.OfClass(typeof(View)).Cast<View>().Where(x => !x.IsTemplate).Where(x => x.ViewTemplateId == viewTemplate.Id).Select(x => x.Id);
        }

        /***************************************************/
    }
}