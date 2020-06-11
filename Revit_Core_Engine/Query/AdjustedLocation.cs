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
using BH.oM.Geometry.CoordinateSystem;
using BH.oM.Physical.Elements;
using BH.oM.Reflection;
using System;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ICurve AdjustedLocation(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            ICurve curve = null;

            if (typeof(Column).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.AdjustedLocationColumn(settings);
            else if (typeof(IFramingElement).BuiltInCategories().Contains((BuiltInCategory)familyInstance.Category.Id.IntegerValue))
                curve = familyInstance.AdjustedLocationFraming(settings);

            return curve;
        }

        /***************************************************/
        
        public static ICurve AdjustedLocationFraming(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            ICurve curve = (familyInstance.Location as LocationCurve)?.Curve?.IFromRevit();
            if (curve == null || curve.ILength() <= settings.DistanceTolerance)
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
            return new BH.oM.Geometry.Line { Start = transformedLine.PlaneIntersection(startPlane, true), End = transformedLine.PlaneIntersection(endPlane, true) };
        }

        /***************************************************/

        public static ICurve AdjustedLocationFraming(this IFramingElement framingElement, FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            ICurve curve = framingElement?.Location;
            if (curve == null || curve.ILength() <= settings.DistanceTolerance)
            {
                framingElement.FramingCurveNotFoundWarning();
                return null;
            }

            BH.oM.Geometry.Line line = curve as BH.oM.Geometry.Line;
            if (line == null)
            {
                familyInstance.NonLinearFramingOffsetWarning();
                return curve;
            }

            Output<Vector, Vector> offsets = familyInstance.FramingOffsetVectors();
            Vector startOffset = -offsets.Item1;
            Vector endOffset = -offsets.Item2;

            double startOffsetLength = startOffset.Length();
            double endOffsetLength = endOffset.Length();
            if (startOffsetLength > Tolerance.Distance || endOffsetLength > Tolerance.Distance)
            {
                if ((startOffset - endOffset).Length() > settings.DistanceTolerance)
                {
                    //throw warning and reset offsets
                }

                Transform transform = familyInstance.GetTotalTransform();
                Vector yOffsetStart = new Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                Vector zOffsetStart = new Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                Vector yOffsetEnd = new Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
                Vector zOffsetEnd = new Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
                curve = new BH.oM.Geometry.Line { Start = line.Start.Translate(yOffsetStart + zOffsetStart), End = line.End.Translate(yOffsetEnd + zOffsetEnd) };
            }

            return curve;
        }

        /***************************************************/

        public static BH.oM.Geometry.Line AdjustedLocationColumn(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            BH.oM.Geometry.Line curve;

            if (familyInstance.IsSlantedColumn)
                curve = (familyInstance.Location as LocationCurve).Curve.IFromRevit() as BH.oM.Geometry.Line;
            else
            {
                XYZ loc = (familyInstance.Location as LocationPoint).Point;
                Parameter baseLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                Parameter topLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                Parameter baseOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                Parameter topOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                double baseLevel = (familyInstance.Document.GetElement(baseLevelParam.AsElementId()) as Level).ProjectElevation;
                double topLevel = (familyInstance.Document.GetElement(topLevelParam.AsElementId()) as Level).ProjectElevation;
                double baseOffset = baseOffsetParam.AsDouble();
                double topOffset = topOffsetParam.AsDouble();

                XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                curve = new oM.Geometry.Line { Start = baseNode.PointFromRevit(), End = topNode.PointFromRevit() };
            }

            Output<double, double> extensions = familyInstance.ColumnExtensions();
            double startExtension = extensions.Item1;
            double endExtension = extensions.Item2;

            if (Math.Abs(startExtension) > settings.DistanceTolerance || Math.Abs(endExtension) > settings.DistanceTolerance)
            {
                Vector direction = curve.Direction();
                curve = new oM.Geometry.Line { Start = curve.Start - direction * startExtension, End = curve.End + direction * endExtension };
            }

            if (curve.Length() <= settings.DistanceTolerance)
            {
                familyInstance.FramingCurveNotFoundWarning();
                return null;
            }

            return curve;
        }

        /***************************************************/

        public static BH.oM.Geometry.Line AdjustedLocationColumn(this FamilyInstance familyInstance, BH.oM.Geometry.Line curve = null, bool inverse = false, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            if (curve == null)
            {
                if (familyInstance.IsSlantedColumn)
                    curve = (familyInstance.Location as LocationCurve).Curve.IFromRevit() as BH.oM.Geometry.Line;
                else
                {
                    XYZ loc = (familyInstance.Location as LocationPoint).Point;
                    Parameter baseLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                    Parameter topLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                    Parameter baseOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                    Parameter topOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                    double baseLevel = (familyInstance.Document.GetElement(baseLevelParam.AsElementId()) as Level).ProjectElevation;
                    double topLevel = (familyInstance.Document.GetElement(topLevelParam.AsElementId()) as Level).ProjectElevation;
                    double baseOffset = baseOffsetParam.AsDouble();
                    double topOffset = topOffsetParam.AsDouble();

                    XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                    XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                    curve = new oM.Geometry.Line { Start = baseNode.PointFromRevit(), End = topNode.PointFromRevit() };
                }
            }

            Output<double, double> extensions = familyInstance.ColumnExtensions();
            double startExtension = extensions.Item1;
            double endExtension = extensions.Item2;

            if (Math.Abs(startExtension) > settings.DistanceTolerance || Math.Abs(endExtension) > settings.DistanceTolerance)
            {
                if (inverse)
                {
                    startExtension *= -1;
                    endExtension *= -1;
                }

                curve = ((BH.oM.Geometry.Line)curve).Extend(startExtension, endExtension);
            }

            if (curve.Length() <= settings.DistanceTolerance)
            {
                familyInstance.FramingCurveNotFoundWarning();
                return null;
            }

            return curve;
        }

        /***************************************************/

        public static Output<double, double> ColumnExtensions(this FamilyInstance familyInstance)
        {
            double startExtension = 0;
            double endExtension = 0;
            if (familyInstance.IsSlantedColumn)
            {
                int attachedBase = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_BASE_ATTACHED_PARAM);
                int attachedTop = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_TOP_ATTACHED_PARAM);

                if (attachedBase == 1 || attachedTop == 1)
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("A slanted column is attached at base or top, this may cause wrong length on pull to BHoM. Element Id: {0}", familyInstance.Id.IntegerValue));

                XYZ direction = ((Autodesk.Revit.DB.Line)((LocationCurve)familyInstance.Location).Curve).Direction;
                double angle = direction.AngleTo(XYZ.BasisZ);

                if (attachedBase == 1)
                    startExtension = -familyInstance.LookupParameterDouble(BuiltInParameter.COLUMN_BASE_ATTACHMENT_OFFSET_PARAM);
                else
                {
                    double baseExtensionValue = familyInstance.LookupParameterDouble(BuiltInParameter.SLANTED_COLUMN_BASE_EXTENSION);
                    if (!double.IsNaN(baseExtensionValue) && Math.Abs(baseExtensionValue) > Tolerance.Distance)
                    {
                        int baseCutStyle = familyInstance.LookupParameterInteger(BuiltInParameter.SLANTED_COLUMN_BASE_CUT_STYLE);
                        switch (baseCutStyle)
                        {
                            case 0:
                                startExtension = baseExtensionValue;
                                break;
                            case 1:
                                startExtension = baseExtensionValue / Math.Cos(angle);
                                break;
                            case 2:
                                startExtension = baseExtensionValue / Math.Sin(angle);
                                break;
                        }
                    }
                }

                if (attachedTop == 1)
                    endExtension = -familyInstance.LookupParameterDouble(BuiltInParameter.COLUMN_TOP_ATTACHMENT_OFFSET_PARAM);
                else
                {
                    double topExtensionValue = familyInstance.LookupParameterDouble(BuiltInParameter.SLANTED_COLUMN_TOP_EXTENSION);
                    if (!double.IsNaN(topExtensionValue) && Math.Abs(topExtensionValue) > Tolerance.Distance)
                    {
                        int topCutStyle = familyInstance.LookupParameterInteger(BuiltInParameter.SLANTED_COLUMN_TOP_CUT_STYLE);
                        switch (topCutStyle)
                        {
                            case 0:
                                endExtension = topExtensionValue;
                                break;
                            case 1:
                                endExtension = topExtensionValue / Math.Cos(angle);
                                break;
                            case 2:
                                endExtension = topExtensionValue / Math.Sin(angle);
                                break;
                        }
                    }
                }
            }

            return new Output<double, double> { Item1 = startExtension, Item2 = endExtension };
        }

        /***************************************************/

        public static Cartesian FramingCS(this BH.oM.Geometry.Line curve, RevitSettings settings)
        {
            Vector localX = curve.Direction();
            Vector localY;
            Vector localZ;

            double dotProduct = localX.DotProduct(Vector.ZAxis);
            if (1 - dotProduct <= settings.AngleTolerance)
            {
                localY = Vector.YAxis;
                localZ = -Vector.XAxis;
            }
            else if (1 + dotProduct <= settings.AngleTolerance)
            {
                localY = -Vector.YAxis;
                localZ = Vector.XAxis;
            }
            else
            {
                localY = Vector.ZAxis.CrossProduct(localX).Normalise();
                localZ = localX.CrossProduct(localY);
            }

            return new Cartesian(curve.Start, localX, localY, localZ);
        }
    }
}

