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
using BH.oM.MEP.System.ConnectionProperties;
using BH.oM.MEP.System;
using BH.oM.Reflection.Attributes;
using BH.oM.Geometry;


namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit Cable Tray to BHoM cable trays.")]
        [Input("revitCableTray", "Revit Cable Tray to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("cableTrays", "BHoM cable tray objects converted from a Revit cable tray elements.")]
        public static List<BH.oM.MEP.System.CableTray> CableTrayFromRevit(this Autodesk.Revit.DB.Electrical.CableTray revitCableTray, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Reuse a BHoM cable tray from refObjects if it has been converted before
            List<BH.oM.MEP.System.CableTray> bhomCableTrays = refObjects.GetValues<BH.oM.MEP.System.CableTray>(revitCableTray.Id);
            if (bhomCableTrays != null)
            {
                return bhomCableTrays;
            }
            else
            {
                bhomCableTrays = new List<BH.oM.MEP.System.CableTray>();
            }

            // Section properties
            BH.oM.MEP.System.SectionProperties.CableTraySectionProperty sectionProperty = BH.Revit.Engine.Core.Query.CableTraySectionProperty(revitCableTray, settings);

            // Orientation angle
            double orientationAngle = revitCableTray.OrientationAngle(settings);

            bool isStartConnected = false;
            bool isEndConnected = false;
            List<BH.oM.Geometry.Line> queried = Query.LocationCurveMEP(revitCableTray, out isStartConnected, out isEndConnected, settings);
            Vector revitCableTrayVector = BH.Engine.Geometry.Modify.RoundCoordinates(
                VectorFromRevit((revitCableTray.Location as LocationCurve).Curve.GetEndPoint(0) -
                                (revitCableTray.Location as LocationCurve).Curve.GetEndPoint(1)),4).Normalise();
            if (queried.Count > 2)
            {
                //required to assert connector property later
                queried = MatchRevitOrder(queried, revitCableTray);
            }
            
            for (int i = 0; i < queried.Count; i++)
            {
                BH.oM.Geometry.Line segment = queried[i];
                BH.oM.MEP.System.CableTray thisSegment = new CableTray
                {
                    StartPoint = segment.StartPoint(),
                    EndPoint = segment.EndPoint(),
                    SectionProperty = sectionProperty,
                    ConnectionProperty = new CableTrayConnectionProperty(),
                    OrientationAngle = orientationAngle
                };
                Vector bhomCableTrayVector = BH.Engine.Geometry.Modify.RoundCoordinates((thisSegment.StartPoint - thisSegment.EndPoint),4).Normalise();

                if (queried.Count > 1)
                {
                    if (i == 0)
                    {
                        if (revitCableTrayVector == bhomCableTrayVector)
                        {
                            thisSegment.ConnectionProperty.IsStartConnected = isStartConnected;
                            thisSegment.ConnectionProperty.IsEndConnected = true;
                        }
                        else
                        {
                            thisSegment.ConnectionProperty.IsStartConnected = true;
                            thisSegment.ConnectionProperty.IsEndConnected = isStartConnected;   
                        }
                    }
                    else if (i == queried.Count - 1)
                    {
                        if (revitCableTrayVector == bhomCableTrayVector)
                        {
                            thisSegment.ConnectionProperty.IsStartConnected = true;
                            thisSegment.ConnectionProperty.IsEndConnected = isEndConnected;
                        }
                        else
                        {
                            thisSegment.ConnectionProperty.IsStartConnected = isEndConnected;
                            thisSegment.ConnectionProperty.IsEndConnected = true;   
                        }
                    }
                    else
                    {
                        thisSegment.ConnectionProperty.IsStartConnected = true;
                        thisSegment.ConnectionProperty.IsEndConnected = true;
                    }
                }
                else
                {
                    if (revitCableTrayVector == bhomCableTrayVector)
                    {
                        thisSegment.ConnectionProperty.IsStartConnected = isStartConnected;
                        thisSegment.ConnectionProperty.IsEndConnected = isEndConnected;
                    }
                    else
                    {
                        thisSegment.ConnectionProperty.IsStartConnected = isEndConnected;
                        thisSegment.ConnectionProperty.IsEndConnected = isStartConnected;   
                    }
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
        /****              Private Methods              ****/
        /***************************************************/
        
        [Description("Re-orders converted BHoM Line list to match the direction from the reference MEPCurve.")]
        private static List<BH.oM.Geometry.Line> MatchRevitOrder(List<BH.oM.Geometry.Line> linesToMatch, MEPCurve reference)
        {
            LocationCurve locationCurve = reference.Location as LocationCurve;
            Curve curve = locationCurve.Curve;
            BH.oM.Geometry.Point referenceStart = BH.Revit.Engine.Core.Convert.PointFromRevit(curve.GetEndPoint(0));

            List<BH.oM.Geometry.Line> result = linesToMatch.OrderBy(x => x.Start.Distance(referenceStart)).ToList();

            return result;
        }
        
        /***************************************************/
    }
}
