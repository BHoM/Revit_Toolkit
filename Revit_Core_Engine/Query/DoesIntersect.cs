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

            Document doc1 = element1.Document;
            Document doc2 = element2.Document;

            //host vs host
            if (!doc1.IsLinked && !doc2.IsLinked)
            {
                BoundingBoxXYZ bbox1 = element1.PhysicalBounds();
                BoundingBoxXYZ bbox2 = element2.PhysicalBounds();

                if (!bbox1.IsInRange(bbox2))
                    return false;

                ElementIntersectsElementFilter intersectFilter = new ElementIntersectsElementFilter(element2);
                return intersectFilter.PassesFilter(element1);
            }
            //host vs link
            else if (!doc1.IsLinked && doc2.IsLinked)
            {
                BoundingBoxXYZ bbox1 = element1.PhysicalBounds();
                Transform transform2 = doc2.LinkTransform();
                BoundingBoxXYZ bbox2 = element2.get_Geometry(new Options()).GetTransformed(transform2).GetBoundingBox();

                if (!bbox1.IsInRange(bbox2))
                    return false;

                List<Solid> element2Solids = element2.Solids(new Options());
                foreach (Solid elSolid in element2Solids)
                {
                    Solid transformedSolid = SolidUtils.CreateTransformed(elSolid, transform2);
                    ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(transformedSolid);
                    if (intersectFilter.PassesFilter(element1))
                        return true;
                }
            }
            //link vs host
            else if (doc1.IsLinked && !doc2.IsLinked)
            {
                BoundingBoxXYZ bbox2 = element2.PhysicalBounds();
                Transform transform1 = doc1.LinkTransform();
                BoundingBoxXYZ bbox1 = element1.get_Geometry(new Options()).GetTransformed(transform1).GetBoundingBox();

                if (!bbox1.IsInRange(bbox2))
                    return false;

                List<Solid> element1Solids = element1.Solids(new Options());
                foreach (Solid elSolid in element1Solids)
                {
                    Solid transformedSolid = SolidUtils.CreateTransformed(elSolid, transform1);
                    ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(transformedSolid);
                    if (intersectFilter.PassesFilter(element1))
                        return true;
                }
            }
            //link vs link
            else if (doc1.IsLinked && doc2.IsLinked)
            {
                Transform transform1 = doc1.LinkTransform();
                Transform transform2 = doc2.LinkTransform();
                BoundingBoxXYZ bbox1 = element1.get_Geometry(new Options()).GetTransformed(transform1).GetBoundingBox();
                BoundingBoxXYZ bbox2 = element2.get_Geometry(new Options()).GetTransformed(transform2).GetBoundingBox();

                if (!bbox1.IsInRange(bbox2))
                    return false;

                List<Solid> element1Solids = element1.Solids(new Options());
                List<Solid> element2Solids = element2.Solids(new Options());
                List<Solid> el1TransSolids = element1Solids.Select(x => SolidUtils.CreateTransformed(x, transform1)).ToList();
                List<Solid> el2TransSolids = element2Solids.Select(x => SolidUtils.CreateTransformed(x, transform2)).ToList();

                foreach (Solid el1Solid in el1TransSolids)
                {
                    foreach (Solid el2Solid in el2TransSolids)
                    {
                        Solid intersection = BooleanOperationsUtils.ExecuteBooleanOperation(el1Solid, el2Solid, BooleanOperationsType.Intersect);
                        if (intersection.Volume != 0)
                            return true;
                    }
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

            Document doc = element.Document;

            Outline outline = new Outline(bbox.Min, bbox.Max);
            BoundingBoxIntersectsFilter bboxIntersect = new BoundingBoxIntersectsFilter(outline);
            BoundingBoxIsInsideFilter bboxInside = new BoundingBoxIsInsideFilter(outline);
            LogicalOrFilter bboxFilter = new LogicalOrFilter(new List<ElementFilter> { bboxIntersect, bboxInside });

            //host vs host
            if (!doc.IsLinked)
            {
                return bboxFilter.PassesFilter(element);
            }
            //host vs link
            else
            {
                Transform transform = doc.LinkTransform();
                BoundingBoxXYZ elBbox = element.get_Geometry(new Options()).GetTransformed(transform).GetBoundingBox();

                if (!bbox.IsInRange(elBbox))
                    return false;

                Solid bboxSolid = bbox.ToSolid();
                Solid transformedBBoxSolid = SolidUtils.CreateTransformed(bboxSolid, transform.Inverse);
                ElementIntersectsSolidFilter intersectFilter = new ElementIntersectsSolidFilter(transformedBBoxSolid);
                return intersectFilter.PassesFilter(element);
            }
        }

        /***************************************************/

    }
}