/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Physical.Elements;

using BH.Engine.Structure;
using BH.Engine.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static FamilyInstance ToRevitFamilyInstance(this IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            if (framingElement is Column)
            {
                return ToRevitFamilyInstance_Column(framingElement, document, pushSettings);
            }
            else if (framingElement is Beam || framingElement is Bracing || framingElement is Cable)
            {
                return ToRevitFamilyInstance_Framing(framingElement, document, pushSettings);
            }
            else if (framingElement is Pile)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Push of pile foundations is not supported in current version of BHoM. BHoM element Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Push of {0} is not supported in current version of BHoM. BHoM element Guid: {1}",framingElement.GetType(), framingElement.BHoM_Guid));
                return null;
            }
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilyInstance ToRevitFamilyInstance_Column(this IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = pushSettings.FindRefObject<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            pushSettings.DefaultIfNull();

            object customDataValue = null;

            //Check that the curve works for revit
            if (!CheckLocationCurveColumns(framingElement))
                return null;
            
            Line columnLine = framingElement.Location.IToRevit() as Line;
            Level level = null;

            customDataValue = framingElement.CustomDataValue("Base Level");
            if (customDataValue != null && customDataValue is int)
            {
                ElementId elementId = new ElementId((int)customDataValue);
                level = document.GetElement(elementId) as Level;
            }

            if (level == null)
                level = Query.BottomLevel(framingElement.Location, document);

            FamilySymbol familYSymbol = framingElement.Property.ToRevitFamilySymbol_Column(document, pushSettings);

            if (familYSymbol == null)
            {
                familYSymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralColumns, pushSettings.FamilyLoadSettings) as FamilySymbol;

                if (familYSymbol == null)
                {
                    Compute.ElementTypeNotFoundWarning(framingElement);
                    return null;
                }
            }

            FamilyPlacementType familyPlacementType = familYSymbol.Family.FamilyPlacementType;
            if (familyPlacementType != FamilyPlacementType.CurveBased && familyPlacementType != FamilyPlacementType.CurveBasedDetail && familyPlacementType != FamilyPlacementType.CurveDrivenStructural && familyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeWarning(framingElement, familYSymbol);
                return null;
            }

            familyInstance = document.Create.NewFamilyInstance(columnLine, familYSymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Column);

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                double orientationAngle = ToRevitOrientationAngleColumn(barProperty.OrientationAngle, framingElement.Location as oM.Geometry.Line);
                Parameter parameter = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(orientationAngle);

                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Autodesk.Revit.DB.Material material = document.GetElement(new ElementId(BH.Engine.Adapters.Revit.Query.ElementId(barProperty.Material))) as Autodesk.Revit.DB.Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }
            
            if (1 - Math.Abs(columnLine.Direction.DotProduct(XYZ.BasisZ)) < BH.oM.Geometry.Tolerance.Angle)
            {
                document.Regenerate();
                familyInstance.SetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 0);

                familyInstance.SetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM, columnLine.Origin.Z - level.Elevation, false);
                familyInstance.SetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM, columnLine.Origin.Z + columnLine.Length - level.Elevation, false);
                document.Regenerate();
            }

            familyInstance.SetParameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM, level.Id);
            familyInstance.SetParameter(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM, level.Id);
            familyInstance.SetParameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM, level.Id);
            familyInstance.SetParameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM, level.Id);

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    //BuiltInParameter.SCHEDULE_LEVEL_PARAM,
                    BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
                    BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
                    BuiltInParameter.FAMILY_TOP_LEVEL_PARAM,
                    BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM,
                    BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM,
                    BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE,
                    BuiltInParameter.COLUMN_BASE_ATTACHED_PARAM,
                    BuiltInParameter.COLUMN_TOP_ATTACHED_PARAM,
                    BuiltInParameter.COLUMN_BASE_ATTACHMENT_OFFSET_PARAM,
                    BuiltInParameter.COLUMN_TOP_ATTACHMENT_OFFSET_PARAM,
                    BuiltInParameter.SLANTED_COLUMN_BASE_EXTENSION,
                    BuiltInParameter.SLANTED_COLUMN_TOP_EXTENSION,
                    BuiltInParameter.SLANTED_COLUMN_BASE_CUT_STYLE,
                    BuiltInParameter.SLANTED_COLUMN_TOP_CUT_STYLE,
                    BuiltInParameter.STRUCTURAL_MATERIAL_PARAM
                };

                Modify.SetParameters(familyInstance, framingElement, paramsToIgnore);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElement, familyInstance);

            return familyInstance;
        }

        /***************************************************/

        private static bool CheckLocationCurveColumns(IFramingElement framingElement)
        {

            if ((framingElement.Location is BH.oM.Geometry.Line))
            {
                BH.oM.Geometry.Line line = framingElement.Location as BH.oM.Geometry.Line;

                if (line.Start.Z >= line.End.Z)
                {
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Start point of revit columns need to have a lower elevation than the end point. Have a look at flipping your location curves. Failing for object with BHoMGuid: {0}", framingElement.BHoM_Guid));
                    return false;
                }
                return true;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Revit does only support Line based columns. Try pushing your element as a beam instead. Failing for object with BHoMGuid: {0}", framingElement.BHoM_Guid));
                return false;
            }
        }

        /***************************************************/

        private static double ToRevitOrientationAngleColumn(double bhomOrientationAngle, BH.oM.Geometry.Line centreLine)
        {
            //For vertical columns orientation angles are following similar rules between Revit and BHoM but flipped 90 degrees
            if (BH.Engine.Structure.Query.IsVertical(centreLine))
            {
                return CheckOrientationAngleDomain((Math.PI * 0.5 - bhomOrientationAngle));
            }
            else
            {
                return CheckOrientationAngleDomain(-bhomOrientationAngle);
            }
        }

        /***************************************************/

        private static FamilyInstance ToRevitFamilyInstance_Framing(this IFramingElement framingElement, Document document, PushSettings pushSettings = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = pushSettings.FindRefObject<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            pushSettings.DefaultIfNull();

            object customDataValue = null;

            //Update justification based on custom data
            BH.oM.Geometry.ICurve adjustedLocation;
            bool adjusted = framingElement.AdjustLocation(out adjustedLocation);
            Curve revitCurve = adjustedLocation.IToRevit();

            bool isVertical, isLinear;
            //Check if curve is planar, and if so, if it is vertical. This is used to determine if the orientation angle needs
            //To be subtracted by 90 degrees or not.
            if (!CheckLocationCurveBeams(framingElement, revitCurve, out isVertical, out isLinear))
                return null;

            Level level = null;

            customDataValue = framingElement.CustomDataValue("Reference Level");
            if (customDataValue != null && customDataValue is int)
            {
                ElementId elementID = new ElementId((int)customDataValue);
                level = document.GetElement(elementID) as Level;
            }

            if (level == null)
                level = Query.BottomLevel(framingElement.Location, document);

            FamilySymbol familySymbol = framingElement.Property.ToRevitFamilySymbol_Framing(document, pushSettings);

            if (familySymbol == null)
            {
                familySymbol = Query.ElementType(framingElement, document, BuiltInCategory.OST_StructuralFraming, pushSettings.FamilyLoadSettings) as FamilySymbol;

                if (familySymbol == null)
                {
                    Compute.ElementTypeNotFoundWarning(framingElement);
                    return null;
                }
            }

            FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;
            if (familyPlacementType != FamilyPlacementType.CurveBased && familyPlacementType != FamilyPlacementType.CurveBasedDetail && familyPlacementType != FamilyPlacementType.CurveDrivenStructural && familyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeWarning(framingElement, familySymbol);
                return null;
            }

            if (framingElement is Beam)
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Beam);
            else if (framingElement is Bracing || framingElement is Cable)
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.Brace);
            else
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, Autodesk.Revit.DB.Structure.StructuralType.UnknownFraming);

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                double orientationAngle = ToRevitOrientationAngleBeams(barProperty.OrientationAngle, isVertical, isLinear);
                Parameter parameter = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE);
                if (parameter != null && !parameter.IsReadOnly)
                    parameter.Set(orientationAngle);

                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Autodesk.Revit.DB.Material material = document.GetElement(new ElementId(BH.Engine.Adapters.Revit.Query.ElementId(barProperty.Material))) as Autodesk.Revit.DB.Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Reflection.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }

            //Sets the insertion point to the centroid. 
            //TODO: Remove this once FramingElement.AdjustedLocation() query method is added.
            Parameter zJustification = familyInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
            if (zJustification != null && !zJustification.IsReadOnly)
                zJustification.Set((int)Autodesk.Revit.DB.Structure.ZJustification.Origin);

            if (pushSettings.CopyCustomData)
            {
                BuiltInParameter[] paramsToIgnore = new BuiltInParameter[]
                {
                    BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION,
                    BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM,
                    BuiltInParameter.ALL_MODEL_IMAGE,
                    BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE,
                    BuiltInParameter.STRUCTURAL_MATERIAL_PARAM
                };

                //TODO: this is a hack, should be removed once the offset/justification  is implemented for curved framing.
                if (framingElement.Location is BH.oM.Geometry.Line)
                {
                    List<BuiltInParameter> paramsToIgnoreList = new List<BuiltInParameter>
                    {
                        BuiltInParameter.Y_OFFSET_VALUE,
                        BuiltInParameter.Z_OFFSET_VALUE,
                        BuiltInParameter.START_Y_OFFSET_VALUE,
                        BuiltInParameter.END_Y_OFFSET_VALUE,
                        BuiltInParameter.START_Z_OFFSET_VALUE,
                        BuiltInParameter.END_Z_OFFSET_VALUE,
                    };

                    paramsToIgnoreList.AddRange(paramsToIgnore);
                    paramsToIgnore = paramsToIgnoreList.ToArray();
                }

                //TODO: this is a hack, should be removed once the offset/justification  is properly implemented.
                if (!adjusted)
                {
                    List<BuiltInParameter> paramsToIgnoreList = new List<BuiltInParameter>
                    {
                        BuiltInParameter.YZ_JUSTIFICATION,
                        BuiltInParameter.Y_JUSTIFICATION,
                        BuiltInParameter.Z_JUSTIFICATION,
                        BuiltInParameter.START_Y_JUSTIFICATION,
                        BuiltInParameter.END_Y_JUSTIFICATION,
                        BuiltInParameter.START_Z_JUSTIFICATION,
                        BuiltInParameter.END_Z_JUSTIFICATION
                    };

                    paramsToIgnoreList.AddRange(paramsToIgnore);
                    paramsToIgnore = paramsToIgnoreList.ToArray();
                }

                Modify.SetParameters(familyInstance, framingElement, paramsToIgnore);
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(framingElement, familyInstance);

            return familyInstance;
        }

        /***************************************************/

        private static bool CheckLocationCurveBeams(IFramingElement framingElement, Curve revitCurve, out bool isVertical, out bool isLinear)
        {
            isLinear = framingElement.Location is BH.oM.Geometry.Line;

            //Line-based elements are handled slightly differently for orientation angles check used later
            if (isLinear)
            {
                isVertical = BH.Engine.Structure.Query.IsVertical(framingElement.Location as BH.oM.Geometry.Line);
                return true;
            }
            else
            {
                //Revit framing elements can only handle planar curves.
                CurveLoop curveLoop = CurveLoop.Create(new Curve[] { revitCurve });
                if (curveLoop.HasPlane())
                {
                    Plane curvePlane = curveLoop.GetPlane();
                    //Orientation angles are handled slightly differently for framing elements that have a curve fits in a plane that contains the z-vector
                    isVertical = Math.Abs(curvePlane.Normal.DotProduct(XYZ.BasisZ)) < BH.oM.Geometry.Tolerance.Angle;
                    return true;
                }
                else
                {
                    isVertical = false;
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Revit framing elements does only support planar curves. Failing for object with BHoMGuid: {0}", framingElement.BHoM_Guid));
                    return false;
                }
            }
        }

        /***************************************************/

        private static double ToRevitOrientationAngleBeams(double bhomOrientationAngle, bool isVertical, bool isLinear)
        {
            return CheckOrientationAngleDomain(-bhomOrientationAngle);
        }

        /***************************************************/

        private static double CheckOrientationAngleDomain(double orientationAngle)
        {
            //Fixes orientation angle excedening +- 2 PI
            orientationAngle = orientationAngle % (2 * Math.PI);

            //The above should be enough, but bue to some tolerance issues going into revit it can sometimes still give errors.
            //The below is added as an extra saftey check
            if (orientationAngle - BH.oM.Geometry.Tolerance.Angle < -Math.PI * 2)
                return orientationAngle + Math.PI * 2;
            else if (orientationAngle + BH.oM.Geometry.Tolerance.Angle > Math.PI * 2)
                return orientationAngle - Math.PI * 2;

            return orientationAngle;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        private static bool AdjustLocation(this IFramingElement framingElement, out BH.oM.Geometry.ICurve adjustedLocation)
        {
            adjustedLocation = framingElement.Location;
            if (framingElement.Location == null || framingElement.Location.ILength() < BH.oM.Geometry.Tolerance.Distance)
            {
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element has zero length or no curve assigned. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return false;
            }

            if (!framingElement.CustomData.ContainsKey("yz Justification"))
                return false;

            BH.oM.Geometry.Line locationLine = framingElement.Location as BH.oM.Geometry.Line;
            if (locationLine == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("Offset/justification of nonlinear bars is currently not supported. Revit justification and offset has been ignored. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return false;
            }

            BH.oM.Physical.FramingProperties.ConstantFramingProperty property = framingElement.Property as BH.oM.Physical.FramingProperties.ConstantFramingProperty;
            if (property == null || property.Profile == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element does not have a profile. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return false;
            }

            List<BH.oM.Geometry.PolyCurve> outlines = BH.Engine.Geometry.Compute.IJoin(property.Profile.Edges.ToList());
            BH.oM.Geometry.PolyCurve profileCurve = outlines.FirstOrDefault();
            if (outlines.Count != 1 || profileCurve == null || !profileCurve.IIsClosed())
            {
                BH.Engine.Reflection.Compute.RecordWarning(string.Format("Framing element's profile is not supported or incorrect. BHoM Guid: {0}", framingElement.BHoM_Guid));
                return false;
            }

            BH.oM.Geometry.BoundingBox bbox = profileCurve.Bounds();
            double dY = (bbox.Max.X - bbox.Min.X) * 0.5;
            double dZ = (bbox.Max.Y - bbox.Min.Y) * 0.5;
            BH.oM.Geometry.Point centre = (bbox.Min + bbox.Max) * 0.5;

            BH.oM.Geometry.Vector xDir = locationLine.Direction();
            BH.oM.Geometry.Vector yDir = 1 - Math.Abs(xDir.DotProduct(BH.oM.Geometry.Vector.ZAxis)) < BH.oM.Geometry.Tolerance.Angle ? BH.oM.Geometry.Vector.XAxis.CrossProduct(xDir) : BH.oM.Geometry.Vector.ZAxis.CrossProduct(xDir).Normalise().Rotate(property.OrientationAngle, xDir);
            BH.oM.Geometry.Vector zDir = yDir.CrossProduct(xDir);

            int yzJustification = (int)framingElement.CustomData["yz Justification"];
            if (yzJustification == 0)
            {
                if (!framingElement.CustomData.ContainsKey("y Justification") || !framingElement.CustomData.ContainsKey("z Justification"))
                {
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element does not have justification properties. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return false;
                }

                int yJustification = (int)framingElement.CustomData["y Justification"];
                int zJustification = (int)framingElement.CustomData["z Justification"];

                double yAdjustment = 0;
                if (yJustification == 0)
                    yAdjustment = centre.Y + dY;
                else if (yJustification == 1)
                    yAdjustment = centre.Y;
                else if (yJustification == 3)
                    yAdjustment = centre.Y - dY;

                double zAdjustment = 0;
                if (zJustification == 0)
                    zAdjustment = centre.Z - dZ;
                else if (zJustification == 1)
                    zAdjustment = centre.Z;
                else if (zJustification == 3)
                    zAdjustment = centre.Z + dZ;

                adjustedLocation = locationLine.Translate(yDir * yAdjustment + zDir * zAdjustment);
                return true;
            }
            else
            {
                if (!framingElement.CustomData.ContainsKey("Start y Justification") || !framingElement.CustomData.ContainsKey("Start z Justification") || !framingElement.CustomData.ContainsKey("End y Justification") || !framingElement.CustomData.ContainsKey("End z Justification"))
                {
                    BH.Engine.Reflection.Compute.RecordWarning(string.Format("The framing element does not have justification properties. BHoM Guid: {0}", framingElement.BHoM_Guid));
                    return false;
                }

                int startYJustification = (int)framingElement.CustomData["Start y Justification"];
                int startZJustification = (int)framingElement.CustomData["Start z Justification"];
                int endYJustification = (int)framingElement.CustomData["End y Justification"];
                int endZJustification = (int)framingElement.CustomData["End z Justification"];

                double startYAdjustment = 0;
                if (startYJustification == 0)
                    startYAdjustment = centre.Y + dY;
                else if (startYJustification == 1)
                    startYAdjustment = centre.Y;
                else if (startYJustification == 3)
                    startYAdjustment = centre.Y - dY;

                double startZAdjustment = 0;
                if (startZJustification == 0)
                    startZAdjustment = centre.Z - dZ;
                else if (startZJustification == 1)
                    startZAdjustment = centre.Z;
                else if (startZJustification == 3)
                    startZAdjustment = centre.Z + dZ;

                double endYAdjustment = 0;
                if (endYJustification == 0)
                    endYAdjustment = centre.Y + dY;
                else if (endYJustification == 1)
                    endYAdjustment = centre.Y;
                else if (endYJustification == 3)
                    endYAdjustment = centre.Y - dY;

                double endZAdjustment = 0;
                if (endZJustification == 0)
                    endZAdjustment = centre.Z - dZ;
                else if (endZJustification == 1)
                    endZAdjustment = centre.Z;
                else if (endZJustification == 3)
                    endZAdjustment = centre.Z + dZ;

                adjustedLocation = new BH.oM.Geometry.Line { Start = locationLine.Start.Translate(yDir * startYAdjustment + zDir * startZAdjustment), End = locationLine.End.Translate(yDir * endYAdjustment + zDir * endZAdjustment) };
                return true;
            }
        }
        
        /***************************************************/
    }
}