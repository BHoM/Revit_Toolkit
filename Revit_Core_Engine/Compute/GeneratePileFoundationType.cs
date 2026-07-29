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
using Autodesk.Revit.UI;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Physical.FramingProperties;
using BH.oM.Spatial.Layouts;
using BH.oM.Spatial.ShapeProfiles;
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

        [Description("Generates a Revit FamilySymbol for a BHoM PileFoundation by querying existing families in the document or creating a new one from a freeform template.")]
        [Input("pileFoundation", "BHoM pile foundation to generate the Revit type for.")]
        [Input("document", "Revit document, in which the family type will be created.")]
        [Input("settings", "Settings to be used when generating the family type.")]
        [Output("symbol", "Created Revit family type that represents the input BHoM pile foundation.")]
        public static FamilySymbol GeneratePileFoundationType(this PileFoundation pileFoundation, Document document, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            if (pileFoundation?.PileCap == null || pileFoundation.Piles == null)
                return null;

            return GenerateFreeformType(pileFoundation, document, settings);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilySymbol GenerateFreeformType(PileFoundation pileFoundation, Document document, RevitSettings settings)
        {
            string prefix = "StructuralFoundations_PileFoundation-Freeform_";

            Polyline outline = pileFoundation.PileCap.PadFoundationOutline();
            if (outline?.IIsClosed() != true)
            {
                BH.Engine.Base.Compute.RecordError($"Pile cap outline is invalid. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return null;
            }

            double thickness = pileFoundation.PileCap.PadFoundationThickness();
            double pileDepth = pileFoundation.Piles.Select(p => p.Location as BH.oM.Geometry.Line).Where(l => l != null).Select(l => Math.Abs(l.Start.Z - l.End.Z)).DefaultIfEmpty(double.NaN).Max();

            CircleProfile profile = (pileFoundation.Piles[0].Property as ConstantFramingProperty)?.Profile as CircleProfile;
            double diameter = profile?.Diameter ?? 0;

            Polyline orientedOutline = outline.OrientToOrigin();
            ExplicitLayout layout = pileFoundation.PileFoundationLayout(orientedOutline, settings);
            List<Family> freeformFamilies = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>()
                .Where(x => Regex.IsMatch(x.Name, $"{prefix}\\d+$")).ToList();

            Family family = freeformFamilies.FirstOrDefault(x => x.IsMatchingOutlineAndLayout(orientedOutline, layout, settings));
            if (family == null)
            {
                List<int> takenIndices = freeformFamilies.Select(x => Regex.Match(x.Name, $"{Regex.Escape(prefix)}(\\d+)$")).Select(x => int.Parse(x.Groups[1].Value)).ToList();
                int newIndex = takenIndices.Count > 0 ? takenIndices.Max() + 1 : 1;
                family = GenerateFreeFormPileFamilyFromTemplate(document, orientedOutline, pileFoundation, layout, thickness, pileDepth, diameter, newIndex, settings);
            }

            if (family == null)
                return null;

            return family.FindOrCreatePileFoundationType(thickness, diameter, pileDepth);
        }

        /***************************************************/

        private static Family GenerateFreeFormPileFamilyFromTemplate(Document document, Polyline orientedOutline, PileFoundation pileFoundation, ExplicitLayout layout, double thickness, double pileDepth, double diameter, int index, RevitSettings settings = null)
        {
            string templateFamilyName = "StructuralFoundations_PileFoundation-Freeform";
            string templatePath = Directory.GetFiles(m_FamilyDirectory, $"*{templateFamilyName}.rfa").FirstOrDefault();
            if (string.IsNullOrEmpty(templatePath))
            {
                BH.Engine.Base.Compute.RecordError($"Pile foundation template '{templateFamilyName}.rfa' not found in {m_FamilyDirectory}.");
                return null;
            }

            Document familyDocument = new UIDocument(document).Application.Application.OpenDocumentFile(templatePath);

            try
            {
                List<FamilyInstance> templatePiles = new FilteredElementCollector(familyDocument).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().ToList();
                FamilySymbol pileSymbol = templatePiles.Select(x => x.Symbol).FirstOrDefault()
                    ?? new FilteredElementCollector(familyDocument).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                    .FirstOrDefault(x => x?.Family != null && x.Family.Name.Contains("Pile"));

                if (!ReplaceFreeFormExtrusion(familyDocument, orientedOutline, thickness))
                {
                    BH.Engine.Base.Compute.RecordError($"Replacing freeform pile cap extrusion failed. Depth={thickness}, BHoM_Guid: {pileFoundation.BHoM_Guid}");
                    return null;
                }

                using (Transaction t = new Transaction(familyDocument, "Nested Piles"))
                {
                    t.Start();
                    if (templatePiles.Count > 0)
                        familyDocument.Delete(templatePiles.Select(x => x.Id).ToList());

                    FamilySymbol nestSymbol = PrepareNestedPileSymbol(pileSymbol, diameter, pileDepth);
                    if (nestSymbol == null)
                    {
                        t.RollBack();
                        BH.Engine.Base.Compute.RecordError($"Could not prepare nested pile type. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                        return null;
                    }

                    double embedment = nestSymbol.LookupParameterDouble("Pile Embedment");
                    if (double.IsNaN(embedment) || embedment < 0)
                        embedment = 0;
                    if (embedment > thickness)
                        embedment = thickness;

                    double nestZ = -(thickness - embedment).FromSI(SpecTypeId.Length);

                    int count = Math.Min(pileFoundation.Piles.Count, layout.Points.Count);
                    for (int i = 0; i < count; i++)
                    {
                        oM.Geometry.Point pt = layout.Points[i];
                        familyDocument.FamilyCreate.NewFamilyInstance(
                            new XYZ(pt.X.FromSI(SpecTypeId.Length), pt.Y.FromSI(SpecTypeId.Length), nestZ),
                            nestSymbol,
                            StructuralType.NonStructural);
                    }
                    t.Commit();
                }

                return SaveAndLoadFamily(document, familyDocument, $"{Path.GetFileNameWithoutExtension(templatePath)}_{index}");
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit pile foundation failed with the following error: {ex.Message}");
                return null;
            }
            finally
            {
                familyDocument.Close(false);
            }
        }

        /***************************************************/

        private static FamilySymbol PrepareNestedPileSymbol(FamilySymbol pileSymbol, double diameter, double pileDepth)
        {
            if (pileSymbol == null)
                return null;

            string typeName = $"{(long)Math.Round(diameter * 1000.0)}dia_{(long)Math.Round(pileDepth * 1000.0)}p";
            Family pileFamily = pileSymbol.Family;
            FamilySymbol nestSymbol = pileFamily.GetFamilySymbolIds()
                .Select(id => pileFamily.Document.GetElement(id) as FamilySymbol)
                .FirstOrDefault(s => s != null && s.Name == typeName);

            if (nestSymbol == null)
                nestSymbol = pileSymbol.Duplicate(typeName) as FamilySymbol;


            if (!nestSymbol.IsActive)
                nestSymbol.Activate();

            nestSymbol.SetParameter("Pile Depth", pileDepth);
            if (diameter > 0)
            {
                nestSymbol.SetParameter("Diameter", diameter);
                nestSymbol.SetParameter("Radius", diameter / 2);
            }

            double embedment = nestSymbol.LookupParameterDouble("Pile Embedment");
            if (!double.IsNaN(embedment))
                nestSymbol.SetParameter("Pile Embedment", embedment);

            return nestSymbol;
        }

        /***************************************************/

        private static bool ReplaceFreeFormExtrusion(Document familyDocument, Polyline orientedOutline, double thickness)
        {
            try
            {
                Extrusion extrusion = new FilteredElementCollector(familyDocument)
                    .OfClass(typeof(Extrusion)).FirstOrDefault() as Extrusion;
                CurveArrArray profile = new CurveArrArray();
                profile.Append(orientedOutline.ToRevitCurveArray());
                using (Transaction t = new Transaction(familyDocument, "Update Freeform PileCap"))
                {
                    t.Start();
                    familyDocument.FamilyCreate.NewExtrusion(true, profile, extrusion.Sketch.SketchPlane, -thickness.FromSI(SpecTypeId.Length));
                    familyDocument.Delete(extrusion.Id);
                    t.Commit();
                }
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Replacing freeform pile cap extrusion failed with the following error: {ex.Message}");
                return false;
            }
            return true;
        }

        /***************************************************/

        private static FamilySymbol FindOrCreatePileFoundationType(this Family family, double thickness, double diameter, double pileDepth)
        {
            List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(id => family.Document.GetElement(id) as FamilySymbol).Where(s => s != null).ToList();
            if (symbols.Count == 0)
                return null;

            string typeName = $"{(long)Math.Round(diameter * 1000.0)}dia_{(long)Math.Round(thickness * 1000.0)}mm_{(long)Math.Round(pileDepth * 1000.0)}p";
            FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == typeName);
            if (result == null && symbols.Count != 0)
            {
                result = symbols.FirstOrDefault(x => !(new FilteredElementCollector(family.Document).WherePasses(new FamilyInstanceFilter(family.Document, x.Id)).Any()));
                if (result != null)
                    result.Name = typeName;
                else
                    result = symbols[0].Duplicate(typeName) as FamilySymbol;
            }

            if (result == null)
                return null;

            double embedment = result.LookupParameterDouble("Pile Embedment");

            result.SetParameter("Foundation Thickness", thickness);
            result.SetParameter("Depth", thickness);
            result.SetParameter("Pile Depth", pileDepth);
            if (!double.IsNaN(embedment))
                result.SetParameter("Pile Embedment", embedment);
            if (diameter > 0)
                result.SetParameter("Radius", diameter / 2);

            result.Activate();
            return result;
        }

        /***************************************************/
    }
}
