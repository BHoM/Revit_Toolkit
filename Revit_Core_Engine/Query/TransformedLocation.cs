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
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Geometry.CoordinateSystem;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ICurve TransformedFramingLocation(this FamilyInstance familyInstance, ICurve curve = null  /*IFramingElement framingElement*/, bool inverse = false, RevitSettings settings = null)
        {
            //ICurve curve = framingElement?.Location;
            if (curve == null)
                curve = (familyInstance.Location as LocationCurve)?.Curve?.IFromRevit();

            if (curve == null || curve.ILength() <= settings.DistanceTolerance)
            {
                familyInstance.BarCurveNotFoundWarning();
                return null;
            }

            BH.oM.Geometry.Line line = curve as BH.oM.Geometry.Line;
            if (line == null)
            {
                familyInstance.NonLinearFramingOffsetWarning();
                return curve;
            }

            //ConstantFramingProperty framingProperty = framingElement?.Property as ConstantFramingProperty;

            //IEnumerable<IParameterLink> parameterLinks = null;
            //RevitParametersToPush bHoMParameters = framingElement?.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
            //if (bHoMParameters != null)
            //    parameterLinks = settings?.ParameterSettings?.ParameterMap(framingElement.GetType())?.ParameterLinks.Where(x => x is ElementParameterLink);

            Output<Vector, Vector> offsets = familyInstance.OffsetVectors();
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
                //TransformMatrix transform;
                //if (framingElement != null)
                //{
                //    double rotation = bHoMParameters.l(LabelUtils.GetLabelFor(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE), parameterLinks);
                //    if (double.IsNaN(rotation))
                //        rotation = framingProperty.OrientationAngle;

                //    transform = Cartesian.GlobalXYZ.OrientationMatrix(line.FramingCS(rotation, settings));
                //}
                //else
                //    transform = familyInstance.GetTotalTransform().TransformFromRevit();

                //Vector yOffsetStart = new Vector { X = transform.Matrix[0, 1] * startOffset.Y, Y = transform.Matrix[1, 1] * startOffset.Y, Z = transform.Matrix[2, 1] * startOffset.Y };
                //Vector zOffsetStart = new Vector { X = transform.Matrix[0, 2] * startOffset.Z, Y = transform.Matrix[1, 2] * startOffset.Z, Z = transform.Matrix[2, 2] * startOffset.Z };
                //Vector yOffsetEnd = new Vector { X = transform.Matrix[0, 1] * endOffset.Y, Y = transform.Matrix[1, 1] * endOffset.Y, Z = transform.Matrix[2, 1] * endOffset.Y };
                //Vector zOffsetEnd = new Vector { X = transform.Matrix[0, 2] * endOffset.Z, Y = transform.Matrix[1, 2] * endOffset.Z, Z = transform.Matrix[2, 2] * endOffset.Z };
                //curve = new BH.oM.Geometry.Line { Start = line.Start.Translate(yOffsetStart - zOffsetStart), End = line.End.Translate(yOffsetEnd - zOffsetEnd) };

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

        //TODO: this could use RefObjects?
        public static Output<Vector, Vector> OffsetVectors(this FamilyInstance familyInstance)
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

        //public static ICurve TransformedFramingLocation(this FamilyInstance familyInstance, IFramingElement framingElement, bool inverse = false, RevitSettings settings = null)
        //{
        //    ICurve curve = framingElement?.Location;
        //    if (curve == null)
        //        curve = (familyInstance.Location as LocationCurve)?.Curve?.IFromRevit();

        //    if (curve == null || curve.ILength() <= settings.DistanceTolerance)
        //    {
        //        familyInstance.BarCurveNotFoundWarning();
        //        return null;
        //    }

        //    BH.oM.Geometry.Line line = curve as BH.oM.Geometry.Line;
        //    if (line == null)
        //    {
        //        familyInstance.NonLinearFramingOffsetWarning(framingElement);
        //        return curve;
        //    }

        //    ConstantFramingProperty framingProperty = framingElement?.Property as ConstantFramingProperty;

        //    IEnumerable<IParameterLink> parameterLinks = null;
        //    RevitParametersToPush bHoMParameters = framingElement?.Fragments.FirstOrDefault(x => x is RevitParametersToPush) as RevitParametersToPush;
        //    if (bHoMParameters != null)
        //        parameterLinks = settings?.ParameterSettings?.ParameterMap(framingElement.GetType())?.ParameterLinks.Where(x => x is ElementParameterLink);

        //    Output<Vector, Vector> offsets = familyInstance.OffsetVectors(framingProperty, bHoMParameters, parameterLinks);
        //    Vector startOffset = offsets.Item1;
        //    Vector endOffset = offsets.Item2;

        //    if (inverse)
        //    {
        //        startOffset *= -1;
        //        endOffset *= -1;
        //    }

        //    double startOffsetLength = startOffset.Length();
        //    double endOffsetLength = endOffset.Length();
        //    if (startOffsetLength > Tolerance.Distance || endOffsetLength > Tolerance.Distance)
        //    {
        //        TransformMatrix transform;
        //        if (framingElement != null)
        //        {
        //            double rotation = bHoMParameters.ParameterDouble(LabelUtils.GetLabelFor(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE), parameterLinks);
        //            if (double.IsNaN(rotation))
        //                rotation = framingProperty.OrientationAngle;

        //            transform = Cartesian.GlobalXYZ.OrientationMatrix(line.FramingCS(rotation, settings));
        //        }
        //        else
        //            transform = familyInstance.GetTotalTransform().TransformFromRevit();

        //        Vector yOffsetStart = new Vector { X = transform.Matrix[0, 1] * startOffset.Y, Y = transform.Matrix[1, 1] * startOffset.Y, Z = transform.Matrix[2, 1] * startOffset.Y };
        //        Vector zOffsetStart = new Vector { X = transform.Matrix[0, 2] * startOffset.Z, Y = transform.Matrix[1, 2] * startOffset.Z, Z = transform.Matrix[2, 2] * startOffset.Z };
        //        Vector yOffsetEnd = new Vector { X = transform.Matrix[0, 1] * endOffset.Y, Y = transform.Matrix[1, 1] * endOffset.Y, Z = transform.Matrix[2, 1] * endOffset.Y };
        //        Vector zOffsetEnd = new Vector { X = transform.Matrix[0, 2] * endOffset.Z, Y = transform.Matrix[1, 2] * endOffset.Z, Z = transform.Matrix[2, 2] * endOffset.Z };
        //        curve = new BH.oM.Geometry.Line { Start = line.Start.Translate(yOffsetStart - zOffsetStart), End = line.End.Translate(yOffsetEnd - zOffsetEnd) };
        //    }

        //    return curve;
        //}

        ///***************************************************/

        //public static Output<Vector, Vector> OffsetVectors(this FamilyInstance familyInstance, ConstantFramingProperty framingProperty, RevitParametersToPush bHoMParameters, IEnumerable<IParameterLink> parameterLinks)
        //{
        //    Vector startOffset = new Vector { X = 0, Y = 0, Z = 0 };
        //    Vector endOffset = new Vector { X = 0, Y = 0, Z = 0 };
        //    Output<Vector, Vector> result = new Output<Vector, Vector> { Item1 = startOffset, Item2 = endOffset };

        //    int yzJustification = familyInstance.ParameterIntegerOnPush(BuiltInParameter.YZ_JUSTIFICATION, bHoMParameters, parameterLinks);
        //    if (yzJustification == -1)
        //        yzJustification = 0;

        //    if (yzJustification == 0)
        //    {
        //        double yOffset = familyInstance.ParameterDoubleOnPush(BuiltInParameter.Y_OFFSET_VALUE, bHoMParameters, parameterLinks);
        //        if (double.IsNaN(yOffset))
        //            yOffset = 0;

        //        double zOffset = -familyInstance.ParameterDoubleOnPush(BuiltInParameter.Z_OFFSET_VALUE, bHoMParameters, parameterLinks);
        //        if (double.IsNaN(zOffset))
        //            zOffset = 0;

        //        int yJustification = familyInstance.ParameterIntegerOnPush(BuiltInParameter.Y_JUSTIFICATION, bHoMParameters, parameterLinks);
        //        if (yJustification == -1)
        //            yJustification = 2;

        //        int zJustification = familyInstance.ParameterIntegerOnPush(BuiltInParameter.Z_JUSTIFICATION, bHoMParameters, parameterLinks);
        //        if (zJustification == -1)
        //            zJustification = 2;

        //        if (yJustification == 0 || yJustification == 3 || zJustification == 0 || zJustification == 3)
        //        {
        //            //TODO: this could use RefObjects?
        //            double profileHeight = 0;
        //            double profileWidth = 0;
        //            if (familyInstance?.Symbol != null)
        //            {
        //                BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
        //                if (bbox != null)
        //                {
        //                    profileHeight = (bbox.Max.Z - bbox.Min.Z).ToSI(UnitType.UT_Length);
        //                    profileWidth = (bbox.Max.Y - bbox.Min.Y).ToSI(UnitType.UT_Length);
        //                }
        //            }
        //            else if (framingProperty != null)
        //            {
        //                IProfile profile = framingProperty.Profile;
        //                if (profile != null)
        //                {
        //                    profileHeight = profile.IHeight();
        //                    profileWidth = profile.IWidth();
        //                }
        //                else
        //                {
        //                    familyInstance.NoProfileWarning(framingProperty);
        //                    return result;
        //                }
        //            }
        //            else
        //            {
        //                familyInstance.NoProfileWarning(framingProperty);
        //                return result;
        //            }

        //            if (yJustification == 0)
        //                yOffset -= profileWidth * 0.5;
        //            else if (yJustification == 3)
        //                yOffset += profileWidth * 0.5;

        //            if (zJustification == 0)
        //                zOffset += profileHeight * 0.5;
        //            else if (zJustification == 3)
        //                zOffset -= profileHeight * 0.5;
        //        }

        //        startOffset = new Vector { X = 0, Y = yOffset, Z = zOffset };
        //        endOffset = startOffset;
        //    }
        //    else if (yzJustification == 1)
        //    {
        //        double yOffsetStart = familyInstance.ParameterDoubleOnPush(BuiltInParameter.START_Y_OFFSET_VALUE, bHoMParameters, parameterLinks);
        //        if (double.IsNaN(yOffsetStart))
        //            yOffsetStart = 0;

        //        double yOffsetEnd = familyInstance.ParameterDoubleOnPush(BuiltInParameter.END_Y_OFFSET_VALUE, bHoMParameters, parameterLinks);
        //        if (double.IsNaN(yOffsetEnd))
        //            yOffsetEnd = 0;

        //        double zOffsetStart = -familyInstance.ParameterDoubleOnPush(BuiltInParameter.START_Z_OFFSET_VALUE, bHoMParameters, parameterLinks);
        //        if (double.IsNaN(zOffsetStart))
        //            zOffsetStart = 0;

        //        double zOffsetEnd = -familyInstance.ParameterDoubleOnPush(BuiltInParameter.END_Z_OFFSET_VALUE, bHoMParameters, parameterLinks);
        //        if (double.IsNaN(zOffsetEnd))
        //            zOffsetEnd = 0;

        //        int yJustificationStart = familyInstance.ParameterIntegerOnPush(BuiltInParameter.START_Y_JUSTIFICATION, bHoMParameters, parameterLinks);
        //        if (yJustificationStart == -1)
        //            yJustificationStart = 2;

        //        int yJustificationEnd = familyInstance.ParameterIntegerOnPush(BuiltInParameter.END_Y_JUSTIFICATION, bHoMParameters, parameterLinks);
        //        if (yJustificationEnd == -1)
        //            yJustificationEnd = 2;

        //        int zJustificationStart = familyInstance.ParameterIntegerOnPush(BuiltInParameter.START_Z_JUSTIFICATION, bHoMParameters, parameterLinks);
        //        if (zJustificationStart == -1)
        //            zJustificationStart = 2;

        //        int zJustificationEnd = familyInstance.ParameterIntegerOnPush(BuiltInParameter.END_Z_JUSTIFICATION, bHoMParameters, parameterLinks);
        //        if (zJustificationEnd == -1)
        //            zJustificationEnd = 2;

        //        if (yJustificationStart == 0 || yJustificationStart == 3 || yJustificationEnd == 0 || yJustificationEnd == 3 || zJustificationStart == 0 || zJustificationStart == 3 || zJustificationEnd == 0 || zJustificationEnd == 3)
        //        {
        //            //TODO: this could use RefObjects?
        //            double profileHeight = 0;
        //            double profileWidth = 0;
        //            if (familyInstance?.Symbol != null)
        //            {
        //                BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
        //                if (bbox != null)
        //                {
        //                    profileHeight = (bbox.Max.Z - bbox.Min.Z).ToSI(UnitType.UT_Length);
        //                    profileWidth = (bbox.Max.Y - bbox.Min.Y).ToSI(UnitType.UT_Length);
        //                }
        //            }
        //            else if (framingProperty != null)
        //            {
        //                IProfile profile = framingProperty.Profile;
        //                if (profile != null)
        //                {
        //                    profileHeight = profile.IHeight();
        //                    profileWidth = profile.IWidth();
        //                }
        //                else
        //                {
        //                    familyInstance.NoProfileWarning(framingProperty);
        //                    return result;
        //                }
        //            }
        //            else
        //            {
        //                familyInstance.NoProfileWarning(framingProperty);
        //                return result;
        //            }

        //            if (yJustificationStart == 0)
        //                yOffsetStart -= profileWidth * 0.5;
        //            else if (yJustificationStart == 3)
        //                yOffsetStart += profileWidth * 0.5;

        //            if (yJustificationEnd == 0)
        //                yOffsetEnd -= profileWidth * 0.5;
        //            else if (yJustificationEnd == 3)
        //                yOffsetEnd += profileWidth * 0.5;

        //            if (zJustificationStart == 0)
        //                zOffsetStart += profileHeight * 0.5;
        //            else if (zJustificationStart == 3)
        //                zOffsetStart -= profileHeight * 0.5;

        //            if (zJustificationEnd == 0)
        //                zOffsetEnd += profileHeight * 0.5;
        //            else if (zJustificationEnd == 3)
        //                zOffsetEnd -= profileHeight * 0.5;
        //        }

        //        startOffset = new Vector { X = 0, Y = yOffsetStart, Z = zOffsetStart };
        //        endOffset = new Vector { X = 0, Y = yOffsetEnd, Z = zOffsetEnd };
        //    }

        //    return result;
        //}

        public static Cartesian FramingCS(this BH.oM.Geometry.Line curve, double rotation, RevitSettings settings)
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

            localY = localY.Rotate(rotation, localX);
            localZ = localZ.Rotate(rotation, localX);

            return new Cartesian(curve.Start, localX, localY, localZ);
        }

        /***************************************************/

        //public static int ParameterIntegerOnPush(this Element element, BuiltInParameter builtInParam, IRevitParameterFragment parameters, IEnumerable<IParameterLink> parameterLinks = null)
        //{
        //    int result = parameters.ParameterInteger(LabelUtils.GetLabelFor(builtInParam), parameterLinks);
        //    if (result == -1)
        //    {
        //        string stringValue = parameters.ParameterString(LabelUtils.GetLabelFor(builtInParam), parameterLinks);
        //        // TODO: THIS WILL NOT WORK! INSTEAD: CREATE ELEMENT => UPDATE PARAMS => ADJUST LOCATION!
        //    }

        //    if (result == -1 && element != null)
        //        result = element.LookupParameterInteger(builtInParam);

        //    return result;
        //}

        ///***************************************************/

        //public static double ParameterDoubleOnPush(this Element element, BuiltInParameter builtInParam, IRevitParameterFragment parameters, IEnumerable<IParameterLink> parameterLinks = null)
        //{
        //    double result = parameters.ParameterDouble(LabelUtils.GetLabelFor(builtInParam), parameterLinks);
        //    if (double.IsNaN(result) && element != null)
        //        result = element.LookupParameterDouble(builtInParam);

        //    return result;
        //}

        ///***************************************************/

        //public static int ParameterInteger(this IRevitParameterFragment parameters, string name, IEnumerable<IParameterLink> parameterLinks = null)
        //{
        //    if (parameters == null)
        //        return -1;

        //    RevitParameter bHoMParam = parameters.Parameter(name, parameterLinks);
        //    if (bHoMParam != null)
        //    {
        //        if (bHoMParam.Value is double || bHoMParam.Value is int)
        //            return (int)bHoMParam.Value;
        //        else if (bHoMParam.Value is string)
        //        {
        //            int value;
        //            if (int.TryParse((string)bHoMParam.Value, out value))
        //                return value;
        //        }
        //    }

        //    return -1;
        //}

        ///***************************************************/

        //public static double ParameterDouble(this IRevitParameterFragment parameters, string name, IEnumerable<IParameterLink> parameterLinks = null)
        //{
        //    if (parameters == null)
        //        return double.NaN;

        //    RevitParameter bHoMParam = parameters.Parameter(name, parameterLinks);
        //    if (bHoMParam != null)
        //    {
        //        if (bHoMParam.Value is double || bHoMParam.Value is int)
        //            return (double)bHoMParam.Value;
        //        else if (bHoMParam.Value is string)
        //        {
        //            double value;
        //            if (double.TryParse((string)bHoMParam.Value, out value))
        //                return value;
        //        }
        //    }

        //    return double.NaN;
        //}

        ///***************************************************/

        //public static RevitParameter Parameter(this IRevitParameterFragment parameters, string name, IEnumerable<IParameterLink> parameterLinks = null)
        //{
        //    if (parameters == null)
        //        return null;

        //    IParameterLink paramLink = null;
        //    if (parameterLinks != null)
        //        paramLink = parameterLinks.FirstOrDefault(x => x.ParameterNames.Any(y => y == name));

        //    if (paramLink != null)
        //        name = paramLink.PropertyName;

        //    return parameters.Parameters.FirstOrDefault(x => x.Name == name);
        //}

        /***************************************************/

        //public static double FramingRotation(this FamilyInstance familyInstance, RevitSettings settings)
        //{
        //    double rotation = bHoMParameters.ParameterDouble(LabelUtils.GetLabelFor(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE), parameterLinks);
        //    if (double.IsNaN(rotation) && framingElement != null)
        //    {
        //        if (framingProperty != null)
        //            rotation = framingProperty.OrientationAngle;
        //        else
        //        {
        //            //TODO: raise warning that no framing prop!
        //        }
        //    }

        //    double rotation = double.NaN;
        //    Curve location = (familyInstance.Location as LocationCurve)?.Curve;

        //    if (location != null)
        //    {
        //        BH.oM.Geometry.ICurve locationCurve = location.IFromRevit();
        //        if (locationCurve is BH.oM.Geometry.Line)
        //        {
        //            Transform transform = familyInstance.GetTotalTransform();
        //            if ((locationCurve as BH.oM.Geometry.Line).IsVertical(settings.AngleTolerance))
        //                rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, transform.BasisX);
        //            else
        //                rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisZ, transform.BasisX);
        //        }
        //        else
        //        {
        //            if (location.IsVerticalNonLinearCurve())
        //                rotation = Math.PI * 0.5 - familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
        //            else
        //                rotation = -familyInstance.LookupParameterDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);

        //            if (familyInstance.Mirrored)
        //                rotation *= -1;
        //        }
        //    }

        //    return rotation;
        //}

        /***************************************************/
    }
}

