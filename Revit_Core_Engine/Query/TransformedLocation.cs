///*
// * This file is part of the Buildings and Habitats object Model (BHoM)
// * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
// *
// * Each contributor holds copyright over their respective contributions.
// * The project versioning (Git) records all such contribution source information.
// *                                           
// *                                                                              
// * The BHoM is free software: you can redistribute it and/or modify         
// * it under the terms of the GNU Lesser General Public License as published by  
// * the Free Software Foundation, either version 3.0 of the License, or          
// * (at your option) any later version.                                          
// *                                                                              
// * The BHoM is distributed in the hope that it will be useful,              
// * but WITHOUT ANY WARRANTY; without even the implied warranty of               
// * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
// * GNU Lesser General Public License for more details.                          
// *                                                                            
// * You should have received a copy of the GNU Lesser General Public License     
// * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
// */

//using Autodesk.Revit.DB;
//using BH.oM.Adapters.Revit.Parameters;
//using BH.oM.Adapters.Revit.Settings;
//using BH.oM.Geometry;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace BH.Revit.Engine.Core
//{
//    public static partial class Query
//    {
//        /***************************************************/
//        /****              Public methods               ****/
//        /***************************************************/

//        public static ICurve TransformedFramingLocation(this ICurve curve, bool inverse, List<Parameter> parameters = null, IList<RevitParameter> bHoMParameters = null, RevitSettings settings = null)
//        {
//            //TODO: how to match BHoM params with revit params?
//            // Construct a list of location-driving BuiltInParameters per BHoM type - match them by LabelUtils & ParameterSettings
//            // Then replace the ones from parameters with those from BHoM
//            // do the location transformation

////            double startExtension = 0;
////            double endExtension = 0;
////            int yzJustification = familyInstance.LookupParameterInteger(BuiltInParameter.YZ_JUSTIFICATION);
////            if (yzJustification == 0)
////            {
////                double yOffset = familyInstance.LookupParameterDouble(BuiltInParameter.Y_OFFSET_VALUE);
////                double zOffset = -familyInstance.LookupParameterDouble(BuiltInParameter.Z_OFFSET_VALUE);
////                if (double.IsNaN(yOffset))
////                    yOffset = 0;
////                if (double.IsNaN(zOffset))
////                    zOffset = 0;

////                int yJustification = familyInstance.LookupParameterInteger(BuiltInParameter.Y_JUSTIFICATION);
////                int zJustification = familyInstance.LookupParameterInteger(BuiltInParameter.Z_JUSTIFICATION);

////                if (yJustification == 0 || yJustification == 3 || zJustification == 0 || zJustification == 3)
////                {
////                    BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
////                    if (bbox != null)
////                    {
////                        if (yJustification == 0)
////                            yOffset -= (bbox.Max.Y - bbox.Min.Y) * 0.5;
////                        else if (yJustification == 3)
////                            yOffset += (bbox.Max.Y - bbox.Min.Y) * 0.5;

////                        if (zJustification == 0)
////                            zOffset += (bbox.Max.Z - bbox.Min.Z) * 0.5;
////                        else if (zJustification == 3)
////                            zOffset -= (bbox.Max.Z - bbox.Min.Z) * 0.5;
////                    }
////                }

////                startOffset = (new XYZ(0, yOffset, zOffset)).VectorFromRevit();
////                endOffset = (new XYZ(0, yOffset, zOffset)).VectorFromRevit();
////            }
////            else if (yzJustification == 1)
////            {
////                double yOffsetStart = familyInstance.LookupParameterDouble(BuiltInParameter.START_Y_OFFSET_VALUE);
////                double yOffsetEnd = familyInstance.LookupParameterDouble(BuiltInParameter.END_Y_OFFSET_VALUE);
////                double zOffsetStart = -familyInstance.LookupParameterDouble(BuiltInParameter.START_Z_OFFSET_VALUE);
////                double zOffsetEnd = -familyInstance.LookupParameterDouble(BuiltInParameter.END_Z_OFFSET_VALUE);
////                if (double.IsNaN(yOffsetStart))
////                    yOffsetStart = 0;
////                if (double.IsNaN(yOffsetEnd))
////                    yOffsetEnd = 0;
////                if (double.IsNaN(zOffsetStart))
////                    zOffsetStart = 0;
////                if (double.IsNaN(zOffsetEnd))
////                    zOffsetEnd = 0;

