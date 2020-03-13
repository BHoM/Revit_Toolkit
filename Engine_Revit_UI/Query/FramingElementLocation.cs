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
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ICurve FramingElementLocation(this FamilyInstance familyInstance, RevitSettings settings)
        {
            ICurve locationCurve = null;
            Vector startOffset = new Vector { X = 0, Y = 0, Z = 0 };
            Vector endOffset = new Vector { X = 0, Y = 0, Z = 0 };

            Location location = familyInstance.Location;

            if (location is LocationPoint)
            {
                XYZ loc = (location as LocationPoint).Point;
                Parameter baseLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
                Parameter topLevelParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                Parameter baseOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM);
                Parameter topOffsetParam = familyInstance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM);

                if (baseLevelParam == null || !baseLevelParam.HasValue || topLevelParam == null || !topLevelParam.HasValue || baseOffsetParam == null || !baseOffsetParam.HasValue || topOffsetParam == null || !topOffsetParam.HasValue)
                    locationCurve = null;
                else
                {
                    double baseLevel = (familyInstance.Document.GetElement(baseLevelParam.AsElementId()) as Level).ProjectElevation;
                    double topLevel = (familyInstance.Document.GetElement(topLevelParam.AsElementId()) as Level).ProjectElevation;
                    double baseOffset = baseOffsetParam.AsDouble();
                    double topOffset = topOffsetParam.AsDouble();
                    XYZ baseNode = new XYZ(loc.X, loc.Y, baseLevel + baseOffset);
                    XYZ topNode = new XYZ(loc.X, loc.Y, topLevel + topOffset);
                    locationCurve = new oM.Geometry.Line { Start = baseNode.PointFromRevit(), End = topNode.PointFromRevit() };
                }
            }
            else if (location is LocationCurve)
            {
                locationCurve = (location as LocationCurve).Curve.IFromRevit();
                double startExtension = 0;
                double endExtension = 0;

                if (familyInstance.IsSlantedColumn)
                {
                    if (locationCurve is BH.oM.Geometry.Line)
                    {
                        BH.oM.Geometry.Line locationLine = locationCurve as BH.oM.Geometry.Line;
                        Vector direction = locationLine.Direction();
                        double angle = direction.Angle(Vector.ZAxis);

                        int attachedBase = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_BASE_ATTACHED_PARAM);
                        int attachedTop = familyInstance.LookupParameterInteger(BuiltInParameter.COLUMN_TOP_ATTACHED_PARAM);

                        if (attachedBase == 1 || attachedTop == 1)
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("A slanted column is attached at base or top, this may cause wrong length on pull to BHoM. Element Id: {0}", familyInstance.Id.IntegerValue));

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

                        locationLine.Start -= direction * startExtension;
                        locationLine.End += direction * endExtension;
                        locationCurve = locationLine;
                    }
                    else
                        BH.Engine.Reflection.Compute.RecordWarning(string.Format("A nonlinear slanted column has been detected. Attachment properties are lost. Element Id: {0}", familyInstance.Id.IntegerValue));
                }

                int yzJustification = familyInstance.LookupParameterInteger(BuiltInParameter.YZ_JUSTIFICATION);
                if (yzJustification == 0)
                {
                    double yOffset = familyInstance.LookupParameterDouble(BuiltInParameter.Y_OFFSET_VALUE);
                    double zOffset = -familyInstance.LookupParameterDouble(BuiltInParameter.Z_OFFSET_VALUE);
                    if (double.IsNaN(yOffset))
                        yOffset = 0;
                    if (double.IsNaN(zOffset))
                        zOffset = 0;

                    int yJustification = familyInstance.LookupParameterInteger(BuiltInParameter.Y_JUSTIFICATION);
                    int zJustification = familyInstance.LookupParameterInteger(BuiltInParameter.Z_JUSTIFICATION);

                    if (yJustification == 0 || yJustification == 3 || zJustification == 0 || zJustification == 3)
                    {
                        BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
                        if (bbox != null)
                        {
                            if (yJustification == 0)
                                yOffset -= (bbox.Max.Y - bbox.Min.Y) * 0.5;
                            else if (yJustification == 3)
                                yOffset += (bbox.Max.Y - bbox.Min.Y) * 0.5;

                            if (zJustification == 0)
                                zOffset += (bbox.Max.Z - bbox.Min.Z) * 0.5;
                            else if (zJustification == 3)
                                zOffset -= (bbox.Max.Z - bbox.Min.Z) * 0.5;
                        }
                    }

                    startOffset = (new XYZ(0, yOffset, zOffset)).VectorFromRevit();
                    endOffset = (new XYZ(0, yOffset, zOffset)).VectorFromRevit();
                }
                else if (yzJustification == 1)
                {
                    double yOffsetStart = familyInstance.LookupParameterDouble(BuiltInParameter.START_Y_OFFSET_VALUE);
                    double yOffsetEnd = familyInstance.LookupParameterDouble(BuiltInParameter.END_Y_OFFSET_VALUE);
                    double zOffsetStart = -familyInstance.LookupParameterDouble(BuiltInParameter.START_Z_OFFSET_VALUE);
                    double zOffsetEnd = -familyInstance.LookupParameterDouble(BuiltInParameter.END_Z_OFFSET_VALUE);
                    if (double.IsNaN(yOffsetStart))
                        yOffsetStart = 0;
                    if (double.IsNaN(yOffsetEnd))
                        yOffsetEnd = 0;
                    if (double.IsNaN(zOffsetStart))
                        zOffsetStart = 0;
                    if (double.IsNaN(zOffsetEnd))
                        zOffsetEnd = 0;

                    int yJustificationStart = familyInstance.LookupParameterInteger(BuiltInParameter.START_Y_JUSTIFICATION);
                    int yJustificationEnd = familyInstance.LookupParameterInteger(BuiltInParameter.END_Y_JUSTIFICATION);
                    int zJustificationStart = familyInstance.LookupParameterInteger(BuiltInParameter.START_Z_JUSTIFICATION);
                    int zJustificationEnd = familyInstance.LookupParameterInteger(BuiltInParameter.END_Z_JUSTIFICATION);

                    if (yJustificationStart == 0 || yJustificationStart == 3 || yJustificationEnd == 0 || yJustificationEnd == 3 || zJustificationStart == 0 || zJustificationStart == 3 || zJustificationEnd == 0 || zJustificationEnd == 3)
                    {
                        BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
                        if (bbox != null)
                        {
                            if (yJustificationStart == 0)
                                yOffsetStart -= (bbox.Max.Y - bbox.Min.Y) * 0.5;
                            else if (yJustificationStart == 3)
                                yOffsetStart += (bbox.Max.Y - bbox.Min.Y) * 0.5;

                            if (yJustificationEnd == 0)
                                yOffsetEnd -= (bbox.Max.Y - bbox.Min.Y) * 0.5;
                            else if (yJustificationEnd == 3)
                                yOffsetEnd += (bbox.Max.Y - bbox.Min.Y) * 0.5;

                            if (zJustificationStart == 0)
                                zOffsetStart += (bbox.Max.Z - bbox.Min.Z) * 0.5;
                            else if (zJustificationStart == 3)
                                zOffsetStart -= (bbox.Max.Z - bbox.Min.Z) * 0.5;

                            if (zJustificationEnd == 0)
                                zOffsetEnd += (bbox.Max.Z - bbox.Min.Z) * 0.5;
                            else if (zJustificationEnd == 3)
                                zOffsetEnd -= (bbox.Max.Z - bbox.Min.Z) * 0.5;
                        }
                    }

                    startOffset = (new XYZ(0, yOffsetStart, zOffsetStart)).VectorFromRevit();
                    endOffset = (new XYZ(0, yOffsetEnd, zOffsetEnd)).VectorFromRevit();
                }
            }
            
            if (locationCurve == null)
                familyInstance.BarCurveNotFoundWarning();

            //TODO: for nonlinear bars this should be actual offset, not translation?
            double startOffsetLength = startOffset.Length();
            double endOffsetLength = endOffset.Length();
            if (startOffsetLength > Tolerance.Distance || endOffsetLength > Tolerance.Distance)
            {
                Transform transform = familyInstance.GetTotalTransform();
                if (locationCurve is BH.oM.Geometry.Line)
                {
                    BH.oM.Geometry.Line l = locationCurve as BH.oM.Geometry.Line;
                    Vector yOffsetStart = new Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                    Vector zOffsetStart = new Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                    Vector yOffsetEnd = new Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
                    Vector zOffsetEnd = new Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
                    locationCurve = new BH.oM.Geometry.Line { Start = l.Start.Translate(yOffsetStart - zOffsetStart), End = l.End.Translate(yOffsetEnd - zOffsetEnd) };
                }
                else
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("Offset/justification of nonlinear framing is currently not supported. Revit justification and offset has been ignored. Element Id: {0}", familyInstance.Id.IntegerValue));
            }

            return locationCurve;
        }
        
        /***************************************************/
    }
}
