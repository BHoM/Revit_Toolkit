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

        [Description("Returns list of elements connected to each other between starting and ending element in MEP network. For elements not connected in the system the result is null")]
        [Input("startingElement", "Starting element in the path of the elements.")]
        [Input("endingElement", "Ending element in the path of the elements.")]
        [Output("elementPath", "Elements in path between starting and ending element.")]
        public static List<Element> ElementsInPath(this Element startingElement, Element endingElement)
        {
            if (startingElement == null || endingElement == null) 
                return null;

            return ElementPath(startingElement, endingElement, new List<Element>());
        }

        /***************************************************/
        /****               Private methods             ****/
        /***************************************************/

        private static List<Element> ElementPath(this Element element, Element endingElement, List<Element> visitedElements)
        {
            if (element.Id.IntegerValue == endingElement.Id.IntegerValue)
                return visitedElements;

            if (!visitedElements.Any())
                visitedElements.Add(element);

            List<Element> connectedElements = element.ConnectedNetworkElements();
            List<Element> nextElements = connectedElements.Where(x => !visitedElements.Select(y => y.Id.IntegerValue).Contains(x.Id.IntegerValue)).ToList();

            if (nextElements.Count == 0)
            {
                return null;
            }
            else if (nextElements.Count == 1)
            {
                visitedElements.Add(nextElements[0]);
                var result = ElementPath(nextElements[0], endingElement, visitedElements);

                if (result != null)
                    return result;
            }
            else
            {
                foreach (Element el in nextElements)
                {
                    List<Element> childNetwork = visitedElements.ToList();
                    childNetwork.Add(el);
                    List<Element> result = ElementPath(el, endingElement, childNetwork);

                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        /***************************************************/

        private static List<Element> ConnectedNetworkElements(this Element element)
        {
            List<Element> connectedElemnets = element.ConnectedElements();
            return connectedElemnets.Where(x => x is FamilyInstance || x is Duct || x is FlexDuct || x is Pipe || x is FlexPipe || x is CableTrayConduitBase || x is Wire).ToList();
        }

        /***************************************************/
    }
}



