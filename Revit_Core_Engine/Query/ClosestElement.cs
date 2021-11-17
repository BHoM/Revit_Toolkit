/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Constructions;
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

        [Description("Queries the document and its linked documents for the closest element of input category from the original element. Search includes only elements that are XYZ point based, not line-based.")]
        [Input("document", "The document to query elements and linked documents from.")]
        [Input("originalElement", "The original element to find closest elements from.")]
        [Input("searchRadius", "Optional, search radius in Revit internal unit (ft) to exclude elements outside the range.")]
        [Input("builtInCategory", "Optional, the Revit BuiltInCategory to filter elements in the closest elements search.")]
        [Input("includeLinkInstance", "Optional, whether or not to also search for elements in linked documents.")]
        [Output("closestElement", "The closest element from the input element.")]
        public static Element ClosestElement(this Document document, Element originalElement, double searchRadius = Double.MaxValue, BuiltInCategory builtInCategory = default, bool includeLinkInstance = false)
        {
            List<Document> documentsToSearch = new List<Document>(){document};
            if (includeLinkInstance)
            {
                List<ElementId> linkInstancesIds = Core.Query.ElementIdsOfLinkInstances(document);
                if (linkInstancesIds.Any())
                {
                    foreach (ElementId id in linkInstancesIds)
                    {
                        RevitLinkInstance linkInstance = document.GetElement(id) as RevitLinkInstance;
                        documentsToSearch.Add(linkInstance.GetLinkDocument());
                    }
                }
            }

            // get original element location
            Location location = originalElement.Location;
            if (!(location is LocationPoint))
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Original element to search for closest element is not point XYZ based. Location of type {0} is not implemented in this search.",location.GetType()));
                return null;
            }
            XYZ point = (location as LocationPoint).Point;
            
            double smallestDistance = searchRadius;
            Element result = null;
            
            foreach (Document doc in documentsToSearch)
            {
                FilteredElementCollector filteredElementCollector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
                if (builtInCategory != default)
                {
                    filteredElementCollector.OfCategory(builtInCategory).Where(x=>x.HasLocation(true,false));
                }
                else
                {
                    filteredElementCollector.Where(x => x.HasLocation());
                }
                
                if (filteredElementCollector.Any())
                {
                    // check distances to each element from category 
                    foreach (Element searchedElement in filteredElementCollector)
                    {
                        Location searchedLocation = searchedElement.Location;
                        if (searchedLocation is LocationPoint)
                        {
                            XYZ searchedPoint = (searchedLocation as LocationPoint).Point;
                            double distance = searchedPoint.DistanceTo(point);
                            if (distance < smallestDistance)
                            {
                                result = searchedElement;
                                smallestDistance = distance;
                            }
                        }
                    }
                }
            }

            if (result == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Could not find any element within {0} feet near original element of id {1}",searchRadius.ToString(), originalElement.Id));
                return null;
            }
            
            return result;
        }
        
        /***************************************************/
    }
}

