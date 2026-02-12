/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****            Interface methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.IFramingElement to a Revit FamilyInstance.")]
        [Input("framingElement", "BH.oM.Physical.Elements.IFramingElement to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "Revit FamilyInstance resulting from converting the input BH.oM.Physical.Elements.IFramingElement.")]
        public static FamilyInstance IToRevitFamilyInstance(this IFramingElement framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<long>> refObjects = null)
        {
            return ToRevitFamilyInstance(framingElement as dynamic, document, settings, refObjects);
        }


        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Column to a Revit FamilyInstance.")]
        [Input("framingElement", "BH.oM.Physical.Elements.Column to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "Revit FamilyInstance resulting from converting the input BH.oM.Physical.Elements.Column.")]
        public static FamilyInstance ToRevitFamilyInstance(this Column framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<long>> refObjects = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            BH.oM.Geometry.Line locationLine = framingElement.VerticalElementLocation(settings);
            if (locationLine == null)
                return null;

            Level level = document.LevelBelow(locationLine, settings);
            if (level == null)
                return null;

            Line columnLine = locationLine.IToRevit() as Line;

            FamilySymbol familySymbol = framingElement.ElementType(document, settings);
            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(framingElement);
                return null;
            }

            FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;
            if (familyPlacementType != FamilyPlacementType.CurveBased && familyPlacementType != FamilyPlacementType.CurveBasedDetail && familyPlacementType != FamilyPlacementType.CurveDrivenStructural && familyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeError(framingElement, familySymbol);
                return null;
            }

            familyInstance = document.Create.NewFamilyInstance(columnLine, familySymbol, level, StructuralType.Column);
            document.Regenerate();

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Material material = document.GetElement(barProperty.Material.ElementId()) as Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Base.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }

            // Make sure the top is above base, otherwise Revit will complain for no reason.
            familyInstance.get_Parameter((BuiltInParameter.FAMILY_BASE_LEVEL_OFFSET_PARAM)).Set(-1e+3);
            familyInstance.get_Parameter((BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM)).Set(1e+3);

            if (locationLine.IsVertical())
                familyInstance.SetParameter(BuiltInParameter.SLANTED_COLUMN_TYPE_PARAM, 0);

            familyInstance.CopyParameters(framingElement, settings);
            familyInstance.SetLocation(framingElement, settings);

            refObjects.AddOrReplace(framingElement, familyInstance);
            return familyInstance;
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.Pile to a Revit FamilyInstance.")]
        [Input("framingElement", "BH.oM.Physical.Elements.Pile to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "Revit FamilyInstance resulting from converting the input BH.oM.Physical.Elements.Pile.")]
        public static FamilyInstance ToRevitFamilyInstance(this Pile framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<long>> refObjects = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            BH.oM.Geometry.Line locationLine = framingElement.VerticalElementLocation(settings);
            if (locationLine == null)
                return null;

            if (1 - Math.Abs(locationLine.Direction().DotProduct(BH.oM.Geometry.Vector.ZAxis)) > settings.AngleTolerance)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of nonvertical piles is currently not supported. Pile BHoM_Guid: {framingElement.BHoM_Guid}");
                return null;
            }

            Level level = document.LevelBelow(locationLine, settings, true);
            if (level == null)
                return null;

            FamilySymbol familySymbol = framingElement.ElementType(document, settings);
            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(framingElement);
                return null;
            }

            FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;

            // Piles must use OneLevelBased placement type for proper structural foundation behavior
            if (familyPlacementType != FamilyPlacementType.OneLevelBased)
            {
                BH.Engine.Base.Compute.RecordError($"Pile family must use OneLevelBased placement type. Current type: {familyPlacementType}. Please use an appropriate pile family. Pile BHoM_Guid: {framingElement.BHoM_Guid}");
                return null;
            }

            // Extract base point from pile line geometry (use the lower Z point as base)
            BH.oM.Geometry.Point basePoint = locationLine.Start.Z < locationLine.End.Z ? locationLine.Start : locationLine.End;
            XYZ pileBasePoint = basePoint.ToRevit();

            // Use point-based creation for piles as they are typically placed by point with height constraints
            familyInstance = document.Create.NewFamilyInstance(pileBasePoint, familySymbol, level, StructuralType.Footing);
            document.Regenerate();

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty pileProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (pileProperty != null)
            {
                //TODO: if the material does not get assigned an error should be thrown?
                if (pileProperty.Material != null)
                {
                    Material material = document.GetElement(pileProperty.Material.ElementId()) as Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Base.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }

            familyInstance.CopyParameters(framingElement, settings);
            familyInstance.SetLocation(framingElement, settings);

            refObjects.AddOrReplace(framingElement, familyInstance);
            return familyInstance;
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.PadFoundation to a Revit FamilyInstance.")]
        [Input("framingElement", "BH.oM.Physical.Elements.PadFoundation to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "Revit FamilyInstance resulting from converting the input BH.oM.Physical.Elements.PadFoundation.")]
        public static FamilyInstance ToRevitFamilyInstance(this PadFoundation framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<long>> refObjects = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            List<BH.oM.Geometry.Line> boundary = ExtractBoundary(framingElement);
            if (boundary == null || boundary.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError($"Failed to extract boundary from PadFoundation. BHoM_Guid: {framingElement.BHoM_Guid}");
                return null;
            }

            if (!IsRectangular(boundary))
            {
                BH.Engine.Base.Compute.RecordError($"PadFoundation must be rectangular. BHoM_Guid: {framingElement.BHoM_Guid}");
                return null;
            }

            BH.oM.Geometry.Point centerPoint = CalculateBoundaryCenter(boundary);

            BH.oM.Geometry.CoordinateSystem.Cartesian localCS = new BH.oM.Geometry.CoordinateSystem.Cartesian(
                centerPoint,
                BH.oM.Geometry.Vector.XAxis,
                BH.oM.Geometry.Vector.YAxis,
                BH.oM.Geometry.Vector.ZAxis
            );

            FamilySymbol familySymbol = framingElement.ElementType(document, settings) as FamilySymbol;
            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(framingElement);
                return null;
            }

            BH.oM.Geometry.TransformMatrix orientationMatrix = BH.Engine.Geometry.Create.OrientationMatrixGlobalToLocal(localCS);
            Transform transform = orientationMatrix.ToRevit().TryFixIfNonConformal();

            XYZ origin = transform.Origin;
            Level level = document.LevelBelow(origin, settings);
            if (level == null)
                return null;

            familyInstance = document.Create.NewFamilyInstance(origin, familySymbol, level, StructuralType.Footing);
            document.Regenerate();

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            familyInstance.CopyParameters(framingElement, settings);
            familyInstance.SetLocation(framingElement as IFramingElement, settings);

            refObjects.AddOrReplace(framingElement, familyInstance);
            return familyInstance;
        }

        private static List<BH.oM.Geometry.Line> ExtractBoundary(PadFoundation element)
        {
            if (element.Location == null)
                return null;

            List<BH.oM.Geometry.Line> boundary = new List<BH.oM.Geometry.Line>();

            if (element.Location is BH.oM.Geometry.PlanarSurface surface && surface.ExternalBoundary != null)
            {
                boundary.AddRange(
                    surface.ExternalBoundary
                        .SelectMany(curve => curve is BH.oM.Geometry.PolyCurve polyCurve ? polyCurve.Curves : new[] { curve })
                        .OfType<BH.oM.Geometry.Line>()
                );
            }

            return boundary.Count > 0 ? boundary : null;
        }

        private static bool IsRectangular(List<BH.oM.Geometry.Line> edges)
        {
            if (edges == null || edges.Count != 4)
                return false;

            double[] lengths = new double[4];
            for (int i = 0; i < 4; i++)
                lengths[i] = edges[i].Length();

            double tolerance = 0.001; 
            if (Math.Abs(lengths[0] - lengths[2]) > tolerance || Math.Abs(lengths[1] - lengths[3]) > tolerance)
                return false;

            BH.oM.Geometry.Point p1 = edges[0].Start;
            BH.oM.Geometry.Point p2 = edges[0].End;
            BH.oM.Geometry.Point p3 = edges[1].End;
            BH.oM.Geometry.Point p4 = edges[2].End;

            var d1 = (p1 - p3).Length();
            var d2 = (p2 - p4).Length(); 

            return Math.Abs(d1 - d2) <= tolerance;
        }

        private static BH.oM.Geometry.Point CalculateBoundaryCenter(List<BH.oM.Geometry.Line> boundary)
        {
            if (boundary == null || boundary.Count == 0)
                return oM.Geometry.Point.Origin;

            List<BH.oM.Geometry.Point> boundaryPoints = new List<BH.oM.Geometry.Point>();
            foreach (var line in boundary)
            {
                boundaryPoints.Add(line.Start);
                boundaryPoints.Add(line.End);
            }

            if (boundaryPoints.Count > 0)
            {
                double sumX = 0, sumY = 0, sumZ = 0;
                foreach (var point in boundaryPoints)
                {
                    sumX += point.X;
                    sumY += point.Y;
                    sumZ += point.Z;
                }

                return new oM.Geometry.Point(
                    sumX / boundaryPoints.Count,
                    sumY / boundaryPoints.Count,
                    sumZ / boundaryPoints.Count
                );
            }

            return BH.oM.Geometry.Point.Origin;
        }

        /***************************************************/

        [Description("Converts BH.oM.Physical.Elements.IFramingElement to a Revit FamilyInstance.")]
        [Input("framingElement", "BH.oM.Physical.Elements.IFramingElement to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("instance", "Revit FamilyInstance resulting from converting the input BH.oM.Physical.Elements.IFramingElement.")]
        public static FamilyInstance ToRevitFamilyInstance(this IFramingElement framingElement, Document document, RevitSettings settings = null, Dictionary<Guid, List<long>> refObjects = null)
        {
            if (framingElement == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, framingElement.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            if (framingElement.Location == null)
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Revit element could not be created because the driving curve of a BHoM object is null. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            if (!framingElement.Location.IIsPlanar())
            {
                BH.Engine.Base.Compute.RecordError(string.Format("Revit framing does only support planar curves, element could not be created. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            Curve revitCurve = framingElement.Location.IToRevit();
            if (revitCurve == null)
            {
                BH.Engine.Base.Compute.RecordError(string.Format("Revit element could not be created because of curve conversion issues. BHoM_Guid: {0}", framingElement.BHoM_Guid));
                return null;
            }

            Level level = document.LevelBelow(framingElement.Location, settings);

            FamilySymbol familySymbol = framingElement.ElementType(document, settings);
            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(framingElement);
                return null;
            }

            FamilyPlacementType familyPlacementType = familySymbol.Family.FamilyPlacementType;
            if (familyPlacementType != FamilyPlacementType.CurveBased && familyPlacementType != FamilyPlacementType.CurveBasedDetail && familyPlacementType != FamilyPlacementType.CurveDrivenStructural && familyPlacementType != FamilyPlacementType.TwoLevelsBased)
            {
                Compute.InvalidFamilyPlacementTypeError(framingElement, familySymbol);
                return null;
            }

            if (framingElement is Beam)
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, StructuralType.Beam);
            else if (framingElement is Bracing || framingElement is Cable)
            {
                Line revitLine = revitCurve as Line;
                if (revitLine == null)
                {
                    BH.Engine.Base.Compute.RecordError($"Nonlinear bracing is not allowed, please consider using beam type instead. BHoM_Guid: {framingElement.BHoM_Guid}");
                    return null;
                }

                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, StructuralType.Brace);
                if (framingElement is Cable)
                    BH.Engine.Base.Compute.RecordWarning($"Revit does not support Cable type, the BHoM cable has been converted to a Revit bracing. BHoM_Guid: {framingElement.BHoM_Guid}, ElementId: {familyInstance.Id}");

                if (Math.Abs(revitLine.Direction.DotProduct(XYZ.BasisZ)) < settings.AngleTolerance)
                {
                    familyInstance.StructuralUsage = StructuralInstanceUsage.Other;
                    BH.Engine.Base.Compute.RecordWarning($"The driving curve of a bracing element is horizontal, structural usage of the Revit element has been set to 'Other'. BHoM_Guid: {framingElement.BHoM_Guid}, ElementId: {familyInstance.Id}");
                }
            }
            else
                familyInstance = document.Create.NewFamilyInstance(revitCurve, familySymbol, level, StructuralType.UnknownFraming);

            // Try enforcing reference level to the element (something Create.NewFamilyInstance does not set it without a reason).
            familyInstance.SetParameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM, level.Id);
            document.Regenerate();

            familyInstance.CheckIfNullPush(framingElement);
            if (familyInstance == null)
                return null;

            oM.Physical.FramingProperties.ConstantFramingProperty barProperty = framingElement.Property as oM.Physical.FramingProperties.ConstantFramingProperty;
            if (barProperty != null)
            {
                //TODO: if the material does not get assigned an error should be thrown?
                if (barProperty.Material != null)
                {
                    Material material = document.GetElement(barProperty.Material.ElementId()) as Material;
                    if (material != null)
                    {
                        Parameter param = familyInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                        if (param != null && param.HasValue && !param.IsReadOnly)
                            familyInstance.StructuralMaterialId = material.Id;
                        else
                            BH.Engine.Base.Compute.RecordWarning(string.Format("The BHoM material has been correctly converted, but the property could not be assigned to the Revit element. ElementId: {0}", familyInstance.Id));
                    }
                }
            }

            //Set the insertion point to centroid. 
            Parameter zJustification = familyInstance.get_Parameter(BuiltInParameter.Z_JUSTIFICATION);
            if (zJustification != null && !zJustification.IsReadOnly)
                zJustification.Set((int)Autodesk.Revit.DB.Structure.ZJustification.Origin);

            //Set the extension values to zero.
            familyInstance.SetParameter(BuiltInParameter.START_EXTENSION, 0.0, false);
            familyInstance.SetParameter(BuiltInParameter.END_EXTENSION, 0.0, false);

            familyInstance.CopyParameters(framingElement, settings);
            familyInstance.SetLocation(framingElement, settings);

            if (familyInstance.StructuralMaterialType != StructuralMaterialType.Concrete && familyInstance.StructuralMaterialType != StructuralMaterialType.PrecastConcrete)
            {
                StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 0);
                StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 1);
            }

            refObjects.AddOrReplace(framingElement, familyInstance);
            return familyInstance;
        }

        /***************************************************/
    }
}






