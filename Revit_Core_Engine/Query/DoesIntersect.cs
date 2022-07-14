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

        [Description("Check if two elements intersect.")]
        [Input("element1", "First element to check the intersection for.")]
        [Input("element2", "Second element to check the intersection for.")]
        [Output("bool", "Result of the intersect checking.")]
        public static bool DoesIntersect(this Element element1, Element element2)
        {
            if (element1 == null || element2 == null)
            {
                return false;
            }

            Transform transform1 = element1.Document.LinkTransform();
            Transform transform2 = element2.Document.LinkTransform();

            //host vs host
            if (transform1 == null && transform2 == null)
            {
                ElementIntersectsElementFilter intersectFilter = new ElementIntersectsElementFilter(element2);
                return intersectFilter.PassesFilter(element1);
            }
            //host vs link || link vs host
            else if ((transform1 == null && transform2 != null) || (transform1 != null && transform2 == null))
            {
                return DoesIntersectHostVsLink(element1, transform1, element2, transform2);
            }
            //link vs link
            else if (transform1 != null && transform2 != null)
            {
                if (transform1.AlmostEqual(transform2))
                {
                    ElementIntersectsElementFilter intersectFilter = new ElementIntersectsElementFilter(element2);
                    return intersectFilter.PassesFilter(element1);
                }

                List<Solid> element2Solids = element2.Solids(new Options());
                Transform doubleTransform = transform2.Multiply(transform1.Inverse);
                List<Solid> el2TransSolids = element2Solids.Select(x => SolidUtils.CreateTransformed(x, doubleTransform)).ToList();

                foreach (Solid el2Solid in el2TransSolids)
                {
                    ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(el2Solid);
                    if (intersectFilter.PassesFilter(element1))
                        return true;
                }
            }

            return false;
        }

        /***************************************************/

        [Description("Check if bounding box intersects with element.")]
        [Input("bbox", "Bounding box to check the intersection for.")]
        [Input("element", "Element to check the intersection for.")]
        [Output("bool", "Result of the intersect checking.")]
        public static bool DoesIntersect(this BoundingBoxXYZ bbox, Element element)
        {
            if (bbox == null || element == null)
            {
                return false;
            }

            if (!bbox.Transform.IsIdentity)
            {
                BH.Engine.Base.Compute.RecordWarning("Intersection of the bounding boxe could not be checked. Only identity transformation is currently supported.");
                return false;
            }

            Transform transform = element.Document.LinkTransform();

            //host vs host
            if (transform == null)
            {
                Outline outline = new Outline(bbox.Min, bbox.Max);
                BoundingBoxIntersectsFilter bboxIntersect = new BoundingBoxIntersectsFilter(outline);
                BoundingBoxIsInsideFilter bboxInside = new BoundingBoxIsInsideFilter(outline);
                LogicalOrFilter bboxFilter = new LogicalOrFilter(new List<ElementFilter> { bboxIntersect, bboxInside });

                return bboxFilter.PassesFilter(element);
            }
            //host vs link
            else
            {
                Solid bboxSolid = bbox.ToSolid();
                Solid transformedBBoxSolid = SolidUtils.CreateTransformed(bboxSolid, transform.Inverse);
                ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(transformedBBoxSolid);
                return intersectFilter.PassesFilter(element);
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Check if two elements intersect.")]
        [Input("element1", "First element to check the intersection for.")]
        [Input("element2", "Second element to check the intersection for.")]
        [Output("bool", "Result of the intersect checking.")]
        private static bool DoesIntersectHostVsLink(this Element element1, Transform transform1, Element element2, Transform transform2)
        {
            Element hostElement = null;
            Element linkElement = null;
            Transform linkTransform = null;

            if (transform1 == null && transform2 != null)
            {
                hostElement = element1;
                linkElement = element2;
                linkTransform = transform2;
            }
            else if (transform1 != null && transform2 == null)
            {
                hostElement = element2;
                linkElement = element1;
                linkTransform = transform1;
            }

            List<Solid> element2Solids = linkElement.Solids(new Options());
            foreach (Solid elSolid in element2Solids)
            {
                Solid transformedSolid = SolidUtils.CreateTransformed(elSolid, linkTransform);
                ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(transformedSolid);
                if (intersectFilter.PassesFilter(hostElement))
                    return true;
            }

            return false;
        }

        /***************************************************/

    }
}