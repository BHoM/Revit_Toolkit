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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Queries a Family Instance that contains MEP connectors for their locations.")]
        [Input("mepInstance", "Revit family instance to be queried for their MEP connectors.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("mepConnectorsLocation", "The list of points representing the connector's location points.")]
        public static List<BH.oM.MEP.System.Fittings.Connector> GetConnectors(this FamilyInstance mepInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            if (mepInstance == null)
            {
                BH.Engine.Base.Compute.RecordError("Input mepInstance is null or empty.");
                return null;
            }

            MEPModel mepModel = mepInstance.MEPModel;

            if (mepModel == null)
            {
                BH.Engine.Base.Compute.RecordError("The input fitting does not have any connectors.");
                return null;
            }

            ConnectorManager connectorManager = mepModel.ConnectorManager;
            if (connectorManager == null)
            {
                BH.Engine.Base.Compute.RecordError("The input fitting does not have any connectors.");
                return null;
            }
            List<BH.oM.MEP.System.Fittings.Connector> fittingConnectorSet = new List<oM.MEP.System.Fittings.Connector>();
            ConnectorSet connectorSet = connectorManager.Connectors;
            List<Connector> connectors = connectorSet.Cast<Connector>().ToList();

            BH.oM.MEP.System.Fittings.Connector x;

            foreach (var connector in connectors)
            {

                x = new BH.oM.MEP.System.Fittings.Connector()
                {
                    Location = connector.Origin.PointFromRevit(),
                    FlowRate = connector.Flow,
                    Angle = connector.Angle,
                    IsConnected = connector.IsConnected
                };

                switch (connector.Shape)
                {
                    case ConnectorProfileType.Invalid:
                        break;
                    case ConnectorProfileType.Round:
                        x.Diameter = connector.Radius * 2;
                        break;
                    case ConnectorProfileType.Rectangular:
                    case ConnectorProfileType.Oval:
                        x.Height = connector.Height;
                        x.Width = connector.Width;
                        break;
                }

                switch (connector.Direction)
                {
                    case FlowDirectionType.Bidirectional:
                        x.FlowDirection = oM.MEP.Enums.FlowDirection.Bidirectional;
                        break;
                    case FlowDirectionType.In:
                        x.FlowDirection = oM.MEP.Enums.FlowDirection.In;
                        break;
                    case FlowDirectionType.Out:
                        x.FlowDirection = oM.MEP.Enums.FlowDirection.Out;
                        break;
                }
                fittingConnectorSet.Add(x);

            }

            return fittingConnectorSet;

        }

        /***************************************************/
    }
}


