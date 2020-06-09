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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Reflection;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ICurve AdjustedFramingLocation(this FamilyInstance familyInstance, ICurve curve = null, bool inverse = false, RevitSettings settings = null)
        {
            if (curve == null)
                curve = (familyInstance.Location as LocationCurve)?.Curve?.IFromRevit();

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

            Output<Vector, Vector> offsets = familyInstance.FramingOffsetVectors();
            Vector startOffset = offsets.Item1;
            Vector endOffset = offsets.Item2;

            if (inverse)
            {
                startOffset *= -1;
                endOffset *= -1;
            }

            double startOffsetLength = startOffset.Length();
            double endOffsetLength = endOffset.Length();
            if (startOffsetLength > Tolerance.Distance || endOffsetLength > Tolerance.Distance)
            {
                Transform transform = familyInstance.GetTotalTransform();
                Vector yOffsetStart = new Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                Vector zOffsetStart = new Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                Vector yOffsetEnd = new Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
                Vector zOffsetEnd = new Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
                curve = new BH.oM.Geometry.Line { Start = line.Start.Translate(yOffsetStart - zOffsetStart), End = line.End.Translate(yOffsetEnd - zOffsetEnd) };
            }

            return curve;
        }

        /***************************************************/

        public static BH.oM.Geometry.Line AdjustedColumnLocation(this FamilyInstance familyInstance, BH.oM.Geometry.Line curve = null, bool inverse = false, RevitSettings settings = null)
        {
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

            Output<double, double> extensions = familyInstance.ColumnExtensions(curve);
            double startExtension = extensions.Item1;
            double endExtension = extensions.Item2;

            if (Math.Abs(startExtension) > settings.DistanceTolerance || Math.Abs(endExtension) > settings.DistanceTolerance)
            {

                if (inverse)
                {
                    startExtension *= -1;
                    endExtension *= -1;
                }

                Vector direction = curve.Direction();
                curve.Start -= direction * startExtension;
                curve.End += direction * endExtension;
            }

            if (curve.Length() <= settings.DistanceTolerance)
            {
                familyInstance.FramingCurveNotFoundWarning();
                return null;
            }

            return curve;
        }

        /***************************************************/
        
        //TODO: this could use RefObjects?
        public static Output<Vector, Vector> FramingOffsetVectors(this FamilyInstance familyInstance)
        {
            Output<Vector, Vector> result = new Output<Vector, Vector> { Item1 = new Vector { X = 0, Y = 0, Z = 0 }, Item2 = new Vector { X = 0, Y = 0, Z = 0 } };

            int yzJustification = familyInstance.LookupParameterInteger(BuiltInParameter.YZ_JUSTIFICATION);
            if (yzJustification == -1)
                yzJustification = 0;

            if (yzJustification == 0)
            {
                double yOffset = familyInstance.LookupParameterDouble(BuiltInParameter.Y_OFFSET_VALUE);
                if (double.IsNaN(yOffset))
                    yOffset = 0;

                double zOffset = -familyInstance.LookupParameterDouble(BuiltInParameter.Z_OFFSET_VALUE);
                if (double.IsNaN(zOffset))
                    zOffset = 0;

                int yJustification = familyInstance.LookupParameterInteger(BuiltInParameter.Y_JUSTIFICATION);
                if (yJustification == -1)
                    yJustification = 2;

                int zJustification = familyInstance.LookupParameterInteger(BuiltInParameter.Z_JUSTIFICATION);
                if (zJustification == -1)
                    zJustification = 2;

                if (yJustification == 0 || yJustification == 3 || zJustification == 0 || zJustification == 3)
                {
                    if (familyInstance?.Symbol != null)
                    {
                        BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
                        if (bbox != null)
                        {
                            double profileHeight = (bbox.Max.Z - bbox.Min.Z).ToSI(UnitType.UT_Length);
                            double profileWidth = (bbox.Max.Y - bbox.Min.Y).ToSI(UnitType.UT_Length);

                            if (yJustification == 0)
                                yOffset -= profileWidth * 0.5;
                            else if (yJustification == 3)
                                yOffset += profileWidth * 0.5;

                            if (zJustification == 0)
                                zOffset += profileHeight * 0.5;
                            else if (zJustification == 3)
                                zOffset -= profileHeight * 0.5;
                        }
                    }
                    else
                    {
                        familyInstance.NoProfileWarning();
                        return result;
                    }
                }

                result.Item1 = new Vector { X = 0, Y = yOffset, Z = zOffset };
                result.Item2 = new Vector { X = 0, Y = yOffset, Z = zOffset };
            }
            else if (yzJustification == 1)
            {
                double yOffsetStart = familyInstance.LookupParameterDouble(BuiltInParameter.START_Y_OFFSET_VALUE);
                if (double.IsNaN(yOffsetStart))
                    yOffsetStart = 0;

                double yOffsetEnd = familyInstance.LookupParameterDouble(BuiltInParameter.END_Y_OFFSET_VALUE);
                if (double.IsNaN(yOffsetEnd))
                    yOffsetEnd = 0;

                double zOffsetStart = -familyInstance.LookupParameterDouble(BuiltInParameter.START_Z_OFFSET_VALUE);
                if (double.IsNaN(zOffsetStart))
                    zOffsetStart = 0;

                double zOffsetEnd = -familyInstance.LookupParameterDouble(BuiltInParameter.END_Z_OFFSET_VALUE);
                if (double.IsNaN(zOffsetEnd))
                    zOffsetEnd = 0;

                int yJustificationStart = familyInstance.LookupParameterInteger(BuiltInParameter.START_Y_JUSTIFICATION);
                if (yJustificationStart == -1)
                    yJustificationStart = 2;

                int yJustificationEnd = familyInstance.LookupParameterInteger(BuiltInParameter.END_Y_JUSTIFICATION);
                if (yJustificationEnd == -1)
                    yJustificationEnd = 2;

                int zJustificationStart = familyInstance.LookupParameterInteger(BuiltInParameter.START_Z_JUSTIFICATION);
                if (zJustificationStart == -1)
                    zJustificationStart = 2;

                int zJustificationEnd = familyInstance.LookupParameterInteger(BuiltInParameter.END_Z_JUSTIFICATION);
                if (zJustificationEnd == -1)
                    zJustificationEnd = 2;

                if (yJustificationStart == 0 || yJustificationStart == 3 || yJustificationEnd == 0 || yJustificationEnd == 3 || zJustificationStart == 0 || zJustificationStart == 3 || zJustificationEnd == 0 || zJustificationEnd == 3)
                {
                    if (familyInstance?.Symbol != null)
                    {
                        BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
                        if (bbox != null)
                        {
                            double profileHeight = (bbox.Max.Z - bbox.Min.Z).ToSI(UnitType.UT_Length);
                            double profileWidth = (bbox.Max.Y - bbox.Min.Y).ToSI(UnitType.UT_Length);

                            if (yJustificationStart == 0)
                                yOffsetStart -= profileWidth * 0.5;
                            else if (yJustificationStart == 3)
                                yOffsetStart += profileWidth * 0.5;

                            if (yJustificationEnd == 0)
                                yOffsetEnd -= profileWidth * 0.5;
                            else if (yJustificationEnd == 3)
                                yOffsetEnd += profileWidth * 0.5;

                            if (zJustificationStart == 0)
                                zOffsetStart += profileHeight * 0.5;
                            else if (zJustificationStart == 3)
                                zOffsetStart -= profileHeight * 0.5;

                            if (zJustificationEnd == 0)
                                zOffsetEnd += profileHeight * 0.5;
                            else if (zJustificationEnd == 3)
                                zOffsetEnd -= profileHeight * 0.5;
                        }
                    }
                    else
                    {
                        familyInstance.NoProfileWarning();
                        return result;
                    }
                }

                result.Item1 = new Vector { X = 0, Y = yOffsetStart, Z = zOffsetStart };
                result.Item2 = new Vector { X = 0, Y = yOffsetEnd, Z = zOffsetEnd };
            }

            return result;
        }

        /***************************************************/

        public static Output<double, double> ColumnExtensions(this FamilyInstance familyInstance, BH.oM.Geometry.Line curve)
        {
            double startExtension = 0;
            double endExtension = 0;
            if (familyInstance.IsSlantedColumn)
            {
                int attachedBase = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_BASE_ATTACHED_PARAM);
                int attachedTop = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_TOP_ATTACHED_PARAM);

                if (attachedBase == 1 || attachedTop == 1)
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("A slanted column is attached at base or top, this may cause wrong length on pull to BHoM. Element Id: {0}", familyInstance.Id.IntegerValue));

                Vector direction = curve.Direction();
                double angle = direction.Angle(Vector.ZAxis);

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
    }
}

