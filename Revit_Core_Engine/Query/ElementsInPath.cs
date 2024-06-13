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
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
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

        [Description("Returns list of elements connected to each other between starting and ending element in MEP network. For elements not connected in the system the result is null.")]
        [Input("startingElement", "Starting element in the path of the elements.")]
        [Input("endingElement", "Ending element in the path of the elements.")]
        [Output("elementPath", "Elements in path between starting and ending element.")]
        public static List<ElementId> ElementsInPath(this Element startingElement, Element endingElement)
        {
            if (startingElement == null || endingElement == null) 
                return null;

            Document doc = startingElement.Document;
            List<ElementId> result = ElementPath(startingElement, endingElement, new HashSet<int>()).Select(x => new ElementId(x)).ToList();

            return result;
        }

        /***************************************************/
        /****               Private methods             ****/
        /***************************************************/

        private static HashSet<int> ElementPath(this Element element, Element endingElement, HashSet<int> visitedElementIds)
        {
            if (element.Id.IntegerValue == endingElement.Id.IntegerValue)
                return visitedElementIds;

            if (!visitedElementIds.Any())
                visitedElementIds.Add(element.Id.IntegerValue);

            List<Element> connectedElements = element.ConnectedNetworkElements();
            List<Element> nextElements = connectedElements.Where(x => !visitedElementIds.Contains(x.Id.IntegerValue)).OrderBy(x => x.LocationPoint().DistanceTo(endingElement.LocationPoint())).ToList();

            if (nextElements.Count == 0)
            {
                return null;
            }
            else if (nextElements.Count == 1)
            {
                visitedElementIds.Add(nextElements[0].Id.IntegerValue);
                var result = ElementPath(nextElements[0], endingElement, visitedElementIds);

                if (result != null)
                    return result;
            }
            else
            {
                foreach (Element el in nextElements)
                {
                    HashSet<int> childNetwork = visitedElementIds.ToHashSet();
                    childNetwork.Add(el.Id.IntegerValue);
                    HashSet<int> result = ElementPath(el, endingElement, childNetwork);

                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /***************************************************/

        private static List<Element> ConnectedNetworkElements(this Element element)
        {
            List<Element> connectedElements = element.ConnectedElements();
            return connectedElements.Where(x => x is FamilyInstance || x is Duct || x is FlexDuct || x is Pipe || x is FlexPipe || x is CableTrayConduitBase || x is Wire).ToList();
        }


        /***************************************************/

        private static XYZ LocationPoint(this Element element)
        {
            Location location = element.Location;

            if (location is LocationPoint locationPoint)
                return locationPoint.Point;
            else if (location is LocationCurve locationCurve)
                return locationCurve.Curve.Evaluate(0.5, true);

            return null;
        }

        /***************************************************/
    }
}