////                int yJustificationStart = familyInstance.LookupParameterInteger(BuiltInParameter.START_Y_JUSTIFICATION);
////                int yJustificationEnd = familyInstance.LookupParameterInteger(BuiltInParameter.END_Y_JUSTIFICATION);
////                int zJustificationStart = familyInstance.LookupParameterInteger(BuiltInParameter.START_Z_JUSTIFICATION);
////                int zJustificationEnd = familyInstance.LookupParameterInteger(BuiltInParameter.END_Z_JUSTIFICATION);

////                if (yJustificationStart == 0 || yJustificationStart == 3 || yJustificationEnd == 0 || yJustificationEnd == 3 || zJustificationStart == 0 || zJustificationStart == 3 || zJustificationEnd == 0 || zJustificationEnd == 3)
////                {
////                    BoundingBoxXYZ bbox = familyInstance.Symbol.get_BoundingBox(null);
////                    if (bbox != null)
////                    {
////                        if (yJustificationStart == 0)
////                            yOffsetStart -= (bbox.Max.Y - bbox.Min.Y) * 0.5;
////                        else if (yJustificationStart == 3)
////                            yOffsetStart += (bbox.Max.Y - bbox.Min.Y) * 0.5;

////                        if (yJustificationEnd == 0)
////                            yOffsetEnd -= (bbox.Max.Y - bbox.Min.Y) * 0.5;
////                        else if (yJustificationEnd == 3)
////                            yOffsetEnd += (bbox.Max.Y - bbox.Min.Y) * 0.5;

////                        if (zJustificationStart == 0)
////                            zOffsetStart += (bbox.Max.Z - bbox.Min.Z) * 0.5;
////                        else if (zJustificationStart == 3)
////                            zOffsetStart -= (bbox.Max.Z - bbox.Min.Z) * 0.5;

////                        if (zJustificationEnd == 0)
////                            zOffsetEnd += (bbox.Max.Z - bbox.Min.Z) * 0.5;
////                        else if (zJustificationEnd == 3)
////                            zOffsetEnd -= (bbox.Max.Z - bbox.Min.Z) * 0.5;
////                    }
////                }

////                startOffset = (new XYZ(0, yOffsetStart, zOffsetStart)).VectorFromRevit();
////                endOffset = (new XYZ(0, yOffsetEnd, zOffsetEnd)).VectorFromRevit();
////            }
////        }
            
////            if (curve == null)
////                familyInstance.BarCurveNotFoundWarning();

////            //TODO: for nonlinear bars this should be actual offset, not translation?
////            double startOffsetLength = startOffset.Length();
////        double endOffsetLength = endOffset.Length();
////            if (startOffsetLength > Tolerance.Distance || endOffsetLength > Tolerance.Distance)
////            {
////                Transform transform = familyInstance.GetTotalTransform();
////                if (curve is BH.oM.Geometry.Line)
////                {
////                    BH.oM.Geometry.Line l = curve as BH.oM.Geometry.Line;
////        Vector yOffsetStart = new Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
////        Vector zOffsetStart = new Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
////        Vector yOffsetEnd = new Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
////        Vector zOffsetEnd = new Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
////        curve = new BH.oM.Geometry.Line { Start = l.Start.Translate(yOffsetStart - zOffsetStart), End = l.End.Translate(yOffsetEnd - zOffsetEnd) };
////}
////                else
////                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("Offset/justification of nonlinear framing is currently not supported. Revit justification and offset has been ignored. Element Id: {0}", familyInstance.Id.IntegerValue));
////            }

////            return curve;

//        }

//        /***************************************************/

//        public static List<Parameter> FramingLocationParameters(this FamilyInstance familyInstance)
//        {
//            //TODO: that should return all params that can be needed for geometry manipulation
//            throw new NotImplementedException();
//        }

//        /***************************************************/
//    }
//}

