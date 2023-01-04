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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements that are visible in active view of the host document. If the input document is a linked document, its elements visible in the active view of the host document will be returned.")]
        [Input("document", "Revit document to be processed (can be a linked document).")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByVisibleInActiveView(this Document document, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            Document hostDocument = document.IsLinked ? document.HostDocument() : document;

            ActiveViewVisibilityContext context;
            if (hostDocument.ActiveView is View3D)
                context = new ActiveViewVisibilityContext(hostDocument, document);
            else
            {
#if (REVIT2018 || REVIT2019)
                BH.Engine.Base.Compute.RecordError($"Querying visible elements from the active view failed because only 3d views are supported in Revit 2019 or older. In later versions also 2d views can be queried.");
                return null;
#else
                context = new Active2dViewVisibilityContext(hostDocument, document);
#endif
            }

            CustomExporter exporter = new CustomExporter(hostDocument, context);
            exporter.IncludeGeometricObjects = false;
            exporter.ShouldStopOnError = false;

#if (!REVIT2018 && !REVIT2019)
            exporter.Export2DIncludingAnnotationObjects = true;
#endif

            try
            {
                exporter.Export(new List<ElementId> { hostDocument.ActiveView.Id });
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException ex)
            {
                if (ex.ParamName == "viewIds")
                {
                    BH.Engine.Base.Compute.RecordError($"Visible elements could not be queried from the active view because views of type {hostDocument.ActiveView.ViewType} are not queryable in this version of Revit.");
                    return null;
                }
                else
                    throw;
            }

            HashSet<ElementId> result = context.GetElementsVisibleInActiveView(document);
            if (ids != null)
                result.IntersectWith(ids);

            return result;
        }

        /***************************************************/
    }   
}

