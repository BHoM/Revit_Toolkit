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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.Engine.Spatial;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Generates a Revit family type (and its parent family, if not loaded yet) that represents the profile of a given BHoM framing element." +
                     "\nThe profile is created based on the template families stored in C:\\ProgramData\\Resources\\Revit.")]
        [Input("element", "BHoM framing element to generate the Revit profile for.")]
        [Input("document", "Revit document, in which the family type will be created.")]
        [Input("settings", "Settings to be used when generating the family type.")]
        [Output("symbol", "Created Revit family type that represents the profile of the input BHoM framing element.")]
        public static FamilySymbol GenerateProfile(this IFramingElement element, Document document, RevitSettings settings = null)
        {
            ConstantFramingProperty property = element?.Property as ConstantFramingProperty;
            if (property?.Profile == null)
            {
                BH.Engine.Base.Compute.RecordError($"The BHoM framing element is null or does not have a valid profile. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            string familyName = element.ProfileFamilyName();
            if (string.IsNullOrWhiteSpace(familyName))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a Revit profile family failed because neither the BHoM section property nor its profile has a name. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            settings = settings.DefaultIfNull();

            Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).FirstOrDefault(x => x.Name == familyName) as Family;
            if (family != null)
            {
                List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(x => document.GetElement(x) as FamilySymbol).Where(x => x != null).ToList();
                FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == element.Property.Name);
                if (result == null && symbols.Count != 0)
                {
                    result = symbols[0].Duplicate(element.Property.Name) as FamilySymbol;
                    property.Profile.ICopyDimensions(result, settings);
                }

                return result;
            }
            else
            {
                bool freeform = false;

                family = document.LoadTemplateProfileFamily(element);
                if (family != null)
                    family.Name = familyName;
                else
                {
                    family = GenerateFreeformFamily(document, element, settings);
                    if (family == null)
                        return null;

                    if (!(property.Profile is FreeFormProfile))
                        BH.Engine.Base.Compute.RecordWarning($"Generation of profiles with shape {property.Profile.GetType().Name} is currently not fully supported - a freeform, dimensionless profile with a dedicated family has been created.");

                    freeform = true;
                }

                family.SetMaterialForModelBehaviour(property?.Material);

                FamilySymbol result = document.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                if (result == null)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Generation of a Revit family representing the section property of a BHoM framing element failed due to an internal error. BHoM_Guid: {element.BHoM_Guid}");
                    return null;
                }

                result.Activate();
                result.Name = element.ProfileTypeName();

                if (!freeform)
                    element.ICopyProfileDimensions(result, settings);

                return result;
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Family LoadTemplateProfileFamily(this Document document, IFramingElement element)
        {
            string templateFamilyName = element.TemplateProfileFamilyName();
            if (string.IsNullOrWhiteSpace(templateFamilyName))
                return null;

            string path = $"{m_FamilyDirectory}\\{templateFamilyName}.rfa";
            if (!System.IO.File.Exists(path))
                return null;

            Family family;
            document.LoadFamily(path, out family);
            return family;
        }

        /***************************************************/

        private static Family GenerateFreeformFamily(this Document document, IFramingElement element, RevitSettings settings = null)
        {
            ConstantFramingProperty property = element?.Property as ConstantFramingProperty;

            List<BH.oM.Geometry.ICurve> edges = property?.Profile?.Edges?.ToList();
            if (edges == null || edges.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit profile geometry failed because the BHoM section property does not have a valid geometrical profile. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            if (edges.Any(x => x.Length() < 1e-3))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit profile geometry failed because one of the edges defining the BHoM section property is less than 1mm long (this is a Revit limitation). Please simplify the profile and try again. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            List<BH.oM.Geometry.PolyCurve> edgeLoops = edges.IJoin(settings.DistanceTolerance);
            if (edgeLoops.Any(x => !x.IsClosed(settings.DistanceTolerance)))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit profile geometry failed because one of the edge loops coming from the BHoM section property was not closed. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            Family result = null;
            UIDocument uidoc = new UIDocument(document);
            Document familyDocument = uidoc.Application.Application.OpenDocumentFile($"{m_FamilyDirectory}\\{element.IProfileFamilyNamePrefix()}FreeformProfile.rfa");

            try
            {
                Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).FirstElement() as Extrusion;
                BH.oM.Geometry.CoordinateSystem.Cartesian coordinateSystem = extrusion.Sketch.SketchPlane.GetPlane().FromRevit();
                BH.oM.Geometry.TransformMatrix transform = BH.Engine.Geometry.Create.OrientationMatrix(new BH.oM.Geometry.CoordinateSystem.Cartesian(), coordinateSystem);
                edgeLoops = edgeLoops.Select(x => x.Transform(transform)).ToList();

                CurveArrArray newProfile = new CurveArrArray();
                foreach (BH.oM.Geometry.PolyCurve loop in edgeLoops)
                {
                    newProfile.Append(loop.ToRevitCurveArray());
                }

                (Reference, Reference, double, XYZ, View) constraints = element.IExtrusionConstraints(familyDocument);
                Reference startRef = constraints.Item1;
                Reference endRef = constraints.Item2;
                double length = constraints.Item3;
                XYZ dir = constraints.Item4;
                View constraintView = constraints.Item5;
                if (startRef == null || endRef == null || double.IsNaN(length) || dir == null || constraintView == null)
                    return null;

                using (Transaction t = new Transaction(familyDocument, "Update Extrusion"))
                {
                    t.Start();

                    SketchPlane sketchPlane = SketchPlane.Create(familyDocument, startRef.ElementId);
                    Extrusion newExtrusion = familyDocument.FamilyCreate.NewExtrusion(true, newProfile, sketchPlane, length);

                    Options opt = new Options();
                    opt.ComputeReferences = true;
                    List<Face> extrusionFaces = newExtrusion.Faces(opt);
                    Face startFace = extrusionFaces.Where(x => x is PlanarFace).FirstOrDefault(x => ((PlanarFace)x).FaceNormal.IsAlmostEqualTo(-dir));
                    Face endFace = extrusionFaces.Where(x => x is PlanarFace).FirstOrDefault(x => ((PlanarFace)x).FaceNormal.IsAlmostEqualTo(dir));
                    familyDocument.FamilyCreate.NewAlignment(constraintView, startRef, startFace.Reference);
                    familyDocument.FamilyCreate.NewAlignment(constraintView, endRef, endFace.Reference);

                    familyDocument.Delete(extrusion.Id);
                    t.Commit();
                }

                string tempFolder = Path.GetTempPath();
                string tempLocation = SanitizePath($"{tempFolder}\\{element.ProfileFamilyName()}.rfa");
                try
                {
                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);

                    SaveAsOptions saveOptions = new SaveAsOptions();
                    saveOptions.OverwriteExistingFile = true;
                    familyDocument.SaveAs(tempLocation, saveOptions);
                }
                catch (Exception ex)
                {
                    BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit profile geometry failed because the family could not be temporarily saved in {tempFolder}. Please make sure the folder exists and you have access to it, then try to empty it in case the issue persists.");
                    familyDocument.Close(false);
                    return null;
                }

                document.LoadFamily(tempLocation, out result);

                try
                {
                    File.Delete(tempLocation);
                }
                catch (Exception ex)
                {
                    BH.Engine.Base.Compute.RecordNote($"File {tempLocation} could not be deleted.");
                }
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit profile geometry failed with the following error: {ex.Message}");
            }
            finally
            {
                familyDocument.Close(false);
            }

            return result;
        }

        /***************************************************/

        private static bool SetMaterialForModelBehaviour(this Family family, BH.oM.Physical.Materials.Material material)
        {
            List<BH.oM.Physical.Materials.IMaterialProperties> structuralProperties = material?.Properties?.Where(x => x is BH.oM.Structure.MaterialFragments.IMaterialFragment)?.ToList();
            if (structuralProperties == null || structuralProperties.Count == 0)
            {
                BH.Engine.Base.Compute.RecordWarning($"Material for model behaviour of family {family.Name} could not be set because the source BHoM section property does not contain relevant material information. ElementId: {family.Id}");
                family.SetParameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, (int)Autodesk.Revit.DB.Structure.StructuralMaterialType.Other);
                return false;
            }
            else if (structuralProperties.Select(x => x.GetType().FullName).Distinct().Count() > 1)
            {
                BH.Engine.Base.Compute.RecordWarning($"Material for model behaviour of family {family.Name} could not be set because the source BHoM section property contains more than one relevant material information. ElementId: {family.Id}");
                family.SetParameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, (int)Autodesk.Revit.DB.Structure.StructuralMaterialType.Other);
                return false;
            }

            Type materialType = structuralProperties[0].GetType();
            if (materialType == typeof(BH.oM.Structure.MaterialFragments.Steel))
                return family.SetParameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, (int)Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel);
            else if (materialType == typeof(BH.oM.Structure.MaterialFragments.Concrete))
                return family.SetParameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, (int)Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete);
            else if (materialType == typeof(BH.oM.Structure.MaterialFragments.Timber))
                return family.SetParameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, (int)Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood);
            else
            {
                BH.Engine.Base.Compute.RecordWarning($"Material for model behaviour of family {family.Name} has been set to \'Other\' set because the material of source BHoM section property is not supported by Revit in this context. ElementId: {family.Id}");
                return family.SetParameter(BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, (int)Autodesk.Revit.DB.Structure.StructuralMaterialType.Other);
            }
        }

        /***************************************************/

        private static void ICopyProfileDimensions(this IFramingElement element, FamilySymbol targetSymbol, RevitSettings settings = null)
        {

            CopyProfileDimensions(element as dynamic, targetSymbol, settings);
        }

        /***************************************************/

        private static void CopyProfileDimensions(this Pile element, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            IProfile sourceProfile = (element?.Property as ConstantFramingProperty)?.Profile;
            if (sourceProfile == null)
            {
                BH.Engine.Base.Compute.RecordError($"Could not copy profile dimensions because the BHoM framing element is null or does not have a valid profile. BHoM_Guid: {element.BHoM_Guid}");
                return;
            }

            CopyPileDimensions(sourceProfile as dynamic, targetSymbol, settings);
        }

        /***************************************************/

        private static void CopyProfileDimensions(this IFramingElement element, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            IProfile sourceProfile = (element?.Property as ConstantFramingProperty)?.Profile;
            if (sourceProfile == null)
            {
                BH.Engine.Base.Compute.RecordError($"Could not copy profile dimensions because the BHoM framing element is null or does not have a valid profile. BHoM_Guid: {element.BHoM_Guid}");
                return;
            }

            CopyDimensions(sourceProfile as dynamic, targetSymbol, settings);
        }

        /***************************************************/

        private static void ICopyDimensions(this IProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            CopyDimensions(sourceProfile as dynamic, targetSymbol, settings);
        }

        /***************************************************/

        private static void CopyDimensions(this ISectionProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);

            if (sourceProfile.ToeRadius > settings.DistanceTolerance)
                BH.Engine.Base.Compute.RecordWarning($"Toe radius of a profile has been ignored when generating profile. ElementId: {targetSymbol.Id}");
        }

        /***************************************************/

        private static void CopyDimensions(this AngleProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_TOP_WEB_FILLET, sourceProfile.ToeRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET, sourceProfile.ToeRadius);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ, -bounds.Min.X);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL, -bounds.Min.Y);

            if (sourceProfile.MirrorAboutLocalZ || sourceProfile.MirrorAboutLocalY)
                BH.Engine.Base.Compute.RecordWarning($"Profile of the BHoM section property is mirrored against one of its axes - this information has been ignored while creating the Revit family type on the fly.");
        }

        /***************************************************/

        private static void CopyDimensions(this BoxProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_PIPESTANDARD_WALLNOMINALTHICKNESS, sourceProfile.Thickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_HSS_INNERFILLET, sourceProfile.InnerRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_HSS_OUTERFILLET, sourceProfile.OuterRadius);
        }

        /***************************************************/

        private static void CopyDimensions(this ChannelProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.FlangeWidth);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGE_TOE_OF_FILLET, sourceProfile.ToeRadius);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ, -bounds.Min.X);

            if (sourceProfile.MirrorAboutLocalZ)
                BH.Engine.Base.Compute.RecordWarning($"Profile of the BHoM section property is mirrored against its axis - this information has been ignored while creating the Revit family type on the fly.");
        }

        /***************************************************/

        private static void CopyDimensions(this FabricatedISectionProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_IWELDED_TOPFLANGEWIDTH, sourceProfile.TopFlangeWidth);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_IWELDED_BOTTOMFLANGEWIDTH, sourceProfile.BotFlangeWidth);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_IWELDED_TOPFLANGETHICKNESS, sourceProfile.TopFlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_IWELDED_BOTTOMFLANGETHICKNESS, sourceProfile.BotFlangeThickness);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL, -bounds.Min.Y);
        }

        /***************************************************/

        private static void CopyDimensions(this CircleProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_DIAMETER, sourceProfile.Diameter);
        }

        /***************************************************/

        private static void CopyDimensions(this RectangleProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);

            if (sourceProfile.CornerRadius > settings.DistanceTolerance)
                BH.Engine.Base.Compute.RecordWarning($"Corner radius of a profile has been ignored when generating the rectangular profile. ElementId: {targetSymbol.Id}");
        }

        /***************************************************/

        private static void CopyDimensions(this TaperFlangeChannelProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.FlangeWidth);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_SLOPED_FLANGE_ANGLE, sourceProfile.FlangeSlope);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET, sourceProfile.ToeRadius);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ, -bounds.Min.X);

            if (sourceProfile.MirrorAboutLocalZ)
                BH.Engine.Base.Compute.RecordWarning($"Profile of the BHoM section property is mirrored against its axis - this information has been ignored while creating the Revit family type on the fly.");
        }

        /***************************************************/

        private static void CopyDimensions(this TaperFlangeISectionProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_SLOPED_FLANGE_ANGLE, sourceProfile.FlangeSlope);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET, sourceProfile.ToeRadius);
        }

        /***************************************************/

        private static void CopyDimensions(this TSectionProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET, sourceProfile.ToeRadius);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL, bounds.Max.Y);

            if (sourceProfile.MirrorAboutLocalY)
                BH.Engine.Base.Compute.RecordWarning($"Profile of the BHoM section property is mirrored against its axis - this information has been ignored while creating the Revit family type on the fly.");
        }

        /***************************************************/

        private static void CopyDimensions(this TubeProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_DIAMETER, sourceProfile.Diameter);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_PIPESTANDARD_WALLNOMINALTHICKNESS, sourceProfile.Thickness);
        }

        /***************************************************/

        private static void CopyDimensions(this FabricatedBoxProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter("Web Thickness", sourceProfile.WebThickness);
            targetSymbol.SetParameter("Top Flange Thickness", sourceProfile.TopFlangeThickness);
            targetSymbol.SetParameter("Bottom Flange Thickness", sourceProfile.BotFlangeThickness);
            targetSymbol.SetParameter("Top Left Corbel Width", 0.0);
            targetSymbol.SetParameter("Top Right Corbel Width", 0.0);
            targetSymbol.SetParameter("Bottom Left Corbel Width", 0.0);
            targetSymbol.SetParameter("Bottom Right Corbel Width", 0.0);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ, -bounds.Min.X);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL, -bounds.Min.Y);

            if (sourceProfile.WeldSize > settings.DistanceTolerance)
                BH.Engine.Base.Compute.RecordWarning($"Weld size has been ignored when generating the fabricated box profile. ElementId: {targetSymbol.Id}");
        }

        /***************************************************/

        private static void CopyDimensions(this GeneralisedFabricatedBoxProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter("Web Thickness", sourceProfile.WebThickness);
            targetSymbol.SetParameter("Top Flange Thickness", sourceProfile.TopFlangeThickness);
            targetSymbol.SetParameter("Bottom Flange Thickness", sourceProfile.BotFlangeThickness);
            targetSymbol.SetParameter("Top Left Corbel Width", sourceProfile.TopLeftCorbelWidth);
            targetSymbol.SetParameter("Top Right Corbel Width", sourceProfile.TopRightCorbelWidth);
            targetSymbol.SetParameter("Bottom Left Corbel Width", sourceProfile.BotLeftCorbelWidth);
            targetSymbol.SetParameter("Bottom Right Corbel Width", sourceProfile.BotRightCorbelWidth);

            BH.oM.Geometry.BoundingBox bounds = sourceProfile.Bounds();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ, -bounds.Min.X);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL, -bounds.Min.Y);
        }

        /***************************************************/

        private static void CopyPileDimensions(this CircleProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            targetSymbol.SetParameter("Diameter", sourceProfile.Diameter);
        }

        /***************************************************/

        private static string ProfileFamilyName(this IFramingElement element)
        {
            string propertyName = element?.Property?.Name;
            string profileName = (element?.Property as ConstantFramingProperty)?.Profile?.Name;
            if (string.IsNullOrWhiteSpace(propertyName) && string.IsNullOrWhiteSpace(profileName))
                return null;

            if (!string.IsNullOrWhiteSpace(propertyName) && !string.IsNullOrWhiteSpace(profileName) && profileName != propertyName)
                BH.Engine.Base.Compute.RecordWarning("The BHoM object has different section property and profile names - section property name has been used to create Revit profile family.");

            string name = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : profileName;
            if (name.Contains(':'))
                return name.Split(':')[0].Trim();
            else
            {
                string prefix = element.TemplateProfileFamilyName();
                Regex pattern = new Regex(@"\d([\d\.\/\-xX ])*\d");
                return $"{prefix}_{pattern.Replace(name, "").Replace("  ", " ").Trim()}";
            }
        }

        /***************************************************/

        private static string ProfileTypeName(this IFramingElement element)
        {
            string propertyName = element?.Property?.Name;
            string profileName = (element?.Property as ConstantFramingProperty)?.Profile?.Name;
            if (string.IsNullOrWhiteSpace(propertyName) && string.IsNullOrWhiteSpace(profileName))
                return null;

            string name = !string.IsNullOrWhiteSpace(propertyName) ? propertyName : profileName;
            if (name.Contains(':'))
                return name.Split(':')[1].Trim();
            else
                return name;
        }

        /***************************************************/

        private static string TemplateProfileFamilyName(this IFramingElement element)
        {
            Type profileType = (element?.Property as ConstantFramingProperty)?.Profile?.GetType();
            if (!m_FamilyFileNames.ContainsKey(profileType))
                return null;

            return element.IProfileFamilyNamePrefix() + m_FamilyFileNames[profileType];
        }

        /***************************************************/

        private static string IProfileFamilyNamePrefix(this IFramingElement element)
        {
            return ProfileFamilyNamePrefix(element as dynamic);
        }

        /***************************************************/

        private static string ProfileFamilyNamePrefix(this Column element)
        {
            return "StructuralColumns_";
        }

        /***************************************************/

        private static string ProfileFamilyNamePrefix(this Pile element)
        {
            return "StructuralFoundations_";
        }

        /***************************************************/

        private static string ProfileFamilyNamePrefix(this IFramingElement element)
        {
            return "StructuralFraming_";
        }

        /***************************************************/

        private static (Reference, Reference, double, XYZ, View) IExtrusionConstraints(this IFramingElement element, Document familyDocument)
        {
            return ExtrusionConstraints(element as dynamic, familyDocument);
        }

        /***************************************************/

        private static (Reference, Reference, double, XYZ, View) ExtrusionConstraints(this Column element, Document familyDocument)
        {
            View constraintView = new FilteredElementCollector(familyDocument).OfClass(typeof(View)).FirstOrDefault(x => x.Name == "Front") as View;
            if (constraintView == null)
                return (null, null, double.NaN, null, null);

            Level bottom = new FilteredElementCollector(familyDocument).OfClass(typeof(Level)).FirstOrDefault(x => x.Name == "Lower Ref Level") as Level;
            Level top = new FilteredElementCollector(familyDocument).OfClass(typeof(Level)).FirstOrDefault(x => x.Name == "Upper Ref Level") as Level;
            if (bottom == null || top == null)
                return (null, null, double.NaN, null, null);

            Reference startRef = bottom.GetPlaneReference();
            Reference endRef = top.GetPlaneReference();
            double length = top.ProjectElevation - bottom.ProjectElevation;
            XYZ dir = XYZ.BasisZ;

            return (startRef, endRef, length, dir, constraintView);
        }

        /***************************************************/

        private static (Reference, Reference, double, XYZ, View) ExtrusionConstraints(this Pile element, Document familyDocument)
        {
            View constraintView = new FilteredElementCollector(familyDocument).OfClass(typeof(View)).FirstOrDefault(x => x.Name == "Front") as View;
            if (constraintView == null)
                return (null, null, double.NaN, null, null);

            ReferencePlane bottom = new FilteredElementCollector(familyDocument).OfClass(typeof(ReferencePlane)).FirstOrDefault(x => x.Name == "Bottom Constraint") as ReferencePlane;
            ReferencePlane top = new FilteredElementCollector(familyDocument).OfClass(typeof(ReferencePlane)).FirstOrDefault(x => x.Name == "Top Constraint") as ReferencePlane;
            if (bottom == null || top == null)
                return (null, null, double.NaN, null, null);

            Reference startRef = bottom.GetReference();
            Reference endRef = top.GetReference();
            double length = top.GetPlane().Origin.Z - bottom.GetPlane().Origin.Z;
            XYZ dir = XYZ.BasisZ;

            return (startRef, endRef, length, dir, constraintView);
        }

        /***************************************************/

        private static (Reference, Reference, double, XYZ, View) ExtrusionConstraints(this IFramingElement element, Document familyDocument)
        {
            View constraintView = new FilteredElementCollector(familyDocument).OfClass(typeof(View)).FirstOrDefault(x => x.Name == "Ref. Level") as View;
            if (constraintView == null)
                return (null, null, double.NaN, null, null);

            ReferencePlane left = new FilteredElementCollector(familyDocument).OfClass(typeof(ReferencePlane)).FirstOrDefault(x => x.Name == "Member Left") as ReferencePlane;
            ReferencePlane right = new FilteredElementCollector(familyDocument).OfClass(typeof(ReferencePlane)).FirstOrDefault(x => x.Name == "Member Right") as ReferencePlane;
            if (left == null || right == null)
                return (null, null, double.NaN, null, null);

            Reference endRef = left.GetReference();
            Reference startRef = right.GetReference();
            double length = left.GetPlane().Origin.X - right.GetPlane().Origin.X;
            XYZ dir = -XYZ.BasisX;

            return (startRef, endRef, length, dir, constraintView);
        }

        /***************************************************/

        private static string SanitizePath(string inputPath)
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
            char[] allInvalidChars = invalidPathChars.Concat(invalidFileNameChars).Where(x => x != '\\' && x != ':').Distinct().ToArray();

            foreach (char c in allInvalidChars)
            {
                inputPath = inputPath.Replace(c.ToString(), "");
            }

            return inputPath;
        }


        /***************************************************/
        /****              Fallback methods             ****/
        /***************************************************/

        private static void CopyDimensions(this BH.oM.Spatial.ShapeProfiles.IProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            BH.Engine.Base.Compute.RecordWarning($"Dimensions of a profile with shape {sourceProfile.GetType().Name} could not be copied to the Revit family symbol because such shape is not supported. ElementId: {targetSymbol.Id}");
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static readonly string m_FamilyDirectory = @"C:\ProgramData\BHoM\Resources\Revit\Families";

        private static readonly Dictionary<Type, string> m_FamilyFileNames = new Dictionary<Type, string>
        {
            { typeof(ISectionProfile), "IProfile" },
            { typeof(AngleProfile), "AngleProfile" },
            { typeof(BoxProfile), "BoxProfile" },
            { typeof(ChannelProfile), "ChannelProfile" },
            { typeof(CircleProfile), "CircleProfile" },
            { typeof(FabricatedISectionProfile), "FabricatedIProfile" },
            { typeof(RectangleProfile), "RectangleProfile" },
            { typeof(TaperFlangeChannelProfile), "TaperFlangeChannelProfile" },
            { typeof(TaperFlangeISectionProfile), "TaperFlangeISectionProfile" },
            { typeof(TSectionProfile), "TProfile" },
            { typeof(TubeProfile), "TubeProfile" },
            { typeof(FabricatedBoxProfile), "FabricatedBoxProfile" },
            { typeof(GeneralisedFabricatedBoxProfile), "FabricatedBoxProfile" },
        };

        /***************************************************/
    }
}



