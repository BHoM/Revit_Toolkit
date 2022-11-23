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

        [Description("Queries the document and optionally its linked documents for the closest element of input category from the original element." +
                     "\nSearch includes all elements that either have point/curve location or have no location but a valid bounding box in 3D (then centre of bounding box is taken as location).")]
        [Input("document", "The document to query elements and linked documents from.")]
        [Input("originalElement", "The original element to find the closest element from.")]
        [Input("searchRadius", "Optional, search radius in Revit internal unit (ft) to exclude elements outside the range.")]
        [Input("category", "Optional, the Revit BuiltInCategory to filter elements in the closest elements search.")]
        [Input("includeLinks", "If true, elements from linked documents will also be taken into account.")]
        [Output("closestElement", "The closest element from the input element.")]
        public static Element ClosestElement(this Document document, Element originalElement, double searchRadius = Double.MaxValue, BuiltInCategory category = default, bool includeLinks = false)
        {
            if (document == null || originalElement == null || searchRadius == 0)
                return null;
            
            // Find documents to search through
            List<Document> documentsToSearch = new List<Document>(){document};
            if (includeLinks)
            {
                List<ElementId> linkInstancesIds = Core.Query.ElementIdsOfLinkInstances(document);
                if (linkInstancesIds.Any())
                {
                    foreach (ElementId id in linkInstancesIds)
                    {
                        RevitLinkInstance linkInstance = document.GetElement(id) as RevitLinkInstance;
                        Document doc = linkInstance?.GetLinkDocument();
                        // Linked files that are not loaded will be null
                        if (doc != null)
                            documentsToSearch.Add(doc);
                    }
                }
            }

            // Get original element location
            object location = Location(originalElement);
            if (location == null)
            {
                BH.Engine.Base.Compute.RecordError($"Element {originalElement.Id} {originalElement.Name} does not have a valid location.");
                return null;
            }

            // Transform the location to host document coordinate system if element comes from link
            if (originalElement.Document.IsLinked)
                location = TransformLocation(location, originalElement.Document.LinkTransform());

            double smallestDistance = searchRadius;
            Element result = null;
            foreach (Document doc in documentsToSearch)
            {
                // If document is from linked file we need to make sure location is corrected
                Transform transform = null;
                if (doc.IsLinked)
                    transform = doc.LinkTransform();

                // Find elements from category
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                if (category != default)
                    filteredElementCollector.OfCategory(category);

                // Check distances to each element from category 
                foreach (Element checkedElement in filteredElementCollector)
                {
                    object checkedLocation = Location(checkedElement);
                    if (checkedLocation == null)
                        continue;

                    // Transform the location to host document coordinate system
                    checkedLocation = TransformLocation(checkedLocation, transform);

                    // Calculate the distance
                    double distance = Distance(location, checkedLocation);
                    if (!double.IsNaN(distance) && distance < smallestDistance)
                    {
                        result = checkedElement;
                        smallestDistance = distance;
                    }
                }
            }
             
            return result;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static object Location(Element element)
        {
            if (element?.Location is LocationPoint)
                return ((LocationPoint)element.Location).Point;
            else if (element?.Location is LocationCurve)
                return ((LocationCurve)element.Location).Curve;
            else
            {
                BoundingBoxXYZ bbox = element?.get_BoundingBox(null);
                if (bbox != null)
                {
                    BH.Engine.Base.Compute.RecordNote($"Element {element.Id} {element.Name} did not have location point or curve, centre of its bounding box has been used instead to calculate distance.");
                    return (bbox.Min + bbox.Max) / 2;
                }
            }

            return null;
        }

        /***************************************************/

        private static object TransformLocation(object location, Transform transform)
        {
            if (transform == null)
                return location;

            if (location is XYZ)
                return transform.OfPoint((XYZ)location);
            else if (location is Curve)
                return ((Curve)location).CreateTransformed(transform);
            else
                return null;
        }

        /***************************************************/

        private static double Distance(object location1, object location2)
        {
            if (location1 is XYZ && location2 is XYZ)
                return ((XYZ)location1).DistanceTo((XYZ)location2);
            else if (location1 is Curve && location2 is XYZ)
                return ((Curve)location1).Distance((XYZ)location2);
            else if (location1 is XYZ && location2 is Curve)
                return ((Curve)location2).Distance((XYZ)location1);
            else if (location1 is Curve && location2 is Curve)
            {
                IList<ClosestPointsPairBetweenTwoCurves> pairs = new List<ClosestPointsPairBetweenTwoCurves>();
                ((Curve)location1).ComputeClosestPoints((Curve)location2, true, true, false, out pairs);
                return pairs.OrderBy(x => x.Distance).First().Distance;
            }
            else
                return double.NaN;
        }

        /***************************************************/
    }
}
