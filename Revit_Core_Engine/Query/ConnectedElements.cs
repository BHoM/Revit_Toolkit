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

        [Description("Returns elements connected directly to the MEP element connectors. Works for Family Instances and MEPCurve objects.")]
        [Input("element", "MEP element to get the connected elements for.")]
        [Output("connectedElements", "List of the elements that are connected to the input element.")]
        public static List<Element> ConnectedElements(this Element element)
        {
            if (element == null)
            {
                BH.Engine.Base.Compute.RecordError("Querying the connected elements failed because the input element cannot be null.");
                return null;
            }
            
            List<Connector> connectors = element.Connectors();
            HashSet<ElementId> connectedElements = new HashSet<ElementId>();

            foreach (Connector conn in connectors)
            {
                ConnectorSet allRefs = conn.AllRefs;
                foreach (Connector refConn in allRefs)
                {
                    Element el = refConn?.Owner;
                    if (el != null && el.Id != element.Id)
                        connectedElements.Add(el.Id);
                }
            }

            Document doc = element.Document;

            return connectedElements.Select(x => doc.GetElement(x)).ToList();
        }

        /***************************************************/
    }
}


