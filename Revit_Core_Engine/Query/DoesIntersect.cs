/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

            if ((transform1.IsIdentity && transform2.IsIdentity) ||  transform1.AlmostEqual(transform2))
            {
                ElementIntersectsElementFilter intersectFilter = new ElementIntersectsElementFilter(element2);
                return intersectFilter.PassesFilter(element1);
            }
            else
            {
                Transform doubleTransform = transform2.Multiply(transform1.Inverse);
                return DoesIntersectWithTransform(element1, element2, doubleTransform);
            }
        }

        /***************************************************/

        [Description("Check if bounding box intersects with element.")]
        [Input("bbox", "Bounding box to check the intersection for.")]
        [Input("element", "Element to check the intersection for.")]
        [Input("transform", "Transform of the element. If element is from revit link, revit link instance transform should be used.")]
        [Output("bool", "Result of the intersect checking.")]
        public static bool DoesIntersect(this BoundingBoxXYZ bbox, Element element, Transform transform = null)
        {
            if (bbox == null || element == null)
            {
                return false;
            }

            if (transform == null)
                transform = Transform.Identity;

            Outline outline = new Outline(bbox.Min, bbox.Max);

            if (bbox.Transform.IsTranslation)
            {
                outline.MinimumPoint += bbox.Transform.Origin;
                outline.MaximumPoint += bbox.Transform.Origin;
            }
            else
            {
                BH.Engine.Base.Compute.RecordWarning("Intersection of the bounding boxes could not be checked. Only translation and identity transformation is currently supported.");
                return false;
            }

            if (transform == null || transform.IsIdentity)
            {
                return element.DoesIntersectWithOutline(outline);
            }
            else if (transform.IsTranslation)
            {
                outline.MinimumPoint = transform.Inverse.OfPoint(outline.MinimumPoint);
                outline.MaximumPoint = transform.Inverse.OfPoint(outline.MaximumPoint);

                return element.DoesIntersectWithOutline(outline);
            }
            else
            {
                BoundingBoxXYZ transformedBBox = BoundsOfTransformed(bbox, transform.Inverse);
                outline.MinimumPoint = transformedBBox.Min;
                outline.MaximumPoint = transformedBBox.Max;

                if (!element.DoesIntersectWithOutline(outline))
                    return false;

                Solid bboxSolid = bbox.ToSolid();
                Solid transformedBBoxSolid = SolidUtils.CreateTransformed(bboxSolid, transform.Inverse);
                ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(transformedBBoxSolid);

                return intersectFilter.PassesFilter(element);
            }
        }

        /***************************************************/

        [Description("Check if point intersects with element.")]
        [Input("point", "Point to check the intersection for.")]
        [Input("element", "Element to check the intersection for.")]
        [Output("bool", "Result of the intersect checking.")]
        public static bool DoesIntersect(this XYZ point, Element element)
        {
            if (point == null || element == null)
            {
                return false;
            }

            List<Solid> solids = element.Solids(new Options());
            Transform transform = element.Document.IsLinked ? element.Document.LinkTransform() : Transform.Identity;

            if (!transform.IsIdentity)
                solids = solids.Select(x => SolidUtils.CreateTransformed(x, transform)).ToList();

            foreach (var solid in solids)
            {
                if (DoesIntersect(point, solid))
                    return true;
            }

            return false;
        }

        /***************************************************/

        [Description("Check if point intersects with solid.")]
        [Input("point", "Point to check the intersection for.")]
        [Input("solid", "Solid to check the intersection for.")]
        [Output("bool", "Result of the intersect checking.")]
        public static bool DoesIntersect(this XYZ point, Solid solid)
        {
            if (point == null || solid == null)
            {
                return false;
            }

            Line line = Line.CreateBound(point, point.Add(XYZ.BasisZ));
            SolidCurveIntersection sci = solid.IntersectWithCurve(line, new SolidCurveIntersectionOptions());

            for (int i = 0; i < sci.SegmentCount; i++)
            {
                Curve c = sci.GetCurveSegment(i);

                if (point.IsAlmostEqualTo(c.GetEndPoint(0), BH.oM.Geometry.Tolerance.Distance) || point.IsAlmostEqualTo(c.GetEndPoint(1), BH.oM.Geometry.Tolerance.Distance))
                {
                    return true;
                }
            }

            return false;
        }

        /***************************************************/

        [Description("Check if solid intersects with another solid.")]
        [Input("solid1", "First solid to check the intersection for.")]
        [Input("solid2", "Second solid to check the intersection for.")]
        [Output("bool", "Result of the intersect checking.")]
        public static bool DoesIntersect(this Solid solid1, Solid solid2)
        {
            if (solid1 == null || solid2 == null)
            {
                return false;
            }

            Solid intersectSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
            return Math.Abs(intersectSolid.Volume) > Math.Pow(BH.oM.Geometry.Tolerance.Distance, 3);
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
            List<Solid> solids = transElement.Solids(new Options());

            foreach (Solid solid in solids)
            {
                Solid newSolid = SolidUtils.CreateTransformed(solid, transform);
                ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(newSolid);
                if (intersectFilter.PassesFilter(hostElement))
                    return true;
            }

            return false;
        }

        /***************************************************/


        [Description("Check intersection between element and outline.")]
        private static bool DoesIntersectWithOutline(this Element element, Outline outline)
        {
            BoundingBoxIntersectsFilter bboxIntersect = new BoundingBoxIntersectsFilter(outline);
            BoundingBoxIsInsideFilter bboxInside = new BoundingBoxIsInsideFilter(outline);
            LogicalOrFilter bboxFilter = new LogicalOrFilter(new List<ElementFilter> { bboxIntersect, bboxInside });

            return bboxFilter.PassesFilter(element);
        }

        /***************************************************/

    }
}

