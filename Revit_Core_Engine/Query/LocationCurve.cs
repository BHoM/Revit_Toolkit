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
using Tolerance = BH.oM.Adapters.Revit.Tolerance;


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
        
        public static List<BH.oM.Geometry.Line> LocationCurveMEP(this MEPCurve mepCurve, RevitSettings settings = null)
        {
            //List<BH.oM.Geometry.Line> result = new List<BH.oM.Geometry.Line>();
            settings = settings.DefaultIfNull();
            
            // Determine start and end points, which if the MEP linear object is connected to
            // a fitting then uses the fitting origin as either start/end
            List<BH.oM.Geometry.Point> allPoints = new List<BH.oM.Geometry.Point>();
            
            var tolerance = BH.oM.Adapters.Revit.Tolerance.Vertex.FromSI(UnitType.UT_Length);
            
            BH.oM.Geometry.Line extraLine1 = null;
            BH.oM.Geometry.Line extraLine2 = null;

            List<BH.oM.Geometry.Point> endPoints = new List<BH.oM.Geometry.Point>();
            List<BH.oM.Geometry.Point> midPoints = new List<BH.oM.Geometry.Point>();

            // Get connectors
            ConnectorManager connectorManager = mepCurve.ConnectorManager;
            ConnectorSet connectorSet = connectorManager.Connectors;
            List<Connector> c1Connectors = connectorSet.Cast<Connector>().ToList();
            for (int i = 0; i < c1Connectors.Count; i++)
            {
                BH.oM.Geometry.Point locationToSnap = null;
                BH.oM.Geometry.Point locationFromSnap = null;
                
                Connector c1 = c1Connectors[i];
                if (c1.ConnectorType == ConnectorType.End)
                {
                    locationFromSnap = BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin),4);
                    if (c1.IsConnected)
                    {
                        List<Connector> c2Connectors = (c1.AllRefs).Cast<Connector>().ToList();
                        for (int j = 0; j < c2Connectors.Count; j++)
                        {
                            Connector c2 = c2Connectors[j];
                            if (c2.Owner.Id != c1.Owner.Id)
                            {
                                if (!(c2.Owner is MEPCurve))
                                {
                                    FamilyInstance familyInstance1 = c2.Owner as FamilyInstance;
                                    ConnectorManager connectorManager1 = familyInstance1.MEPModel.ConnectorManager;
                                    List<Connector> c3Connectors = (connectorManager1.Connectors).Cast<Connector>().ToList();
                                    if (c3Connectors.Count == 2)
                                    {
                                        for (int k = 0; k < c3Connectors.Count; k++)
                                        {
                                            Connector c3 = c3Connectors[k];
                                            if (c3.Origin.DistanceTo(c1.Origin) > tolerance)
                                            {
                                                if (c3.IsConnected)
                                                {
                                                    List<Connector> c4Connectors = (c3.AllRefs).Cast<Connector>().ToList();
                                                    for (int l = 0; l < c4Connectors.Count; l++)
                                                    {
                                                        Connector c4 = c4Connectors[l];
                                                        if (c4.Owner.Id != c2.Owner.Id && c4.Owner.Id != c1.Owner.Id)
                                                        {
                                                            if (!(c4.Owner is MEPCurve))
                                                            {
                                                                FamilyInstance familyInstance2 = c4.Owner as FamilyInstance;
                                                                ConnectorManager connectorManager2 = familyInstance2.MEPModel.ConnectorManager;
                                                                List<Connector> c5Connectors = (connectorManager2.Connectors).Cast<Connector>().ToList();
                                                                if (c5Connectors.Count == 2)
                                                                {
                                                                    for (int m = 0; m < c5Connectors.Count; m++)
                                                                    {
                                                                        Connector c5 = c5Connectors[m];
                                                                        if (c5.Origin.DistanceTo(c4.Origin) > tolerance)
                                                                        {
                                                                            locationToSnap = BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c5.Origin),4);
                                                                            BH.oM.Geometry.Line longLine = BH.Engine.Geometry.Create.Line(locationToSnap, locationFromSnap);
                                                                            locationToSnap = BH.Engine.Geometry.Modify.RoundCoordinates(longLine.PointAtParameter(0.5),4);
                                                                            
                                                                            endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin),4));
                                                                            BH.oM.Geometry.Line shortLine = BH.Engine.Geometry.Create.Line(locationToSnap, locationFromSnap);
                                                                            
                                                                            if (c1.Id == 0)
                                                                                extraLine1 = shortLine;
                                                                            else 
                                                                                extraLine2 = shortLine;
                                                                            
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    locationToSnap = BH.Engine.Geometry.Modify.RoundCoordinates(Convert.FromRevit((familyInstance2.Location) as LocationPoint),4);
                                                                    BH.oM.Geometry.Line shortLine = BH.Engine.Geometry.Create.Line(locationToSnap, locationFromSnap);
                                                                    endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin),4));
                                                                    
                                                                    if (c1.Id == 0)
                                                                        extraLine1 = shortLine;
                                                                    else 
                                                                        extraLine2 = shortLine;
                                                                    
                                                                }
                                                            }
                                                            else
                                                            {
                                                                endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.FromRevit((c3.Owner.Location) as LocationPoint),4));
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin), 4));
                                                    locationToSnap = BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c3.Origin), 4);
                                                    BH.oM.Geometry.Line shortLine = BH.Engine.Geometry.Create.Line(locationToSnap, locationFromSnap);

                                                    if (c1.Id == 0)
                                                    {
                                                        extraLine1 = shortLine;
                                                    }
                                                    else
                                                    {
                                                        extraLine2 = shortLine;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.FromRevit((familyInstance1.Location)as LocationPoint),4));
                                    }
                                }
                                else
                                {
                                    endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin),4));
                                }
                            }
                        }
                    }
                    else
                    {
                        endPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin),4));
                    }
                }
                else
                {
                    //if MEP linear object has connections that don't take fitting,
                    //then we need to store the point to later split the curve
                    midPoints.Add(BH.Engine.Geometry.Modify.RoundCoordinates(Convert.PointFromRevit(c1.Origin),4));
                }
            }

            //split MEP linear object if there was any other MEP linear object connected to it without connector
            BH.oM.Geometry.Line line = BH.Engine.Geometry.Create.Line(endPoints[1], endPoints[0]);
            List<BH.oM.Geometry.Line> result = new List<BH.oM.Geometry.Line>();
            
            if (extraLine1 != null) 
            {
                result.Add(extraLine1);
            }
            
            if (midPoints.Any())
            {
                result.AddRange(line.SplitAtPoints(midPoints));
            }
            else
            {
                result.Add(line);
            }
            
            if (extraLine2 != null) 
            {
                result.Add(extraLine2);
            }

            return result;
        }
        
        /***************************************************/
    }
}
