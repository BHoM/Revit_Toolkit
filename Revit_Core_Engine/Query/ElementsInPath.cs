/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
        /****                Private fields             ****/
        /***************************************************/

        private static List<Domain> m_CommonDomains;

        /***************************************************/
        /****               Public methods              ****/
        /***************************************************/

        [Description("Returns list of element ids connected to each other between starting and ending element in MEP network. For elements not connected in the system the result is null.")]
        [Input("startingElement", "Starting element in the path of the elements.")]
        [Input("endingElement", "Ending element in the path of the elements.")]
        [Output("elementPath", "Element ids in path between starting and ending element.")]
        public static List<ElementId> ElementsInPath(this Element startingElement, Element endingElement)
        {
            if (startingElement == null || endingElement == null) 
                return null;

            m_CommonDomains = startingElement.CommonDomains(endingElement);
            if (m_CommonDomains == null || !m_CommonDomains.Any())
                return null;

            return ElementPath(startingElement, endingElement, new List<ElementId>());
        }

        /***************************************************/
        /****               Private methods             ****/
        /***************************************************/

        private static List<ElementId> ElementPath(this Element element, Element endingElement, List<ElementId> visitedElementIds)
        {
            if (element.Id.Value() == endingElement.Id.Value())
                return visitedElementIds;

            if (!visitedElementIds.Any())
                visitedElementIds.Add(element.Id);

            List<Element> connectedElements = element.ConnectedNetworkElements();
            List<Element> nextElements = connectedElements.
                Where(x => !visitedElementIds.Select(y => y.Value()).Contains(x.Id.Value())).
                OrderBy(x => x.LocationPoint().DistanceTo(endingElement.LocationPoint())).ToList();

            if (nextElements.Count == 0)
            {
                return null;
            }
            else if (nextElements.Count == 1)
            {
                visitedElementIds.Add(nextElements[0].Id);
                var result = ElementPath(nextElements[0], endingElement, visitedElementIds);

                if (result != null)
                    return result;
            }
            else
            {
                foreach (Element el in nextElements)
                {
                    List<ElementId> childNetwork = visitedElementIds.ToList();
                    childNetwork.Add(el.Id);
                    List<ElementId> result = ElementPath(el, endingElement, childNetwork);

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
            connectedElements = connectedElements.Where(x => x is FamilyInstance || x is Duct || x is FlexDuct || x is Pipe || x is FlexPipe || x is CableTrayConduitBase || x is Wire).ToList();
            List<Element> sameDomainElements = new List<Element>();

            foreach (Element el in connectedElements)
            {
                List<Domain> domains = el.Connectors().Select(x => x.Domain).ToList();
                if (m_CommonDomains.Intersect(domains).Any())
                    sameDomainElements.Add(el);
            }

            return sameDomainElements;
        }

        /***************************************************/

        private static List<Domain> CommonDomains(this Element element1, Element element2)
        {
            List<Domain> el1Domains = element1.Connectors()?.Select(x => x.Domain).ToList();
            List<Domain> el2Domains = element2.Connectors()?.Select(x => x.Domain).ToList();

            if (el1Domains == null || el2Domains == null)
                return null;

            return el1Domains.Intersect(el2Domains).ToList();
        }

        /***************************************************/
    }
}




