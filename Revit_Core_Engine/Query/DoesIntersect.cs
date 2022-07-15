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

            Transform transform1 = element1.Document.LinkTransform() ?? Transform.Identity;
            Transform transform2 = element2.Document.LinkTransform() ?? Transform.Identity;

            //null or equal transforms
            if ((transform1.IsIdentity && transform2.IsIdentity) ||  transform1.AlmostEqual(transform2))
            {
                ElementIntersectsElementFilter intersectFilter = new ElementIntersectsElementFilter(element2);
                return intersectFilter.PassesFilter(element1);
            }
            else
            {
                //host vs link
                if (transform1.IsIdentity && !transform2.IsIdentity)
                {
                    return DoesIntersectWithTransform(element1, element2, transform2);
                }
                //link vs host
                else if (!transform1.IsIdentity && transform2.IsIdentity)
                {
                    return DoesIntersectWithTransform(element2, element1, transform1);
                }
                //link vs link
                if (!transform1.IsIdentity && !transform2.IsIdentity)
                {
                    Transform doubleTransform = transform2.Multiply(transform1.Inverse);
                    return DoesIntersectWithTransform(element1, element2, doubleTransform);
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

                if (bbox.Transform.IsTranslation)
                {
                    outline.MinimumPoint += bbox.Transform.Origin;
                    outline.MaximumPoint += bbox.Transform.Origin;
                }

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
        [Input("hostElement", "First element to check the intersection for.")]
        [Input("transElement", "Second element to check the intersection for.")]
        [Input("transform", "Transformation of the second element.")]
        [Output("bool", "Result of the intersect checking.")]
        private static bool DoesIntersectWithTransform(this Element hostElement, Element transElement, Transform transform)
        {

            List<Solid> solids = transElement.Solids(new Options()).Select(x => SolidUtils.CreateTransformed(x, transform)).ToList();

            foreach (Solid solid in solids)
            {
                ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(solid);
                if (intersectFilter.PassesFilter(hostElement))
                    return true;
            }

            return false;
        }

        /***************************************************/

    }
}