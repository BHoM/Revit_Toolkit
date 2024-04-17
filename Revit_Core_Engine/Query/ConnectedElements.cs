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
            
            ConnectorSet connSet = null;
            if (element is FamilyInstance)
            {
                connSet = (element as FamilyInstance).MEPModel?.ConnectorManager?.Connectors;
            }
            else if (element is MEPCurve)
            {
                connSet = (element as MEPCurve).ConnectorManager?.Connectors;
            }
            
            if (connSet == null)
            {
                BH.Engine.Base.Compute.RecordError("Input element is not supported by the method. Check if element is a valid MEP object.");
                return null;
            }

            HashSet<int> connectedElementIds = new HashSet<int>();
            foreach (Connector conn in connSet)
            {
                ConnectorSet allRefs = conn.AllRefs;
                foreach (Connector refConn in allRefs)
                {
                    Element el = refConn?.Owner;
                    if (el != null)
                        connectedElementIds.Add(el.Id.IntegerValue);
                }
            }

            Document doc = element.Document;

            return connectedElementIds.Select(x => doc.GetElement(new ElementId(x))).ToList();
        }

        /***************************************************/
    }
}

