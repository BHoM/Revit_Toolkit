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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Extrusion = Autodesk.Revit.DB.Extrusion;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Generates a Revit FamilySymbol for a BHoM PadFoundation by querying existing families in the document or creating a new one from a template file with parametric dimensions.")]
        [Input("padFoundation", "BHoM pad foundation to generate the Revit profile for.")]
        [Input("document", "Revit document, in which the family type will be created.")]
        [Input("settings", "Settings to be used when generating the family type.")]
        [Output("symbol", "Created Revit family type that represents the outline of the input BHoM pad foundation.")]
        public static FamilySymbol GeneratePadFoundationType(this PadFoundation padFoundation, Document document, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            bool isRectangle;
            Polyline outline = padFoundation?.PadFoundationOutline();
            if (outline != null)
                isRectangle = outline.IsRectangular(settings.DistanceTolerance);
            else
                return null;

            if (isRectangle)
                return GenerateRectangularType(padFoundation, document, settings);
            else
                return GenerateFreeformType(padFoundation, document, settings);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilySymbol GenerateRectangularType(PadFoundation padFoundation, Document document, RevitSettings settings)
        {
            // Check if family loaded to the document, if not, load it from resources path
            string familyName = "StructuralFoundations_Pad-Rectangular";

            Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).FirstOrDefault(x => x.Name == familyName) as Family;
            if (family == null)
            {
                string path = Directory.GetFiles(m_FamilyDirectory, $"*{familyName}.rfa").FirstOrDefault();
                if (!File.Exists(path))
                    return null;

                if (!document.LoadFamily(path, out family) || family == null)
                    return null;
            }

            // Get dimensions of the pad foundation
            (double, double, double) dimensions = padFoundation.RectangularDimensions();
            if (double.IsNaN(dimensions.Item1) || double.IsNaN(dimensions.Item2) || double.IsNaN(dimensions.Item3))
                return null;
            return family.FindOrCreateTypeWithDimensions(dimensions.Item1, dimensions.Item2, dimensions.Item3);
        }

        /***************************************************/
        private static FamilySymbol FindOrCreateTypeWithDimensions(this Family family, double width, double length, double thickness)
        {
            List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(id => family.Document.GetElement(id) as FamilySymbol).Where(s => s != null).ToList();

            long widthMm = (long)Math.Round(width * 1000.0);
            long lengthMm = (long)Math.Round(length * 1000.0);
            long depthMm = (long)Math.Round(thickness * 1000.0);
            string typeName = $"{widthMm}x{lengthMm}x{depthMm}";

            FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == typeName);
            if (result == null && symbols.Count != 0)
            {
                result = symbols.FirstOrDefault(x => !(new FilteredElementCollector(family.Document).WherePasses(new FamilyInstanceFilter(family.Document, x.Id)).Any()));
                if (result != null)
                    result.Name = typeName;
                else
                    result = symbols[0].Duplicate(typeName) as FamilySymbol;
                result.SetParameter("Width", width);
                result.SetParameter("Length", length);
                result.SetParameter("Foundation Thickness", thickness);
            }
            result?.Activate();
            return result;
        }

        /***************************************************/
        private static (double, double, double) RectangularDimensions(this PadFoundation padFoundation)
        {
            Polyline outline = padFoundation.PadFoundationOutline();
            double len1 = outline.ControlPoints[0].Distance(outline.ControlPoints[1]);
            double len2 = outline.ControlPoints[1].Distance(outline.ControlPoints[2]);
            double bhomLength = Math.Max(len1, len2);
            double bhomWidth = Math.Min(len1, len2);

            return (bhomWidth, bhomLength, padFoundation.PadFoundationThickness());
        }

        /***************************************************/

        private static FamilySymbol GenerateFreeformType(PadFoundation padFoundation, Document document, RevitSettings settings)
        {
            string prefix = "StructuralFoundations_FreeForm_";

            // Get the outline and check if valid
            Polyline outline = padFoundation.PadFoundationOutline();
            if (outline?.IIsClosed() != true)
            {
                BH.Engine.Base.Compute.RecordError($"Pad foundation outline is invalid. BHoM_Guid: {padFoundation.BHoM_Guid}");
                return null;
            }

            // Get the thickness and check if valid
            double thickness = padFoundation.PadFoundationThickness();
            if (double.IsNaN(thickness))
                return null;

            // Orient the outline to origin
            Polyline orientedOutline = outline.OrientToOrigin();
            if (orientedOutline == null)
                return null;

            // Get all BHoM-generated freeform families in the document
            List<Family> freeformFamilies = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>()
                .Where(x => Regex.IsMatch(x.Name, $"^{prefix}\\d+$")).ToList();

            // Try to find a family with matching outline, otherwise create a new one from template
            Family family = freeformFamilies.FirstOrDefault(x => x.IsMatchingOutline(orientedOutline, settings));
            if (family == null)
            {
                List<int> takenIndices = freeformFamilies.Select(x => int.Parse(x.Name.Substring(prefix.Length))).ToList();
                int newIndex = takenIndices.Count > 0 ? takenIndices.Max() + 1 : 1;
                family = GenerateFreeFormPadFamilyFromTemplate(document, orientedOutline, thickness, $"{prefix}{newIndex}", padFoundation, settings);
            }

            if (family == null)
                return null;

            // Find or create the type with matching thickness
            return family.FindOrCreateTypeWithThickness(thickness);
        }

        /***************************************************/

        private static Family GenerateFreeFormPadFamilyFromTemplate(this Document document, Polyline orientedOutline, double thickness, string familyName, PadFoundation padFoundation, RevitSettings settings = null)
        {
            string templateFamilyName = "StructuralFoundations_FreeForm";
            string templatePath = Directory.GetFiles(m_FamilyDirectory, $"*{templateFamilyName}.rfa").FirstOrDefault();

            Document familyDocument = new UIDocument(document).Application.Application.OpenDocumentFile(templatePath);
            if (familyDocument == null)
                return null;

            try
            {
                if (!ReplaceFreeFormExtrusion(familyDocument, orientedOutline, padFoundation))
                    return null;

                return SaveAndLoadFamily(document, familyDocument, familyName);
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit pad foundation failed with the following error: {ex.Message}");
                return null;
            }
            finally
            {
                familyDocument.Close(false);
            }
        }

        /***************************************************/
        private static double FreeformExtrusionDepth(PadFoundation padFoundation)
        {
            double depth = padFoundation.PadFoundationThickness();
            double h = double.IsNaN(depth) ? double.NaN : depth.FromSI(SpecTypeId.Length);
            if (double.IsNaN(h) || h <= 1e-6)
                h = 0.5.FromSI(SpecTypeId.Length);
            return h;
        }

        /***************************************************/
        private static bool ReplaceFreeFormExtrusion(Document familyDocument, Polyline orientedOutline, PadFoundation padFoundation)
        {
            try
            {
                Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).FirstOrDefault() as Extrusion;
                CurveArrArray profile = new CurveArrArray();
                profile.Append(orientedOutline.ToRevitCurveArray());

                using (Transaction t = new Transaction(familyDocument, "Update Freeform Pad Foundation Footprint"))
                {
                    t.Start();
                    familyDocument.FamilyCreate.NewExtrusion(true, profile, extrusion.Sketch.SketchPlane, -FreeformExtrusionDepth(padFoundation));
                    familyDocument.Delete(extrusion.Id);
                    t.Commit();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /***************************************************/

        private static FamilySymbol FindOrCreateTypeWithThickness(this Family family, double thickness)
        {
            List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(id => family.Document.GetElement(id) as FamilySymbol).Where(s => s != null).ToList();
            if (symbols.Count == 0)
                return null;

            string typeName = $"{(long)Math.Round(thickness * 1000.0)}mm";
            FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == typeName);
            if (result == null && symbols.Count != 0)
            {
                result = symbols.FirstOrDefault(x => !(new FilteredElementCollector(family.Document).WherePasses(new FamilyInstanceFilter(family.Document, x.Id)).Any()));
                if (result != null)
                    result.Name = typeName;
                else
                    result = symbols[0].Duplicate(typeName) as FamilySymbol;

                result.SetParameter("Foundation Thickness", thickness);
            }

            result?.Activate();
            return result;

            /***************************************************/
        }
    }
}
