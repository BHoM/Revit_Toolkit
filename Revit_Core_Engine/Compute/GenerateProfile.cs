/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using BH.Engine.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Autodesk.Revit.UI;

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
        public static FamilySymbol GenerateProfile(this BH.oM.Physical.Elements.IFramingElement element, Document document, RevitSettings settings = null)
        {
            BH.oM.Physical.FramingProperties.ConstantFramingProperty property = element?.Property as BH.oM.Physical.FramingProperties.ConstantFramingProperty;
            if (property == null)
            {
                BH.Engine.Base.Compute.RecordError($"The BHoM framing element is null or does not have a valid profile. BHoM_Guid: {element.BHoM_Guid}");
                return null;
            }

            string familyName = element.ProfileFamilyName();
            if (string.IsNullOrWhiteSpace(familyName))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a Revit profile family failed because the source neither the BHoM section property nor its profile has a name. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).FirstOrDefault(x => x.Name == familyName) as Family;
            if (family != null)
            {
                List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(x => document.GetElement(x) as FamilySymbol).Where(x => x != null).ToList();
                FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == element.Property.Name);
                if (result == null && symbols.Count != 0)
                {
                    result = symbols[0].Duplicate(element.Property.Name) as FamilySymbol;
                    property.Profile.ICopyDimensions(result);
                }

                return result;
            }
            else
            {
                bool freeform = false;
                family = document.ILoadProfileFamily(property.Profile);
                if (family != null)
                    family.Name = familyName;
                else
                {
                    family = GenerateFreeformFamily(document, property, settings);
                    if (family == null)
                        return null;

                    BH.Engine.Base.Compute.RecordWarning($"Generation of profiles with shape {property.Profile.GetType().Name} is currently not fully supported - a freeform, dimensionless profile with a dedicated family has been created.");
                    freeform = true;
                }

                family.SetMaterialForModelBehaviour(property?.Material);
                FamilySymbol result = document.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                if (result != null)
                    result.Activate();

                result.Name = element.Property.Name;
                if (!freeform)
                    property.Profile.ICopyDimensions(result);

                return result;
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Family GenerateFreeformFamily(this Document document, BH.oM.Physical.FramingProperties.ConstantFramingProperty property, RevitSettings settings = null)
        {
            List<BH.oM.Geometry.ICurve> edges = property?.Profile?.Edges?.ToList();
            if (edges == null)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a custom Revit profile geometry failed because the BHoM section property does not have a valid geometrical profile. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            settings = settings.DefaultIfNull();
            List<BH.oM.Geometry.PolyCurve> edgeLoops = edges.IJoin(settings.DistanceTolerance);
            if (edgeLoops.Any(x => !x.IsClosed(settings.DistanceTolerance)))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a custom Revit profile geometry failed because one of the edge loops coming from the BHoM section property was not closed. BHoM_Guid: {property.BHoM_Guid}");
                return null;
            }

            UIDocument uidoc = new UIDocument(document);
            Document familyDocument = uidoc.Application.Application.OpenDocumentFile(m_FamilyDirectory + "\\StructuralFraming_Freeform.rfa");

            Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).FirstElement() as Extrusion;
            BH.oM.Geometry.CoordinateSystem.Cartesian coordinateSystem = extrusion.Sketch.SketchPlane.GetPlane().FromRevit();
            BH.oM.Geometry.TransformMatrix transform = BH.Engine.Geometry.Create.OrientationMatrix(new BH.oM.Geometry.CoordinateSystem.Cartesian(), coordinateSystem);
            edgeLoops = edgeLoops.Select(x => x.Transform(transform)).ToList();

            CurveArrArray newProfile = new CurveArrArray();
            foreach (BH.oM.Geometry.PolyCurve loop in edgeLoops)
            {
                newProfile.Append(loop.ToRevitCurveArray());
            }

            double length = extrusion.LookupParameterDouble(BuiltInParameter.EXTRUSION_END_PARAM, false) - extrusion.LookupParameterDouble(BuiltInParameter.EXTRUSION_START_PARAM, false);

            using (Transaction t = new Transaction(familyDocument, "Update Extrusion"))
            {
                t.Start();
                familyDocument.FamilyCreate.NewExtrusion(true, newProfile, extrusion.Sketch.SketchPlane, length);
                familyDocument.Delete(extrusion.Id);
                t.Commit();
            }

            Family result;

            string tempLocation = $"{m_TempFolder}\\{property.Name}.rfa";
            familyDocument.SaveAs(tempLocation);
            document.LoadFamily(tempLocation, out result);

            familyDocument.Close(false);
            System.IO.File.Delete(tempLocation);
            return result;
        }

        /***************************************************/

        private static bool SetMaterialForModelBehaviour(this Family family, BH.oM.Physical.Materials.Material material)
        {
            List<BH.oM.Physical.Materials.IMaterialProperties> structuralProperties = material?.Properties?.Where(x => x is BH.oM.Structure.MaterialFragments.IMaterialFragment)?.ToList();
            if (structuralProperties == null || structuralProperties.Count == 0)
            {
                BH.Engine.Base.Compute.RecordWarning($"Material for model behaviour of family {family.Name} could not be set because the source BHoM section property does not contain relevant material information. ElementId: {family.Id}");
                return false;
            }
            else if (structuralProperties.Select(x => x.GetType().FullName).Distinct().Count() > 1) 
            {
                BH.Engine.Base.Compute.RecordWarning($"Material for model behaviour of family {family.Name} could not be set because the source BHoM section property contains more than one relevant material information. ElementId: {family.Id}");
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

        private static void ICopyDimensions(this BH.oM.Spatial.ShapeProfiles.IProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            CopyDimensions(sourceProfile as dynamic, targetSymbol);
        }

        /***************************************************/

        private static void CopyDimensions(this BH.oM.Spatial.ShapeProfiles.ISectionProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);

            if (sourceProfile.ToeRadius > settings.DistanceTolerance)
                BH.Engine.Base.Compute.RecordWarning($"Toe radius of a profile has been ignored when generating profile. ElementId: {targetSymbol.Id}");
        }

        /***************************************************/

        private static void CopyDimensions(this BH.oM.Spatial.ShapeProfiles.AngleProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT, sourceProfile.Height);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH, sourceProfile.Width);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS, sourceProfile.WebThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS, sourceProfile.FlangeThickness);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET, sourceProfile.RootRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_TOP_WEB_FILLET, sourceProfile.ToeRadius);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET, sourceProfile.ToeRadius);

            BH.oM.Geometry.BoundingBox bounds = new BH.oM.Geometry.BoundingBox();
            foreach (BH.oM.Geometry.ICurve edge in sourceProfile.Edges)
            {
                bounds += edge.IBounds();
            }

            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ, -bounds.Min.X);
            targetSymbol.SetParameter(BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL, -bounds.Min.Y);

            if (sourceProfile.MirrorAboutLocalZ || sourceProfile.MirrorAboutLocalY)
                BH.Engine.Base.Compute.RecordWarning($"Profile of the BHoM section property is mirrored against one of its axes - this information has been ignored while creating the Revit family type on the fly.");
        }

        /***************************************************/

        private static Family ILoadProfileFamily(this Document document, BH.oM.Spatial.ShapeProfiles.IProfile profile)
        {
            return LoadProfileFamily(document, profile as dynamic);
        }

        /***************************************************/

        private static Family LoadProfileFamily(this Document document, BH.oM.Spatial.ShapeProfiles.ISectionProfile profile)
        {
            Family family;
            document.LoadFamily(m_FamilyDirectory + "\\StructuralFraming_IProfile.rfa", out family);
            return family;
        }

        /***************************************************/

        private static Family LoadProfileFamily(this Document document, BH.oM.Spatial.ShapeProfiles.AngleProfile profile)
        {
            Family family;
            document.LoadFamily(m_FamilyDirectory + "\\StructuralFraming_Angle.rfa", out family);
            return family;
        }

        /***************************************************/

        private static string ProfileFamilyName(this BH.oM.Physical.Elements.IFramingElement element)
        {
            string name = element?.Property?.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = (element?.Property as BH.oM.Physical.FramingProperties.ConstantFramingProperty)?.Profile?.Name;
            
            if (string.IsNullOrWhiteSpace(name))
                return null;

            string prefix = element is BH.oM.Physical.Elements.Column ? "StructuralColumns_" : "StructuralFraming_";
            Regex pattern = new Regex(@"\d([\d\.\/xX])*\d");
            return prefix + pattern.Replace(name, "").Replace("  ", " ").Trim();
        }


        /***************************************************/
        /****              Fallback methods             ****/
        /***************************************************/

        private static void CopyDimensions(this BH.oM.Spatial.ShapeProfiles.IProfile sourceProfile, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            BH.Engine.Base.Compute.RecordWarning($"Dimensions of a profile with shape {sourceProfile.GetType().Name} could not be copied to the Revit family symbol because such shape is not supported. ElementId: {targetSymbol.Id}");
        }

        /***************************************************/

        private static Family LoadProfileFamily(this Document document, BH.oM.Spatial.ShapeProfiles.IProfile profile)
        {
            return null;
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static readonly string m_FamilyDirectory = @"C:\ProgramData\BHoM\Resources\Revit\Families";
        private static readonly string m_TempFolder = @"C:\temp";

        /***************************************************/
    }
}