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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static ICurve TransformedFramingLocation(this FamilyInstance familyInstance, ICurve curve = null, bool inverse = false, RevitSettings settings = null)
        {
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

        /***************************************************/
    }
}

