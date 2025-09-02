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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System;
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

        [Description("Queries a FamilyInstance to find its start and end points.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Optional, the RevitSettings to be used.")]
        [Output("locationCurveFraming", "BHoM curve queried from the Family Instance.")]
        public static ICurve LocationCurve(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            ICurve curve = null;

            if (typeof(Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.LocationCurveColumn(settings);
            else if (typeof(Pile).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.LocationCurvePile(settings);
            else if (typeof(IFramingElement).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.LocationCurveFraming(settings);

            return curve;
        }

        /***************************************************/

        [Description("Queries a FamilyInstance to find its start and end points, intended usage for framings.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Optional, the RevitSettings to be used.")]
        [Output("locationCurveFraming", "BHoM curve queried from the Family Instance.")]
        public static ICurve LocationCurveFraming(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            ICurve curve = (familyInstance.Location as LocationCurve)?.Curve?.IFromRevit();
            if (curve == null || (!(curve is NurbsCurve) && curve.ILength() <= settings.DistanceTolerance))
            {
                // Try getting the location directly from solid representation
                Options options = new Options();
                Solid alignedBox = familyInstance.AlignedBoundingBox(options);
                XYZ centroid = familyInstance.Centroid(options);
                XYZ extension = familyInstance.HandOrientation * 1000;
                Autodesk.Revit.DB.Line toIntersect = Autodesk.Revit.DB.Line.CreateBound(centroid - extension, centroid + extension);
                SolidCurveIntersection intersection = alignedBox.IntersectWithCurve(toIntersect, new SolidCurveIntersectionOptions());
                if (intersection?.SegmentCount == 1)
                    return intersection.GetCurveSegment(0).IFromRevit();
                else
                {
                    familyInstance.FramingCurveNotFoundWarning();
                    return null;
                }
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

        [Description("Queries a FamilyInstance to find its start and end points, intended usage for Columns.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Optional, the RevitSettings to be used.")]
        [Output("locationCurveColumn", "BHoM line queried from the Family Instance.")]
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
            else
            {
                // Try getting the location directly from solid representation
                Options options = new Options();
                Solid alignedBox = familyInstance.AlignedBoundingBox(options);
                XYZ centroid = familyInstance.Centroid(options);
                XYZ extension = familyInstance.HandOrientation.CrossProduct(familyInstance.FacingOrientation) * 1000;
                Autodesk.Revit.DB.Line toIntersect = Autodesk.Revit.DB.Line.CreateBound(centroid - extension, centroid + extension);
                SolidCurveIntersection intersection = alignedBox.IntersectWithCurve(toIntersect, new SolidCurveIntersectionOptions());
                if (intersection?.SegmentCount == 1)
                    curve = intersection.GetCurveSegment(0).IFromRevit() as BH.oM.Geometry.Line;
            }

            if (curve == null)
                familyInstance.FramingCurveNotFoundWarning();

            return curve;
        }

        /***************************************************/

        [Description("Queries an MEPCurve to find its start and end points, creating multiple lines segments if it contains points in between.")]
        [Input("mepCurve", "Revit MEPCurve to be queried.")]
        [Input("settings", "Optional, the RevitSettings to be used.")]
        [Output("locationCurveMEP", "BHoM lines list queried from the MEPCurve.")]
        public static List<BH.oM.Geometry.Line> LocationCurveMEP(this MEPCurve mepCurve, RevitSettings settings = null)
        {
            //sometimes an mepcurve will be connected without fittings
            //causing connections to occur in the middle of the locationcurve
            //hence the best approach is to also query the connections that are not end, to split curve there
            //so it becomes a node in a network and not ignored

            ConnectorManager connectorManager = mepCurve.ConnectorManager;
            ConnectorSet connectorSet = connectorManager.Connectors;
            List<Connector> connectors = connectorSet.Cast<Connector>().ToList();

            List<BH.oM.Geometry.Point> endPoints = connectors.Where(x => x.ConnectorType == ConnectorType.End).Select(x => x.Origin.PointFromRevit()).ToList();
            List<BH.oM.Geometry.Point> midPoints = connectors.Where(x => x.ConnectorType != ConnectorType.End).Select(x => x.Origin.PointFromRevit()).ToList();

            //Thanks to Revit, the order of connectors from connectorManager.Connectors isn't always the same!
            //So, we need SortCollinear to maintain the same connector order each time we pull an object.
            endPoints = endPoints.SortCollinear();

            BH.oM.Geometry.Line line = BH.Engine.Geometry.Create.Line(endPoints[0], endPoints[1]);
            List<BH.oM.Geometry.Line> result = new List<BH.oM.Geometry.Line>();

            if (midPoints.Any())
            {
                result.AddRange(line.SplitAtPoints(midPoints));
            }
            else
            {
                result.Add(line);
            }

            return result;
        }

        /***************************************************/

        [Description("Queries a FamilyInstance to find its start and end points, intended usage for Piles.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Optional, the RevitSettings to be used.")]
        [Output("locationCurvePile", "BHoM line queried from the Family Instance.")]
        public static BH.oM.Geometry.Line LocationCurvePile(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            // Find vertical centerline of the pile's bounding box
            BoundingBoxXYZ bbox = familyInstance.get_BoundingBox(null);
            XYZ center = (bbox.Min + bbox.Max) / 2;
            oM.Geometry.Point top = new XYZ(center.X, center.Y, bbox.Max.Z).PointFromRevit();
            oM.Geometry.Point bottom = new XYZ(center.X, center.Y, bbox.Min.Z).PointFromRevit();

            // Try to adjust for embedment
            double embedment = familyInstance.LookupParameterDouble("Pile Embedment");
            if (!double.IsNaN(embedment))
                top.Z -= embedment;
            else
                BH.Engine.Base.Compute.RecordWarning($"Parameter 'Pile Embedment' could not be found on the pile, therefore embedment may not be taken into account correctly. ElementId: {familyInstance.Id.IntegerValue}");

            return new BH.oM.Geometry.Line { Start = bottom, End = top };
        }

        /***************************************************/
    }
}






