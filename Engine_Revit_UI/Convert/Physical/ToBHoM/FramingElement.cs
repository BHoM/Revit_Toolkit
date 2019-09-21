/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Structure;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Geometry.ShapeProfiles;
using BHG = BH.Engine.Geometry;
using BHS = BH.Engine.Structure;
using BH.oM.Structure.MaterialFragments;

using System;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static IFramingElement ToBHoMFramingElement(this FamilyInstance familyInstance, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            IFramingElement framingElement = pullSettings.FindRefObject<IFramingElement>(familyInstance.Id.IntegerValue);
            if (framingElement != null)
                return framingElement;

            Type framingType = ((BuiltInCategory)familyInstance.Category.Id.IntegerValue).FramingType();
            if (framingType == null)
            {
                BH.Engine.Reflection.Compute.RecordError(string.Format("Category of the element is not supported by IFramingElement. Element Id: {0}", familyInstance.Id.IntegerValue));
                return null;
            }

            oM.Geometry.ICurve locationCurve = null;
            oM.Geometry.Vector startOffset;
            oM.Geometry.Vector endOffset;
            
            familyInstance.FramingElementLocation(pullSettings, out locationCurve, out startOffset, out endOffset);
            if (locationCurve == null)
                familyInstance.BarCurveNotFoundWarning();
            
            double startOffsetLength = BH.Engine.Geometry.Query.Length(startOffset);
            double endOffsetLength = BH.Engine.Geometry.Query.Length(endOffset);
            if (startOffsetLength > BH.oM.Geometry.Tolerance.Distance || endOffsetLength > BH.oM.Geometry.Tolerance.Distance)
            {
                Transform transform = familyInstance.GetTotalTransform();
                if (BH.Engine.Geometry.Query.Distance(startOffset, endOffset) <= BH.oM.Geometry.Tolerance.Distance)
                {
                    BH.oM.Geometry.Vector yOffset = new BH.oM.Geometry.Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                    BH.oM.Geometry.Vector zOffset = new BH.oM.Geometry.Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                    locationCurve = BHG.Modify.Translate(locationCurve as dynamic, yOffset - zOffset);
                }
                else if (locationCurve is BH.oM.Geometry.Line)
                {
                    BH.oM.Geometry.Line l = locationCurve as BH.oM.Geometry.Line;
                    BH.oM.Geometry.Vector yOffsetStart = new BH.oM.Geometry.Vector { X = transform.BasisY.X * startOffset.Y, Y = transform.BasisY.Y * startOffset.Y, Z = transform.BasisY.Z * startOffset.Y };
                    BH.oM.Geometry.Vector zOffsetStart = new BH.oM.Geometry.Vector { X = transform.BasisZ.X * startOffset.Z, Y = transform.BasisZ.Y * startOffset.Z, Z = transform.BasisZ.Z * startOffset.Z };
                    BH.oM.Geometry.Vector yOffsetEnd = new BH.oM.Geometry.Vector { X = transform.BasisY.X * endOffset.Y, Y = transform.BasisY.Y * endOffset.Y, Z = transform.BasisY.Z * endOffset.Y };
                    BH.oM.Geometry.Vector zOffsetEnd = new BH.oM.Geometry.Vector { X = transform.BasisZ.X * endOffset.Z, Y = transform.BasisZ.Y * endOffset.Z, Z = transform.BasisZ.Z * endOffset.Z };
                    locationCurve = new BH.oM.Geometry.Line { Start = BH.Engine.Geometry.Modify.Translate(l.Start, yOffsetStart - zOffsetStart), End = BH.Engine.Geometry.Modify.Translate(l.End, yOffsetEnd - zOffsetEnd) };
                }
                else
                    BH.Engine.Reflection.Compute.RecordError(string.Format("Nonlinear bars with nonuniform justification are currently not supported. Revit offset has been ignored. Element Id: {0}", familyInstance.Id.IntegerValue));
            }
            
            // Check if an instance or type Structural Material parameter exists.
            ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
            if (structuralMaterialId.IntegerValue < 0)
                structuralMaterialId = familyInstance.Symbol.LookupElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

            Autodesk.Revit.DB.Material revitMaterial = familyInstance.Document.GetElement(structuralMaterialId) as Autodesk.Revit.DB.Material;
            BH.oM.Physical.Materials.Material material = pullSettings.FindRefObject<oM.Physical.Materials.Material>(structuralMaterialId.IntegerValue);
            
            if (material == null)
                material = revitMaterial.ToBHoMEmptyMaterial(pullSettings);

            string materialGrade = familyInstance.MaterialGrade();
            material = material.UpdateMaterialProperties(revitMaterial, pullSettings, materialGrade, familyInstance.StructuralMaterialType);

            string elementName = familyInstance.Name;
            string profileName = familyInstance.Symbol.Name;

            IProfile profile = familyInstance.Symbol.ToBHoMProfile(pullSettings);
            if (profile == null)
                profile = familyInstance.BHoMFreeFormProfile(pullSettings);
            
            if (profile == null)
                familyInstance.Symbol.NotConvertedWarning();

            double rotation = familyInstance.FramingElementRotation(pullSettings);
            ConstantFramingProperty property = BH.Engine.Physical.Create.ConstantFramingProperty(profile, material, rotation, profileName);
            
            if (framingType == typeof(BH.oM.Physical.Elements.Beam))
                framingElement = BH.Engine.Physical.Create.Beam(locationCurve, property, elementName);
            else if (framingType == typeof(BH.oM.Physical.Elements.Bracing))
                framingElement = BH.Engine.Physical.Create.Bracing(locationCurve, property, elementName);
            else if (framingType == typeof(BH.oM.Physical.Elements.Column))
                framingElement = BH.Engine.Physical.Create.Column(locationCurve, property, elementName);
            else
                framingElement = BH.Engine.Physical.Create.Beam(locationCurve, property, elementName);
            
            framingElement = Modify.SetIdentifiers(framingElement, familyInstance) as IFramingElement;
            if (pullSettings.CopyCustomData)
                framingElement = Modify.SetCustomData(framingElement, familyInstance, pullSettings.ConvertUnits) as IFramingElement;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(framingElement);

            return framingElement;
        }

        /***************************************************/

        private static double FramingElementRotation(this FamilyInstance familyInstance, PullSettings pullSettings)
        {
            double rotation = double.NaN;
            Location location = familyInstance.Location;

            if (location is LocationPoint)
                rotation = Math.PI * 0.5 + (location as LocationPoint).Rotation;
            else if (location is LocationCurve)
            {
                BH.oM.Geometry.ICurve locationCurve = (location as LocationCurve).Curve.ToBHoM(pullSettings);
                if (locationCurve is BH.oM.Geometry.Line)
                {
                    Transform transform = familyInstance.GetTotalTransform();
                    if (BHS.Query.IsVertical(locationCurve as BH.oM.Geometry.Line))
                    {
                        if (familyInstance.IsSlantedColumn)
                            rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisX, transform.BasisZ);
                        else
                            rotation = XYZ.BasisY.AngleOnPlaneTo(transform.BasisY, transform.BasisX);
                    }
                    else
                    {
                        if (familyInstance.IsSlantedColumn)
                            rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisY, transform.BasisZ);
                        else
                            rotation = XYZ.BasisZ.AngleOnPlaneTo(transform.BasisZ, transform.BasisX);
                    }
                }
                else
                {
                    if (IsVerticalNonLinearCurve((location as LocationCurve).Curve))
                        rotation = Math.PI * 0.5 - familyInstance.LookupDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, false);
                    else
                        rotation = -familyInstance.LookupDouble(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE, false);

                    if (familyInstance.Mirrored)
                        rotation *= -1;
                }
            }

            return rotation;
        }

        /***************************************************/

        private static void FramingElementLocation(this FamilyInstance familyInstance, PullSettings pullSettings, out oM.Geometry.ICurve locationCurve, out BH.oM.Geometry.Vector startOffset, out BH.oM.Geometry.Vector endOffset)
        {
            locationCurve = null;
            startOffset = new oM.Geometry.Vector { X = 0, Y = 0, Z = 0 };
            endOffset = new oM.Geometry.Vector { X = 0, Y = 0, Z = 0 };

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
                    locationCurve = new oM.Geometry.Line { Start = baseNode.ToBHoM(pullSettings), End = topNode.ToBHoM(pullSettings) };
                }
            }
            else if (location is LocationCurve)
            {
                locationCurve = (location as LocationCurve).Curve.ToBHoM(pullSettings);
                int yzJustification = familyInstance.LookupInteger(BuiltInParameter.YZ_JUSTIFICATION);

                if (yzJustification == 0)
                {
                    double yOffset = familyInstance.LookupDouble(BuiltInParameter.Y_OFFSET_VALUE, false);
                    double zOffset = familyInstance.LookupDouble(BuiltInParameter.Z_OFFSET_VALUE, false);
                    if (double.IsNaN(yOffset))
                        yOffset = 0;
                    if (double.IsNaN(zOffset))
                        zOffset = 0;

                    int yJustification = familyInstance.LookupInteger(BuiltInParameter.Y_JUSTIFICATION);
                    int zJustification = familyInstance.LookupInteger(BuiltInParameter.Z_JUSTIFICATION);

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

                    startOffset = (new XYZ(0, yOffset, zOffset)).ToBHoMVector(pullSettings);
                    endOffset = (new XYZ(0, yOffset, zOffset)).ToBHoMVector(pullSettings);
                }
                else if (yzJustification == 1)
                {
                    double yOffsetStart = familyInstance.LookupDouble(BuiltInParameter.START_Y_OFFSET_VALUE, false);
                    double yOffsetEnd = familyInstance.LookupDouble(BuiltInParameter.END_Y_OFFSET_VALUE, false);
                    double zOffsetStart = familyInstance.LookupDouble(BuiltInParameter.START_Z_OFFSET_VALUE, false);
                    double zOffsetEnd = familyInstance.LookupDouble(BuiltInParameter.END_Z_OFFSET_VALUE, false);
                    if (double.IsNaN(yOffsetStart))
                        yOffsetStart = 0;
                    if (double.IsNaN(yOffsetEnd))
                        yOffsetEnd = 0;
                    if (double.IsNaN(zOffsetStart))
                        zOffsetStart = 0;
                    if (double.IsNaN(zOffsetEnd))
                        zOffsetEnd = 0;

                    int yJustificationStart = familyInstance.LookupInteger(BuiltInParameter.START_Y_JUSTIFICATION);
                    int yJustificationEnd = familyInstance.LookupInteger(BuiltInParameter.END_Y_JUSTIFICATION);
                    int zJustificationStart = familyInstance.LookupInteger(BuiltInParameter.START_Z_JUSTIFICATION);
                    int zJustificationEnd = familyInstance.LookupInteger(BuiltInParameter.END_Z_JUSTIFICATION);

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

                    startOffset = (new XYZ(0, yOffsetStart, zOffsetStart)).ToBHoMVector(pullSettings);
                    endOffset = (new XYZ(0, yOffsetEnd, zOffsetEnd)).ToBHoMVector(pullSettings);
                }
            }
        }

        /***************************************************/

        private static bool IsVerticalNonLinearCurve(Curve revitCurve)
        {
            if (!(revitCurve is Line))
            {
                CurveLoop curveLoop = CurveLoop.Create(new Curve[] { revitCurve });
                if (curveLoop.HasPlane())
                {
                    Plane curvePlane = curveLoop.GetPlane();
                    //Orientation angles are handled slightly differently for framing elements that have a curve fits in a plane that contains the z-vector
                    if (Math.Abs(curvePlane.Normal.DotProduct(XYZ.BasisZ)) < BH.oM.Geometry.Tolerance.Angle)
                        return true;
                }
            }
            return false;
        }

        /***************************************************/
    }
}