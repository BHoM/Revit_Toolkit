/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
    public static partial class Query
    {
        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        [Description("Collects elements within a bounding box from the host document and optionally from linked Revit documents.")]
        [Input("hostDoc", "The host Revit document.")]
        [Input("bbox", "The bounding box to search within.")]
        [Input("categories", "Optional array of categories to filter by. If null, collects from all categories.")]
        [Input("revitLinks", "Optional list of Revit link instances to search in. If null, only searches the host document.")]
        [Output("elements", "List of elements within the bounding box.")]
        public static List<Element> ElementsInBBox(Document hostDoc, BoundingBoxXYZ bbox, BuiltInCategory[] categories = null, List<RevitLinkInstance> revitLinks = null)
        {
            List<Element> elements = new List<Element>();

            // Create category filter - if categories is null, don't filter by category
            ElementFilter categoryFilter = null;
            if (categories != null && categories.Length > 0)
            {
                List<ElementFilter> elementCategoryFilters = categories.Select(cat => new ElementCategoryFilter(cat)).Cast<ElementFilter>().ToList();
                categoryFilter = new LogicalOrFilter(elementCategoryFilters);
            }

            // Create bounding box filter
            var bboxFilter = BoundingBoxFilter(bbox);

            // Collect from host document
            var collector = new FilteredElementCollector(hostDoc);
            if (categoryFilter != null)
                collector = collector.WherePasses(categoryFilter);

            var hostElements = collector.WherePasses(bboxFilter).ToElements();
            elements.AddRange(hostElements);

            // Collect from linked documents only if revitLinks is provided
            if (revitLinks != null)
            {
                foreach (var link in revitLinks)
                {
                    var linkDoc = link.GetLinkDocument();
                    if (linkDoc == null)
                        continue;

                    // Transform bounding box to link document coordinates
                    Transform transform = link.GetTotalTransform();
                    var transformedBbox = bbox.BoundsOfTransformed(transform.Inverse);
                    var linkBboxFilter = BoundingBoxFilter(transformedBbox);

                    var linkCollector = new FilteredElementCollector(linkDoc);
                    if (categoryFilter != null)
                        linkCollector = linkCollector.WherePasses(categoryFilter);

                    var linkElements = linkCollector.WherePasses(linkBboxFilter).ToElements();
                    elements.AddRange(linkElements);
                }
            }

            return elements;
        }

        /***************************************************/
        /****               Private methods             ****/
        /***************************************************/

        private static LogicalOrFilter BoundingBoxFilter(BoundingBoxXYZ bbox)
        {
            Outline outline = new Outline(bbox.Min, bbox.Max);
            BoundingBoxIntersectsFilter bboxIntersect = new BoundingBoxIntersectsFilter(outline);
            BoundingBoxIsInsideFilter bboxInside = new BoundingBoxIsInsideFilter(outline);
            return new LogicalOrFilter(new List<ElementFilter> { bboxIntersect, bboxInside });
        }

        /***************************************************/
    }
}





