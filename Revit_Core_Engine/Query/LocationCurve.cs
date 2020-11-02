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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Mechanical;
using BH.Engine.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ICurve LocationCurve(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            ICurve curve = null;

            if (typeof(Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.LocationCurveColumn(settings);
            else if (typeof(IFramingElement).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.LocationCurveFraming(settings);

            return curve;
        }

        /***************************************************/
        
        public static ICurve LocationCurveFraming(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            ICurve curve = (familyInstance.Location as LocationCurve)?.Curve?.IFromRevit();
            if (curve == null || (!(curve is NurbsCurve) && curve.ILength() <= settings.DistanceTolerance))
            {
                familyInstance.FramingCurveNotFoundWarning();
                return null;
            }

            BH.oM.Geometry.Line line = curve as BH.oM.Geometry.Line;
            if (line == null)
            {
                familyInstance.NonLinearFramingOffsetWarning();
                return curve;
            }

            Transform transform = familyInstance.GetTotalTransform();
            Vector dir = line.Direction();
            BH.oM.Geometry.Plane startPlane = new oM.Geometry.Plane { Origin = line.Start, Normal = dir };
            BH.oM.Geometry.Plane endPlane = new oM.Geometry.Plane { Origin = line.End, Normal = dir };
            BH.oM.Geometry.Line transformedLine = BH.Engine.Geometry.Create.Line(transform.Origin.PointFromRevit(), transform.BasisX.VectorFromRevit());
            line = new BH.oM.Geometry.Line { Start = transformedLine.PlaneIntersection(startPlane, true), End = transformedLine.PlaneIntersection(endPlane, true) };

            double startExtension = familyInstance.LookupParameterDouble(BuiltInParameter.START_EXTENSION);
            if (!double.IsNaN(startExtension))
                line.Start = line.Start - dir * startExtension;

            double endExtension = familyInstance.LookupParameterDouble(BuiltInParameter.END_EXTENSION);
            if (!double.IsNaN(endExtension))
                line.End = line.End + dir * endExtension;

            return line;
        }

        /***************************************************/

        public static BH.oM.Geometry.Line LocationCurveColumn(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            BH.oM.Geometry.Line curve = null;

            if (familyInstance.IsSlantedColumn)
                curve = (familyInstance.Location as LocationCurve).Curve.IFromRevit() as BH.oM.Geometry.Line;
            else
            {
                Parameter baseLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                if (baseLevelParam != null)
                {
                    Parameter topLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                    Parameter baseOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                    Parameter topOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                    double baseLevel = (familyInstance.Document.GetElement(baseLevelParam.AsElementId()) as Level).ProjectElevation;
                    double topLevel = (familyInstance.Document.GetElement(topLevelParam.AsElementId()) as Level).ProjectElevation;
                    double baseOffset = baseOffsetParam.AsDouble();
                    double topOffset = topOffsetParam.AsDouble();

                    XYZ loc = (familyInstance.Location as LocationPoint).Point;
                    XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                    XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                    curve = new oM.Geometry.Line { Start = baseNode.PointFromRevit(), End = topNode.PointFromRevit() };
                }
            }

            if (curve != null)
            {
                Output<double, double> extensions = familyInstance.ColumnExtensions(settings);
                double startExtension = extensions.Item1;
                double endExtension = extensions.Item2;

                if (Math.Abs(startExtension) > settings.DistanceTolerance || Math.Abs(endExtension) > settings.DistanceTolerance)
                {
                    Vector direction = curve.Direction();
                    curve = new oM.Geometry.Line { Start = curve.Start - direction * startExtension, End = curve.End + direction * endExtension };
                }
            }

            if (curve == null)
                familyInstance.FramingCurveNotFoundWarning();

            return curve;
        }

        /***************************************************/
        
        public static List<BH.oM.Geometry.Line> LocationCurveMEP (this MEPCurve mepCurve, RevitSettings settings = null)
        {
            //List<BH.oM.Geometry.Line> result = new List<BH.oM.Geometry.Line>();
            settings = settings.DefaultIfNull();
            
            // Determine start and end points, which if the MEP linear object is connected to
            // a fitting then uses the fitting origin as either start/end
            List<BH.oM.Geometry.Point> allPoints = new List<BH.oM.Geometry.Point>();
            
            // Get connectors
            ConnectorManager connectorManager = mepCurve.ConnectorManager;
            ConnectorSet connectorSet = connectorManager.Connectors;
            //List<bool> endPointsConnected = new List<bool>();
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
                        //at each connector get the connector refs to give access to fitting
                        ConnectorSet cSet = c.AllRefs;
                        foreach (Connector con in cSet)
                        {
                            //check if connector owner is not the original MEP linear object
                            if (con.Owner.Id != mepCurve.Id)
                            {
                                // todo new idea
                                // if sequence is composed of two connectors then try to find midpoint distance but,
                                // if connector is too long it may suggest to be a bend/elbow
                                // and therefore should be queryied and instead of forming a single line for paths,
                                // it forms 2, 1/4 connector 3/4 line
                                // it should connect the back and forth precisely
                                // do 2 levels only
                                
                                // todo continue new idea
                                // if sequence starts with a T that has direct connector attached,
                                // do nothing? pick this T location
                                
                                
                                
                                //in some cases there may be a chain of fittings connected to each other,
                                //to capture these we have to dive deep into at least 2 levels of nested conn
                                //strategy is to check for it in only one of the endpoints of mep curve,
                                //so as to avoid duplicates from another mep curve
                                
                                //this is 1 level
                                //get fitting location
                                Location location = con.Owner.Location;
                                LocationPoint locationPoint = location as LocationPoint;

                                // routine to check 2 level
                                if (c.Id == 0)
                                {
                                    if (con.Owner is FamilyInstance fittingInstance)
                                    {
                                        MEPModel fittingMEP = fittingInstance.MEPModel;
                                        ConnectorManager fittingManager = fittingMEP.ConnectorManager;
                                        ConnectorSet fittingSet = fittingManager.Connectors;
                                        foreach (Connector cFitting in fittingSet)
                                        {
                                            //check each connector to find the next fitting
                                            ConnectorSet subFittingSet = cFitting.AllRefs;
                                            foreach (Connector cSub in subFittingSet)
                                            {
                                                //check if connector owner ins't the original cable tray
                                                //and if its a FamilyInstance (fitting)
                                                if (cSub.Owner.Id != fittingInstance.Id &&
                                                    cSub.Owner.GetType() == typeof(FamilyInstance))
                                                {
                                                    //mark new location to be the next fitting
                                                    location = cSub.Owner.Location;
                                                    locationPoint = location as LocationPoint;
                                                }
                                            }
                                        }
                                    }
                                }

                                //in some cases MEP linear objects connect without fittings,
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
                    }
                    //if not connected at all then just use own connector origin
                    else
                    {
                        point = BH.Engine.Geometry.Create.Point(
                            Math.Round(c.Origin.PointFromRevit().X, 4),
                            Math.Round(c.Origin.PointFromRevit().Y, 4), 
                            Math.Round(c.Origin.PointFromRevit().Z, 4));
                        endPoints.Add(point);
                        allPoints.Add(point);
                    }
                }
                //if MEP linear object has connections that don't take fitting,
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
            
            //split MEP linear object if there was any other MEP linear object connected to it without connector
            BH.oM.Geometry.Line line = BH.Engine.Geometry.Create.Line(endPoints[0], endPoints[1]);
            List<BH.oM.Geometry.Line> result = new List<BH.oM.Geometry.Line>();
            if (midPoints.Any())
            {
                result = line.SplitAtPoints(midPoints);
            }
            else
            {
                result.Add(line);
            }

            return result;
        }
        
        /***************************************************/
    }
}

