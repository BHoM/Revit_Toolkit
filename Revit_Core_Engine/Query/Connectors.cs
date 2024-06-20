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

        [Description("Returns connectors of the MEP element. MEPCurve connectors are sorted by distance from the startpoint, FamilyInstace connectors by IsPrimary property.")]
        [Input("element", "MEP element to get the connectors from.")]
        [Output("connectors", "MEP Element connectors.")]
        public static List<Connector> Connectors(this Element element)
        {
            return SortedConnectors(element as dynamic);
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static List<Connector> SortedConnectors(this MEPCurve mepCurve)
        {
            XYZ startPoint = (mepCurve.Location as LocationCurve).Curve.GetEndPoint(0);
            ConnectorSet connSet = mepCurve.ConnectorManager?.Connectors;
            List<Connector> connList = new List<Connector>();

            if (connSet == null)
                return connList;

            foreach (Connector conn in connSet)
                connList.Add(conn);

            connList = connList.OrderBy(x => x.Origin.DistanceTo(startPoint)).ToList();

            return connList;
        }

        /***************************************************/

        private static List<Connector> SortedConnectors(this FamilyInstance familyInstance)
        {
            ConnectorSet connSet = familyInstance.MEPModel?.ConnectorManager?.Connectors;
            List<Connector> connList = new List<Connector>();

            if (connSet == null)
                return connList;

            foreach (Connector conn in connSet)
                connList.Add(conn);

            return connList;
        }

        /***************************************************/

        private static List<Connector> SortedConnectors(this Element element)
        {
            BH.Engine.Base.Compute.RecordError("Input element is not supported by the connector query method. Check if element is a valid MEP object.");
            return null;
        }

        /***************************************************/
    }
}

