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

        [Description("Generates a Revit FamilySymbol for a BHoM PileFoundation by querying existing freeform families in the document or creating a new one from a template file. Falls back to a pad foundation type when the input has no piles.")]
        [Input("pileFoundation", "BHoM pile foundation to generate the Revit type for.")]
        [Input("document", "Revit document, in which the family type will be created.")]
        [Input("settings", "Settings to be used when generating the family type.")]
        [Output("symbol", "Created Revit family type that represents the input BHoM pile foundation.")]
        public static FamilySymbol GeneratePileFoundationType(this PileFoundation pileFoundation, Document document, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            if (pileFoundation?.PileCap == null)
                return null;

            if (pileFoundation.Piles == null || pileFoundation.Piles.Count == 0)
            {
                BH.Engine.Base.Compute.RecordWarning($"PileFoundation has no piles; generating a pad foundation type instead. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return pileFoundation.PileCap.GeneratePadFoundationType(document, settings);
            }

            return GenerateFreeformType(pileFoundation, document, settings);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilySymbol GenerateFreeformType(PileFoundation pileFoundation, Document document, RevitSettings settings)
        {
            string prefix = "StructuralFoundations_PileFoundation-Freeform_";

            // Get the outline and check if valid
            Polyline outline = pileFoundation.PileCap.PadFoundationOutline();
            if (outline?.IIsClosed() != true)
            {
                BH.Engine.Base.Compute.RecordError($"Pile foundation outline is invalid. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return null;
            }

            // Get the thickness and check if valid
            double thickness = pileFoundation.PileCap.PadFoundationThickness();
            if (double.IsNaN(thickness))
                return null;

            double pileDepth = pileFoundation.PileFoundationDepth(settings);
            if (double.IsNaN(pileDepth))
            {
                BH.Engine.Base.Compute.RecordError($"Could not determine pile depth from cap soffit to pile tip. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return null;
            }

            // Get circular pile profiles and check if same diameter
            List<CircleProfile> profiles = pileFoundation.Piles.Select(x => (x.Property as ConstantFramingProperty)?.Profile).OfType<CircleProfile>().ToList();

            if (profiles.Count != pileFoundation.Piles.Count)
            {
                BH.Engine.Base.Compute.RecordError($"All piles must have a circular profile. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return null;
            }

            double tol = settings.DistanceTolerance;
            double diameter = profiles[0].Diameter;
            if (profiles.Any(p => Math.Abs(p.Diameter - diameter) > tol))
            {
                BH.Engine.Base.Compute.RecordError($"All piles must have the same diameter. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return null;
            }

            // Orient the outline to origin
            Polyline orientedOutline = outline.OrientToOrigin();
            if (orientedOutline == null)
                return null;

            // Get the pile layout and check if valid
            ExplicitLayout layout = pileFoundation.PileFoundationLayout(settings);
            if (layout?.Points == null || layout.Points.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError($"Pile layout is invalid. BHoM_Guid: {pileFoundation.BHoM_Guid}");
                return null;
            }

            // Get all BHoM-generated freeform families in the document
            List<Family> freeformFamilies = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>()
                .Where(x => Regex.IsMatch(x.Name, $"{prefix}\\d+$")).ToList();

            Family family = freeformFamilies.FirstOrDefault(x => x.IsMatchingOutlineAndLayout(orientedOutline, layout, diameter, settings));
            if (family == null)
            {
                List<int> takenIndices = freeformFamilies.Select(x => Regex.Match(x.Name, $"{Regex.Escape(prefix)}(\\d+)$")).Select(x => int.Parse(x.Groups[1].Value)).ToList();
                int newIndex = takenIndices.Count > 0 ? takenIndices.Max() + 1 : 1;
                family = GenerateFreeFormPileFoundationFamilyFromTemplate(document, orientedOutline, layout, diameter, thickness, newIndex);
            }

            if (family == null)
                return null;

            return family.FindOrCreateTypeWithDimensions(thickness, diameter);
        }

        /***************************************************/

        private static Family GenerateFreeFormPileFoundationFamilyFromTemplate(this Document document, Polyline orientedOutline, ExplicitLayout layout, double diameter, double thickness, int index)
        {
            string templateFamilyName = "StructuralFoundations_PileFoundation-Freeform";
            string templatePath = Directory.GetFiles(m_FamilyDirectory, $"*{templateFamilyName}.rfa").FirstOrDefault();
            if (string.IsNullOrEmpty(templatePath))
            {
                BH.Engine.Base.Compute.RecordError($"Pile foundation template '{templateFamilyName}.rfa' not found in {m_FamilyDirectory}.");
                return null;
            }

            Document familyDocument = new UIDocument(document).Application.Application.OpenDocumentFile(templatePath);
            if (familyDocument == null)
                return null;

            try
            {
                if (!ReplaceFreeFormExtrusionWithAssociate(familyDocument, orientedOutline, thickness))
                    return null;

                if (!PlaceNestedPiles(familyDocument, layout, diameter))
                    return null;

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

        private static double FreeformExtrusionDepth(double thickness)
        {
            double h = double.IsNaN(thickness) ? double.NaN : thickness.FromSI(SpecTypeId.Length);
            if (double.IsNaN(h) || h <= 1e-6)
                h = 0.5.FromSI(SpecTypeId.Length);
            return h;
        }

        /***************************************************/

        private static bool ReplaceFreeFormExtrusionWithAssociate(Document familyDocument, Polyline orientedOutline, double thickness)
        {
            try
            {
                if (!ReplaceFreeFormExtrusion(familyDocument, orientedOutline, thickness))
                    return false;

                using (Transaction t = new Transaction(familyDocument, "Update Freeform Pile Foundation Footprint"))
                {
                    Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).Cast<Extrusion>().FirstOrDefault();
                    if (extrusion == null)
                        return false;

                    t.Start();

                    FamilyManager familyManager = familyDocument.FamilyManager;
                    Parameter oldStartParam = extrusion.get_Parameter(BuiltInParameter.EXTRUSION_START_PARAM);
                    FamilyParameter associatedStartParameter = oldStartParam != null
                        ? familyManager.GetAssociatedFamilyParameter(oldStartParam)
                        : null;

                    Parameter endParam = extrusion.get_Parameter(BuiltInParameter.EXTRUSION_END_PARAM);
                    if (endParam != null && !endParam.IsReadOnly)
                        endParam.Set(0.0);

                    if (associatedStartParameter != null)
                    {
                        Parameter newStartParam = extrusion.get_Parameter(BuiltInParameter.EXTRUSION_START_PARAM);
                        familyManager.AssociateElementParameterToFamilyParameter(newStartParam, associatedStartParameter);
                    }

                    t.Commit();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /***************************************************/

        private static bool PlaceNestedPiles(Document familyDocument, ExplicitLayout layout, double diameter)
        {
            List<FamilyInstance> templatePiles = new FilteredElementCollector(familyDocument).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().ToList();
            if (templatePiles.Count == 0)
                return false;

            FamilyInstance templatePile = templatePiles[0];
            XYZ pileOrigin = (templatePile.Location as LocationPoint)?.Point;
            if (pileOrigin == null)
                return false;

            Extrusion templateVoidExtrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).Cast<Extrusion>().Where(e => !e.IsSolid).OrderBy(e =>
                {
                    BoundingBoxXYZ bbox = e.get_BoundingBox(null);
                    if (bbox == null)
                        return double.MaxValue;
                    XYZ c = (bbox.Min + bbox.Max) / 2.0;
                    return c.DistanceTo(new XYZ(pileOrigin.X, pileOrigin.Y, c.Z));
                })
                .FirstOrDefault();

            try
            {
                using (Transaction t = new Transaction(familyDocument, "Place Nested Piles"))
                {
                    t.Start();

                    FamilySymbol nestType = NestedPileSymbol(templatePile.Symbol, diameter);
                    if (nestType == null)
                    {
                        t.RollBack();
                        return false;
                    }

                    if (templatePiles.Count > 1)
                        familyDocument.Delete(templatePiles.Skip(1).Select(x => x.Id).ToList());

                    List<FamilyInstance> placedPiles = new List<FamilyInstance>();
                    List<Extrusion> placedVoids = new List<Extrusion>();

                    foreach (oM.Geometry.Point pt in layout.Points.Where(p => p != null))
                    {
                        double x = pt.X.FromSI(SpecTypeId.Length);
                        double y = pt.Y.FromSI(SpecTypeId.Length);
                        XYZ delta = new XYZ(x - pileOrigin.X, y - pileOrigin.Y, 0);

                        foreach (ElementId id in ElementTransformUtils.CopyElement(familyDocument, templatePile.Id, delta))
                        {
                            FamilyInstance copy = familyDocument.GetElement(id) as FamilyInstance;
                            if (copy != null)
                                placedPiles.Add(copy);
                        }

                        if (templateVoidExtrusion != null)
                        {
                            foreach (ElementId id in ElementTransformUtils.CopyElement(familyDocument, templateVoidExtrusion.Id, delta))
                            {
                                Extrusion copy = familyDocument.GetElement(id) as Extrusion;
                                if (copy != null)
                                    placedVoids.Add(copy);
                            }
                        }
                    }

                    familyDocument.Delete(templatePile.Id);
                    if (templateVoidExtrusion != null)
                        familyDocument.Delete(templateVoidExtrusion.Id);

                    foreach (FamilyInstance pile in placedPiles)
                    {
                        if (pile.Symbol.Id != nestType.Id && pile.IsValidType(nestType.Id))
                            pile.ChangeTypeId(nestType.Id);
                    }

                    FamilyManager fm = familyDocument.FamilyManager;
                    FamilyParameter radiusParam = fm.get_Parameter("Radius");
                    if (radiusParam != null)
                        fm.Set(radiusParam, (diameter / 2.0).FromSI(SpecTypeId.Length));

                    Extrusion cap = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).Cast<Extrusion>().FirstOrDefault(e => e.IsSolid);
                    if (cap != null)
                    {
                        foreach (Extrusion voidExtrusion in placedVoids)
                        {
                            try
                            {
                                if (SolidSolidCutUtils.CanElementCutElement(cap, voidExtrusion, out _))
                                    SolidSolidCutUtils.AddCutBetweenSolids(familyDocument, cap, voidExtrusion);
                            }
                            catch { }
                        }
                    }

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

        private static FamilySymbol NestedPileSymbol(FamilySymbol pileSymbol, double diameter)
        {
            if (pileSymbol == null || diameter <= 0)
                return pileSymbol;

            string typeName = $"Ø{(long)Math.Round(diameter * 1000.0)}";
            FamilySymbol nestSymbol = pileSymbol.Family.GetFamilySymbolIds().Select(id => pileSymbol.Document.GetElement(id) as FamilySymbol).FirstOrDefault(s => s != null && s.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

            if (nestSymbol == null)
                nestSymbol = pileSymbol.Duplicate(typeName) as FamilySymbol;

            if (nestSymbol == null)
                return null;

            nestSymbol.SetParameter("Radius", diameter / 2.0);

            if (!nestSymbol.IsActive)
                nestSymbol.Activate();

            return nestSymbol;
        }

        /***************************************************/

        private static FamilySymbol FindOrCreateTypeWithDimensions(this Family family, double thickness, double diameter)
        {
            List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(id => family.Document.GetElement(id) as FamilySymbol).Where(s => s != null).ToList();
            if (symbols.Count == 0)
                return null;

            string typeName = $"{(long)Math.Round(thickness * 1000.0)}mm thk. Ø{(long)Math.Round(diameter * 1000.0)}";
            FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == typeName);
            if (result == null && symbols.Count != 0)
            {
                result = symbols.FirstOrDefault(x => !(new FilteredElementCollector(family.Document).WherePasses(new FamilyInstanceFilter(family.Document, x.Id)).Any()));
                if (result != null)
                    result.Name = typeName;
                else
                    result = symbols[0].Duplicate(typeName) as FamilySymbol;

                result.SetParameter("Depth", thickness);
            }

            result?.Activate();
            return result;
        }

        /***************************************************/
    }
}
