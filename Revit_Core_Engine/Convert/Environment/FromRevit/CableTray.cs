/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.MEP.ConnectionProperties;
using BH.oM.MEP.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Geometry;


namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit Cable Tray to a BHoM Cable Tray.")]
        [Input("revitCableTray", "Revit Cable Tray to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects",
            "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("cableTray", "BHoM cable tray object converted from a Revit cable tray element.")]
        public static List<BH.oM.MEP.Elements.CableTray> CableTrayFromRevit(
            this Autodesk.Revit.DB.Electrical.CableTray revitCableTray, RevitSettings settings = null,
            Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM cable tray from refObjects if it has been converted before
            List<BH.oM.MEP.Elements.CableTray> bhomCableTrays =
                refObjects.GetValues<BH.oM.MEP.Elements.CableTray>(revitCableTray.Id);
            if (bhomCableTrays != null)
            {
                return bhomCableTrays;
            }
            else
            {
                bhomCableTrays = new List<BH.oM.MEP.Elements.CableTray>();
            }

            // Section properties
            BH.oM.MEP.SectionProperties.CableTraySectionProperty sectionProperty =
                BH.Revit.Engine.Core.Query.CableTraySectionProperty(revitCableTray, settings);

            // Orientation angle
            double orientationAngle = revitCableTray.OrientationAngle(settings);

            /*// Determine start and end points, which if the cable tray is connected to
            // a fitting then uses the fitting origin as either start/end
            List<BH.oM.Geometry.Point> allPoints = new List<BH.oM.Geometry.Point>();
            // Get connectors
            ConnectorManager connectorManager = revitCableTray.ConnectorManager;
            ConnectorSet connectorSet = connectorManager.Connectors;
            List<Connector> connectors = connectorSet.Cast<Connector>().ToList();
            List<bool> endPointsConnected = new List<bool>();
            List<BH.oM.Geometry.Point> endPoints = new List<BH.oM.Geometry.Point>();
            List<BH.oM.Geometry.Point> midPoints = new List<BH.oM.Geometry.Point>();
            foreach (Autodesk.Revit.DB.Connector c in connectorSet)
            {
                //calculate end points, that are in fact the origin of its fittings if connected
                BH.oM.Geometry.Point point = null;
                if (c.ConnectorType == ConnectorType.End)
                {
                    if (c.IsConnected)
                    {
                        bool isConnected = true;

                        //at each connector get the connector refs to give access to fitting
                        ConnectorSet cSet = c.AllRefs;
                        foreach (Connector con in cSet)
                        {
                            //check if connector owner is not the original cable tray
                            if (con.Owner.Id != revitCableTray.Id)
                            {
                                //get fitting location
                                Location location = con.Owner.Location;
                                LocationPoint locationPoint = location as LocationPoint;

                                //in some cases cable trays connect without fittings,
                                //if thats the case then desired point will be its own connector origin
                                if (locationPoint == null)
                                {
                                    //rounding to 4 decimal digits due to precision issues when extracting vector directions.
                                    point = BH.Engine.Geometry.Create.Point(
                                        Math.Round(c.Origin.PointFromRevit().X, 4),
                                        Math.Round(c.Origin.PointFromRevit().Y, 4),
                                        Math.Round(c.Origin.PointFromRevit().Z, 4));
                                    endPoints.Add(point);
                                }
                                //else use the fitting location point
                                else
                                {
                                    point = BH.Engine.Geometry.Create.Point(
                                        Math.Round(locationPoint.Point.PointFromRevit().X, 4),
                                        Math.Round(locationPoint.Point.PointFromRevit().Y, 4),
                                        Math.Round(locationPoint.Point.PointFromRevit().Z, 4));
                                    endPoints.Add(point);
                                }
                            }
                        }

                        endPointsConnected.Add(isConnected);
                    }
                    //if not connected at all then just use own connector origin
                    else
                    {
                        endPointsConnected.Add(false);
                        point = BH.Engine.Geometry.Create.Point(
                            Math.Round(c.Origin.PointFromRevit().X, 4),
                            Math.Round(c.Origin.PointFromRevit().Y, 4), 
                            Math.Round(c.Origin.PointFromRevit().Z, 4));
                        endPoints.Add(point);
                        allPoints.Add(point);
                    }
                }
                //if cable tray has connections that don't take fitting,
                //then we need to store the point to later split the curve
                else
                {
                    point = BH.Engine.Geometry.Create.Point(
                        Math.Round(c.Origin.PointFromRevit().X, 4),
                        Math.Round(c.Origin.PointFromRevit().Y, 4), 
                        Math.Round(c.Origin.PointFromRevit().Z, 4));
                    midPoints.Add(point);
                }
            }

            CableTrayConnectionProperty connectionProperty = new CableTrayConnectionProperty();
            connectionProperty.IsStartConnected = endPointsConnected[0];
            connectionProperty.IsEndConnected = endPointsConnected[1];
            
            //split cable tray if there was any other cable tray connected to it without connector
            BH.oM.Geometry.Line line = BH.Engine.Geometry.Create.Line(endPoints[0], endPoints[1]);
            List<BH.oM.Geometry.Line> segments = new List<Line>();
            if (midPoints.Any())
            {
                segments = line.SplitAtPoints(midPoints);
            }
            else
            {
                segments.Add(line);
            }

            foreach (BH.oM.Geometry.Line segment in segments)
            {
                CableTrayConnectionProperty newConnectionProperty = new CableTrayConnectionProperty();
                if (segment.Start == endPoints[0])
                {
                    newConnectionProperty.IsStartConnected = endPointsConnected[0];
                    newConnectionProperty.IsEndConnected = true; //always true since line has been split
                }
                else if (segment.End == endPoints[0])
                {
                    newConnectionProperty.IsEndConnected = endPointsConnected[0];
                    newConnectionProperty.IsStartConnected = true; //always true since line has been split
                }
                else if (segment.Start == endPoints[1])
                {
                    newConnectionProperty.IsStartConnected = endPointsConnected[1];
                    newConnectionProperty.IsEndConnected = true; //always true since line has been split
                }
                else if (segment.End == endPoints[1])
                {
                    newConnectionProperty.IsEndConnected = endPointsConnected[1];
                    newConnectionProperty.IsStartConnected = true; //always true since line has been split
                }
                else
                {
                    newConnectionProperty.IsStartConnected = true; //always true since line has been split
                    newConnectionProperty.IsEndConnected = true; //always true since line has been split
                }

                BH.oM.MEP.Elements.CableTray thisSegment = new CableTray
                {
                    StartNode = (Node) segment.StartPoint(),
                    EndNode = (Node) segment.EndPoint(),
                    SectionProperty = sectionProperty,
                    ConnectionProperty = newConnectionProperty,
                    OrientationAngle = orientationAngle
                };

                //Set identifiers, parameters & custom data
                thisSegment.SetIdentifiers(revitCableTray);
                thisSegment.CopyParameters(revitCableTray, settings.ParameterSettings);
                thisSegment.SetProperties(revitCableTray, settings.ParameterSettings);
                bhomCableTrays.Add(thisSegment);
            }*/

            List<BH.oM.Geometry.Line> queryied = Query.LocationCurveMEP(revitCableTray, settings);
            List<bool> isConnected = Query.IsEndPointsConnected(revitCableTray);

            if (revitCableTray.Id.IntegerValue == 1392148)
            {
                //debug
            }

            
            
            for (int i = 0; i < queryied.Count; i++)
            {
                BH.oM.Geometry.Line segment = queryied[i];
                BH.oM.MEP.Elements.CableTray thisSegment = new CableTray
                {
                    StartNode = (Node) segment.StartPoint(),
                    EndNode = (Node) segment.EndPoint(),
                    SectionProperty = sectionProperty,
                    ConnectionProperty = new CableTrayConnectionProperty(),
                    OrientationAngle = orientationAngle
                };
                
                thisSegment.ConnectionProperty.StartNode = thisSegment.StartNode;
                thisSegment.ConnectionProperty.EndNode = thisSegment.EndNode;

                //sets is connected to cable tray
                if (queryied.Count > 1)
                {
                    //if first
                    if (i == 0)
                    {
                        thisSegment.ConnectionProperty.IsStartConnected = isConnected[0];
                        thisSegment.ConnectionProperty.IsEndConnected = true;
                    }
                    //if last
                    else if(i == (queryied.Count-1))
                    {
                        thisSegment.ConnectionProperty.IsStartConnected = true;
                        thisSegment.ConnectionProperty.IsEndConnected = isConnected[1];
                    }
                    //if anything in betweeen
                    else
                    {
                        thisSegment.ConnectionProperty.IsStartConnected = true;
                        thisSegment.ConnectionProperty.IsEndConnected = true;
                    }
                }
                else
                {
                    thisSegment.ConnectionProperty.IsStartConnected = isConnected[0];
                    thisSegment.ConnectionProperty.IsEndConnected = isConnected[1];
                }


                //Set identifiers, parameters & custom data
                thisSegment.SetIdentifiers(revitCableTray);
                thisSegment.CopyParameters(revitCableTray, settings.ParameterSettings);
                thisSegment.SetProperties(revitCableTray, settings.ParameterSettings);
                bhomCableTrays.Add(thisSegment);
            }

            refObjects.AddOrReplace(revitCableTray.Id, bhomCableTrays);
            return bhomCableTrays;
        }

        /***************************************************/
    }
}