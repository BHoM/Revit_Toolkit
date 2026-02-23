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
using Autodesk.Revit.DB.Mechanical;
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

        [PreviousVersion("9.1", "BH.Revit.Engine.Core.Query.Space(Autodesk.Revit.DB.Element, System.Collections.Generic.IEnumerable<Autodesk.Revit.DB.Mechanical.Space>, System.Boolean)")]
        [Description("Returns the Revit Space that contains the given element.")]
        [Input("element", "The Revit element for which to find the containing Space.")]
        [Input("spaces", "An optional collection of Revit Spaces to search. If not provided, all Spaces in the element's document will be used.")]
        [Input("useRoomCalculationPoint", "If true and the element is a FamilyInstance with a spatial element calculation point, that point will be used for containment checks.")]
        [Input("findClosestIfNotContained", "If true, if no containing Space is found, the method will attempt to find the closest Space in the direction of the element's connectors or below it.")]
        [Output("space", "The Revit Space containing the element, or the element itself if it is a Space. Returns null if no containing Space is found.")]
        public static Space Space(this Element element, IEnumerable<Space> spaces, bool useRoomCalculationPoint = false, bool findClosestIfNotContained = false)
        {
            if (element == null)
                return null;

            // 1. If the element is a Space, return itself
            if (element is Space space)
                return space;

            // 2. If the element is a FamilyInstance, try the .Space property
            if (element is FamilyInstance fi && fi.Space != null)
                return fi.Space;

            if (spaces == null)
            {
                Document doc = element.Document;
                spaces = new FilteredElementCollector(doc).OfClass(typeof(SpatialElement)).OfType<Space>().ToList();
            }
            else
            {
                m_LinkTransforms = spaces.GroupBy(s => s.Document).Where(g => g.Key.IsLinked).ToDictionary(g => g.Key, g => g.Key.LinkInstance().GetTotalTransform());
            }

            // 3. Use location point and check which space contains it
            XYZ locationPoint = element.LocationPoint(useRoomCalculationPoint);
            if (locationPoint == null)
                return null;

            Transform elementTransform = element.Document.IsLinked ? element.Document.LinkInstance().GetTotalTransform() : Transform.Identity;
            if (!elementTransform.IsIdentity)
                locationPoint = elementTransform.OfPoint(locationPoint);

            foreach (var sp in spaces)
            {
                if (locationPoint.IsInSpace(sp))
                    return sp;
            }

            if (!findClosestIfNotContained)
                return null;

            // 4. If not found, try find closest space in connector directions (for MEP elements)
            var connectors = element.Connectors()?.OrderByDescending(x => x.GetMEPConnectorInfo().IsPrimary).ToList();
            if (connectors != null && connectors.Any())
            {
                foreach (var conn in connectors)
                {
                    XYZ connPoint = conn.Origin;
                    XYZ connDirection = conn.CoordinateSystem.BasisZ;

                    if (!elementTransform.IsIdentity)
                    {
                        connPoint = elementTransform.OfPoint(connPoint);
                        connDirection = elementTransform.OfVector(connDirection);
                    }

                    Space foundClosest = connPoint.FindClosestSpaceInDirection(connDirection, spaces, maxDistance: 3); // 3 feet max distance
                    if (foundClosest != null)
                        return foundClosest;
                }
            }

            // 5. If still not found, try find closest below (negative Z direction)
            Space foundClosestBelow = locationPoint.FindClosestSpaceInDirection(-XYZ.BasisZ, spaces, maxDistance: 10); // 10 feet max distance
            if (foundClosestBelow != null)
                return foundClosestBelow;

            // 6. Not found
            return null;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static bool IsInSpace(this XYZ locationPoint, Space space)
        {
            Transform spaceTransform = space.Document.IsLinked ? m_LinkTransforms[space.Document] : Transform.Identity;

            if (!spaceTransform.IsIdentity)
                locationPoint = spaceTransform.Inverse.OfPoint(locationPoint);

            return space.IsPointInSpace(locationPoint);
        }

        /***************************************************/

        private static Space FindClosestSpaceInDirection(this XYZ startPoint, XYZ direction, IEnumerable<Space> spaces, double maxDistance)
        {
            if (startPoint == null || direction == null || spaces == null || maxDistance <= 0)
                return null;

            // Ensure direction is normalized
            XYZ normalizedDirection = direction.Normalize();

            // Calculate step size and number of steps based on maxDistance
            double stepSize = 1.0; // 1 feet step size
            int maxSteps = (int)Math.Ceiling(maxDistance / stepSize);

            for (int step = 1; step <= maxSteps; step++)
            {
                double distance = step * stepSize;
                if (distance > maxDistance) break;

                XYZ testPoint = startPoint.Add(normalizedDirection.Multiply(distance));

                // Check if this point along the ray is in any space
                foreach (Space space in spaces)
                {
                    if (testPoint.IsInSpace(space))
                        return space;
                }
            }

            return null; // No space found within max distance
        }

        /***************************************************/
        /****              Private field                ****/
        /***************************************************/

        private static Dictionary<Document, Transform> m_LinkTransforms = new Dictionary<Document, Transform>();

        /***************************************************/
    }
}

