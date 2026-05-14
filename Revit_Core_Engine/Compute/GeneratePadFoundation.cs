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
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        public static FamilySymbol PadFoundationGenerateType(this PadFoundation padFoundation, Document document, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            PadFoundationOutlineShape shape = PadFoundationOutlineShape.Rectangle;
            Polyline outline = padFoundation.FoundationBoundary();
            if (outline != null)
                outline.FoundationClassifyOutline(out shape);

            if (shape == PadFoundationOutlineShape.Rectangle)
                return PadFoundationTypeRectangularLoad(padFoundation, document, settings);
            else
                return PadFoundationTypeFreeformLoad(padFoundation, document, settings);
        }

        /***************************************************/

        [Description("Loads or activates the PadFoundation family template matching the requested outline shape.")]
        [Input("document", "Revit document where the family symbol should be loaded/activated.")]
        [Input("shape", "Requested outline shape (Rectangle / Freeform).")]
        [Input("settings", "Revit adapter settings used for loading the family symbol.")]
        [Output("symbol", "FamilySymbol for the requested PadFoundation template, or null if not found.")]
        public static FamilySymbol PadFoundationTemplateLoad(this Document document, PadFoundationOutlineShape shape, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();

            string familyName;
            string typeName;
            if (shape == PadFoundationOutlineShape.Rectangle)
            {
                familyName = "BHE_StructuralFoundations_Pad-Rectangular";
                typeName = "1000x1000x500 DP";
            }
            else
            {
                familyName = "BHE_StructuralFoundations_FreeForm";
                typeName = "FreeForm_PreLoaded";
            }

            FamilySymbol symbol = PadFoundationTemplateSymbolLoad(document, settings, familyName, typeName);
            if (symbol != null)
                return symbol;

            BH.Engine.Base.Compute.RecordError($"Could not load pad foundation template family for shape '{shape}' (family '{familyName}', type '{typeName}').");
            return null;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static FamilySymbol PadFoundationTemplateSymbolLoad(Document document, RevitSettings settings, string familyName, string typeName)
        {
            Family existingFamily = new FilteredElementCollector(document)
                .OfClass(typeof(Family))
                .FirstOrDefault(x => x.Name == familyName) as Family;

            if (existingFamily != null)
            {
                FamilySymbol symbol = PadFoundationSelectTemplateSymbol(document, existingFamily, typeName);
                if (symbol != null)
                {
                    if (!symbol.IsActive)
                        symbol.Activate();

                    return symbol;
                }
            }

            FamilySymbol loadedSymbol = settings.FamilyLoadSettings?.LoadFamilySymbol(document, "Structural Foundations", familyName, typeName);
            if (loadedSymbol != null)
                return loadedSymbol;

            string path = Path.Combine(m_FamilyDirectory, $"{familyName}.rfa");
            if (!File.Exists(path))
                return null;

            if (!document.LoadFamily(path, out Family family) || family == null)
                return null;

            FamilySymbol fromFile = PadFoundationSelectTemplateSymbol(document, family, typeName);
            if (fromFile == null)
                return null;

            if (!fromFile.IsActive)
                fromFile.Activate();

            return fromFile;
        }

        /***************************************************/

        private static FamilySymbol PadFoundationSelectTemplateSymbol(Document document, Family family, string typeName)
        {
            return family.GetFamilySymbolIds()
                .Select(id => document.GetElement(id) as FamilySymbol)
                .FirstOrDefault(x => x != null && x.Name == typeName);
        }

        /***************************************************/

        private static Family PadFoundationGenerateFamilyFromTemplate(this Document document, PadFoundation padFoundation, string familyName, RevitSettings settings = null)
        {
            string templateFamilyName = padFoundation.PadFoundationFamilyName();
            string path = Path.Combine(m_FamilyDirectory, $"{templateFamilyName}.rfa");

            Family result = null;
            UIDocument uidoc = new UIDocument(document);
            Document familyDocument = uidoc.Application.Application.OpenDocumentFile(path);

            try
            {
                result = SaveAndLoadFamily(document, familyDocument, familyName);
            }
            catch (Exception ex)
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a freeform Revit profile geometry failed with the following error: {ex.Message}");
            }
            finally
            {
                familyDocument.Close(false);
            }

            if (result != null)
            {
                FamilySymbol symbol = document.GetElement(result?.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                if (symbol != null)
                    padFoundation.PadFoundationCopyOutline(symbol, settings);
            }

            return result;
        }

        /***************************************************/
        private static FamilySymbol PadFoundationTypeRectangularLoad(PadFoundation padFoundation, Document document, RevitSettings settings)
        {
            string familyName = padFoundation.PadFoundationFamilyName();

            string typeName = PadFoundationRectangularSetTypeName(padFoundation);
            if (string.IsNullOrWhiteSpace(typeName))
                typeName = padFoundation.PadFoundationRevitTypeName(familyName);

            FamilySymbol resolved = PadFoundationResolvePadSymbolInFamily(padFoundation, document, settings, familyName, typeName);
            if (resolved != null)
                return resolved;

            FamilySymbol templateSymbol = document.PadFoundationTemplateLoad(PadFoundationOutlineShape.Rectangle, settings);
            if (templateSymbol != null)
            {
                FamilySymbol result = templateSymbol.Duplicate(typeName) as FamilySymbol;
                result.Activate();
                padFoundation.PadFoundationCopyOutline(result, settings);
                return result;
            }

            Family family = document.PadFoundationGenerateFamilyFromTemplate(padFoundation, familyName, settings);
            if (family == null)
                return null;

            FamilySymbol fallbackResult = document.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
            if (fallbackResult == null)
                return null;

            fallbackResult.Activate();
            fallbackResult.Name = typeName;

            return fallbackResult;
        }

        /***************************************************/

        private static FamilySymbol PadFoundationTypeFreeformLoad(PadFoundation padFoundation, Document document, RevitSettings settings)
        {
            const string numberedFreeformFamilyPrefix = "BHE_StructuralFoundations_FreeForm";
            const int numberedFreeformFamilyMaxProbe = 50;

            if (!PadFoudationGetFreeFormClosedOutline(padFoundation, out Polyline bhomOutline, out _))
            {
                BH.Engine.Base.Compute.RecordError($"Freeform pad foundation outline is invalid. BHoM_Guid: {padFoundation.BHoM_Guid}");
                return null;
            }

            double thickness = padFoundation.Thickness();

            string typeNameMm = $"{(long)Math.Round(thickness * 1000.0)}mm";
            double tol = Tolerance.Distance;

            for (int index = 1; index <= numberedFreeformFamilyMaxProbe; index++)
            {
                string familyName = $"{numberedFreeformFamilyPrefix}_{index}";
                Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).Cast<Family>().FirstOrDefault(x => x.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));

                if (family == null)
                    return document.PadFoundationGenerateFreeFormTypeFromTemplate(padFoundation, familyName, typeNameMm, settings);

                if (!PadFoundationTryFreeformFamilyMatch(document, family, bhomOutline, tol))
                    continue;

                FamilySymbol symbolForDepth = PadFoundationFindOrDuplicateFreeFormSymbolForDepth(padFoundation, document, family, thickness, typeNameMm);
                if (symbolForDepth != null)
                    return symbolForDepth;
            }

            return null;
        }

        /***************************************************/

        private static FamilySymbol PadFoundationResolvePadSymbolInFamily(PadFoundation padFoundation, Document document, RevitSettings settings, string familyName, string typeName)
        {
            Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).FirstOrDefault(x => x.Name == familyName) as Family;
            if (family == null)
                return null;

            List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(x => document.GetElement(x) as FamilySymbol).Where(x => x != null).ToList();
            FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == typeName);
            if (result == null && symbols.Count != 0)
            {
                result = symbols[0].Duplicate(typeName) as FamilySymbol;
                result.Activate();
                padFoundation.PadFoundationCopyOutline(result, settings);
            }

            return result;
        }

        /***************************************************/

        private static string PadFoundationRectangularSetTypeName(PadFoundation padFoundation)
        {
            Polyline outline = padFoundation?.FoundationBoundary();
            if (outline == null || !outline.TryPadOutlinePlacementInXY(out _, out _, out double extentAlongLongest, out double extentPerpendicular))
                return null;

            double planMin = Math.Min(extentAlongLongest, extentPerpendicular);
            double planMax = Math.Max(extentAlongLongest, extentPerpendicular);
            double depth = padFoundation.Thickness();
            if (double.IsNaN(depth))
                return null;

            long minMm = (long)Math.Round(planMin * 1000.0);
            long maxMm = (long)Math.Round(planMax * 1000.0);
            long depthMm = (long)Math.Round(depth * 1000.0);
            return $"{minMm}x{maxMm}x{depthMm}";
        }

        /***************************************************/

        private static bool PadFoundationTryFreeformFamilyMatch(Document document, Family family, Polyline bhomOutline, double tol)
        {
            Document famDoc = null;
            try
            {
                famDoc = document.EditFamily(family);
                if (famDoc == null)
                    return false;

                if (!PadFoundationExtrusionSketch(famDoc, out Polyline familyOutline))
                    return false;

                return PadFoundationFreeFormShapeMatch(bhomOutline, familyOutline, tol);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (famDoc != null && famDoc.IsValidObject)
                    famDoc.Close(false);
            }
        }

        /***************************************************/

        private static bool PadFoundationExtrusionSketch(Document familyDocument, out Polyline outline)
        {
            outline = null;
            double tol = Tolerance.Distance;

            Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Extrusion)).FirstElement() as Extrusion;
            if (extrusion?.Sketch?.Profile == null)
                return false;

            List<BH.oM.Geometry.Point> controlPoints = new List<BH.oM.Geometry.Point>();
            foreach (CurveArray curveArray in extrusion.Sketch.Profile)
            {
                foreach (Curve curve in curveArray)
                {
                    IList<XYZ> tess = curve.Tessellate();
                    for (int i = 0; i < tess.Count; i++)
                    {
                        if (controlPoints.Count > 0 && i == 0)
                            continue;

                        controlPoints.Add(tess[i].PointFromRevit());
                    }
                }

                break;
            }

            if ((controlPoints[controlPoints.Count - 1] - controlPoints[0]).Length() <= tol)
                controlPoints.RemoveAt(controlPoints.Count - 1);

            List<BH.oM.Geometry.Line> lines = new List<BH.oM.Geometry.Line>();
            for (int i = 0; i < controlPoints.Count; i++)
            {
                BH.oM.Geometry.Point a = controlPoints[i];
                BH.oM.Geometry.Point b = controlPoints[(i + 1) % controlPoints.Count];
                if ((a - b).Length() > tol)
                    lines.Add(BH.Engine.Geometry.Create.Line(a, b));
            }

            if (lines.Count < 3)
                return false;

            outline = BH.Engine.Geometry.Create.Polyline(lines);
            return true;
        }

        /***************************************************/

        private static bool PadFoundationFreeFormShapeMatch(Polyline a, Polyline b, double tol)
        {
            if (a == null || b == null)
                return false;

            double areaA = Math.Abs(a.Area());
            double areaB = Math.Abs(b.Area());
            double areaTol = Math.Max(areaA, areaB) * 1e-6 + tol * tol;
            if (Math.Abs(areaA - areaB) > areaTol)
                return false;

            double perA = PadFoundationClosedPolylinePerim(a, tol);
            double perB = PadFoundationClosedPolylinePerim(b, tol);
            double perTol = tol * Math.Max(Math.Max(PadFoundationVertexCount(a, tol), PadFoundationVertexCount(b, tol)), 1);
            if (Math.Abs(perA - perB) > perTol)
                return false;

            int nA = PadFoundationVertexCount(a, tol);
            int nB = PadFoundationVertexCount(b, tol);
            if (nA >= 3 && nA == nB)
            {
                List<double> edgesA = PadFoundationClosedEdgeLengths(a, tol);
                List<double> edgesB = PadFoundationClosedEdgeLengths(b, tol);
                if (edgesA.Count == edgesB.Count)
                {
                    edgesA.Sort();
                    edgesB.Sort();
                    bool edgesOk = true;
                    for (int i = 0; i < edgesA.Count; i++)
                    {
                        if (Math.Abs(edgesA[i] - edgesB[i]) > tol)
                        {
                            edgesOk = false;
                            break;
                        }
                    }

                    if (edgesOk)
                        return true;
                }
            }

            return true;
        }

        /***************************************************/

        private static int PadFoundationVertexCount(Polyline polyline, double tol)
        {
            List<BH.oM.Geometry.Point> pts = polyline?.ControlPoints;
            if (pts == null || pts.Count < 3)
                return 0;

            int n = pts.Count;
            if (n >= 3 && (pts[n - 1] - pts[0]).Length() <= tol)
                n--;

            return n;
        }

        /***************************************************/

        private static List<double> PadFoundationClosedEdgeLengths(Polyline polyline, double tol)
        {
            List<double> lengths = new List<double>();
            List<BH.oM.Geometry.Point> pts = polyline?.ControlPoints;
            if (pts == null || pts.Count < 3)
                return lengths;

            int n = pts.Count;
            if (n >= 3 && (pts[n - 1] - pts[0]).Length() <= tol)
                n--;

            for (int i = 0; i < n; i++)
            {
                double len = (pts[(i + 1) % n] - pts[i]).Length();
                if (len > tol)
                    lengths.Add(len);
            }

            return lengths;
        }

        /***************************************************/

        private static double PadFoundationClosedPolylinePerim(Polyline polyline, double tol)
        {
            double sum = 0;
            foreach (double len in PadFoundationClosedEdgeLengths(polyline, tol))
                sum += len;

            return sum;
        }

        /***************************************************/

        private static FamilySymbol PadFoundationFindOrDuplicateFreeFormSymbolForDepth(PadFoundation padFoundation, Document document, Family family, double thicknessSi, string typeNameMm)
        {
            List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(id => document.GetElement(id) as FamilySymbol).Where(s => s != null).ToList();
            if (symbols.Count == 0)
                return null;

            foreach (FamilySymbol candidate in symbols)
            {
                Parameter depthParameter = candidate.LookupParameter("BHE_Depth");
                if (depthParameter == null || !depthParameter.HasValue)
                    continue;

                double depthSi = depthParameter.AsDouble().ToSI(depthParameter.Definition.GetDataType());
                if (Math.Abs(depthSi - thicknessSi) <= Tolerance.Distance)
                    return candidate;
            }

            FamilySymbol template = symbols[0];
            FamilySymbol duplicate = template.Duplicate(typeNameMm) as FamilySymbol;
            if (duplicate == null)
                return null;

            duplicate.Activate();
            duplicate.LookupParameter("BHE_Depth")?.SetParameter(thicknessSi, document);

            return duplicate;
        }

        /***************************************************/

        private static string PadFoundationFamilyName(this PadFoundation padFoundation)
        {
            PadFoundationOutlineShape shape = PadFoundationOutlineShape.Rectangle;
            Polyline outline = padFoundation.FoundationBoundary();
            if (outline != null)
                outline.FoundationClassifyOutline(out shape);

            if (shape == PadFoundationOutlineShape.Rectangle && padFoundation?.Name != null && padFoundation.Name.Contains(':'))
                return padFoundation.Name.Split(':')[0].Trim();

            switch (shape)
            {
                case PadFoundationOutlineShape.Rectangle:
                    return "BHE_StructuralFoundations_Pad-Rectangular";
                default:
                    return "BHE_StructuralFoundations_FreeForm";
            }
        }

        /***************************************************/

        private static string PadFoundationTypeName(this PadFoundation padFoundation)
        {
            string name = padFoundation?.Name;

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (name.Contains(':'))
                    return name.Split(':')[1].Trim();
                else
                    return name;
            }
            else
                return null;
        }

        /***************************************************/

        private static string PadFoundationRevitTypeName(this PadFoundation padFoundation, string familyName)
        {
            string fromBhom = padFoundation.PadFoundationTypeName();
            if (!string.IsNullOrWhiteSpace(fromBhom))
                return fromBhom;

            if (!string.IsNullOrWhiteSpace(familyName))
            {
                int i = familyName.LastIndexOf('_');
                if (i >= 0 && i < familyName.Length - 1)
                {
                    string suffix = familyName.Substring(i + 1).Trim();
                    if (!string.IsNullOrWhiteSpace(suffix))
                        return suffix;
                }
            }

            return null;
        }

        /***************************************************/

        private static bool PadFoundationCopyOutline(this PadFoundation padFoundation, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            Polyline outline = padFoundation.FoundationBoundary();
            if (outline != null && outline.FoundationClassifyOutline(out PadFoundationOutlineShape shape) && shape != PadFoundationOutlineShape.Rectangle)
                return false;

            if (outline == null || !outline.TryPadOutlinePlacementInXY(out _, out _, out double length, out double width))
                return false;

            double depth = padFoundation.Thickness();
            Document doc = targetSymbol.Document;

            targetSymbol.LookupParameter("BHE_Width")?.SetParameter(width, doc);
            targetSymbol.LookupParameter("BHE_Length")?.SetParameter(length, doc);
            if (!double.IsNaN(depth))
                targetSymbol.LookupParameter("BHE_Depth")?.SetParameter(depth, doc);
            else
                BH.Engine.Base.Compute.RecordWarning($"Pad foundation type BHE_Depth was not set: construction thickness is undefined (NaN). BHoM_Guid: {padFoundation.BHoM_Guid}");

            return true;
        }

        /***************************************************/

        private static FamilySymbol PadFoundationGenerateFreeFormTypeFromTemplate(this Document document, PadFoundation padFoundation, string familyName, string typeName, RevitSettings settings = null)
        {
            if (!PadFoudationGetFreeFormClosedOutline(padFoundation, out _, out _))
                return null;

            string templatePath = Path.Combine(m_FamilyDirectory, "BHE_StructuralFoundations_FreeForm.rfa");

            Document familyDocument = new UIDocument(document).Application.Application.OpenDocumentFile(templatePath);
            Family loadedFamily = null;
            try
            {
                if (!PadFoundationReplaceFreeFormExtrusion(familyDocument, padFoundation))
                    return null;

                loadedFamily = SaveAndLoadFamily(document, familyDocument, familyName);
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

            if (loadedFamily == null)
                return null;

            List<ElementId> symbolIds = loadedFamily.GetFamilySymbolIds().ToList();
            FamilySymbol keeper = PadFoundationPickFreeformSymbol(document, loadedFamily, typeName);
            if (keeper == null || symbolIds.Count == 0)
                return null;

            PadFoundationConsolidateFreeFormTypes(document, symbolIds, keeper, typeName, padFoundation);
            return keeper;
        }

        /***************************************************/

        private static bool PadFoudationGetFreeFormClosedOutline(PadFoundation padFoundation, out Polyline outline, out int loopVertexCount)
        {
            outline = padFoundation?.FoundationBoundary();
            loopVertexCount = 0;
            if (outline?.ControlPoints == null || outline.ControlPoints.Count < 3)
                return false;

            int n = outline.ControlPoints.Count;
            bool closed = n >= 3 && (outline.ControlPoints[n - 1] - outline.ControlPoints[0]).Length() <= BH.oM.Geometry.Tolerance.Distance;
            if (!closed)
            {
                BH.Engine.Base.Compute.RecordError($"Freeform pad foundation outline is not properly closed. BHoM_Guid: {padFoundation?.BHoM_Guid}");
                return false;
            }

            loopVertexCount = n - 1;
            return loopVertexCount >= 3;
        }

        /***************************************************/

        private static bool PadFoundationBuildFreeformProfile(Polyline outline, int loopVertexCount, PadFoundation padFoundation, out CurveArrArray profile)
        {
            profile = null;
            List<BH.oM.Geometry.Point> pts = outline.ControlPoints;
            CurveArray loop = new CurveArray();

            for (int i = 0; i < loopVertexCount; i++)
            {
                XYZ a = pts[i].ToRevit();
                XYZ b = pts[(i + 1) % loopVertexCount].ToRevit();
                if (a.DistanceTo(b) > 1e-6)
                    loop.Append(Autodesk.Revit.DB.Line.CreateBound(a, b));
            }

            profile = new CurveArrArray();
            profile.Append(loop);
            return true;
        }

        /***************************************************/

        private static bool PadFoundationReplaceFreeFormExtrusion(Document familyDocument, PadFoundation padFoundation)
        {
            if (!PadFoudationGetFreeFormClosedOutline(padFoundation, out Polyline outline, out int loopVertexCount))
                return false;

            Autodesk.Revit.DB.Extrusion extrusion = new FilteredElementCollector(familyDocument).OfClass(typeof(Autodesk.Revit.DB.Extrusion)).FirstElement() as Autodesk.Revit.DB.Extrusion;

            if (!PadFoundationBuildFreeformProfile(outline, loopVertexCount, padFoundation, out CurveArrArray profile))
                return false;

            using (Transaction t = new Transaction(familyDocument, "Update Freeform Pad Foundation Footprint"))
            {
                t.Start();
                familyDocument.FamilyCreate.NewExtrusion(true, profile, extrusion.Sketch.SketchPlane, PadFoundationFreeFormExtrusionHeight(padFoundation));
                familyDocument.Delete(extrusion.Id);
                t.Commit();
            }

            return true;
        }

        /***************************************************/

        private static double PadFoundationFreeFormExtrusionHeight(PadFoundation padFoundation)
        {
            double depth = padFoundation.Thickness();
            double h = double.IsNaN(depth) ? double.NaN : depth.FromSI(SpecTypeId.Length);
            if (double.IsNaN(h) || h <= 1e-6)
                h = 0.5.FromSI(SpecTypeId.Length);
            return h;
        }

        /***************************************************/
        private static FamilySymbol PadFoundationPickFreeformSymbol(Document document, Family loadedFamily, string typeName)
        {
            List<FamilySymbol> symbols = loadedFamily.GetFamilySymbolIds()
                .Select(id => document.GetElement(id) as FamilySymbol)
                .Where(s => s != null)
                .ToList();
            if (symbols.Count == 0)
                return null;

            return symbols.FirstOrDefault(s => string.Equals(s.Name, typeName, StringComparison.OrdinalIgnoreCase))
                ?? symbols.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase).ThenBy(s => s.Id.IntegerValue).First();
        }

        /***************************************************/
        private static void PadFoundationConsolidateFreeFormTypes(Document document, List<ElementId> symbolIds, FamilySymbol keeper, string typeName, PadFoundation padFoundation)
        {
            void Cleanup()
            {
                foreach (ElementId symId in symbolIds)
                {
                    if (symId == keeper.Id)
                        continue;
                    try
                    {
                        document.Delete(symId);
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Could not remove unused pad foundation family type ElementId {symId.Value()}.");
                    }
                }

                if (keeper.Name != typeName)
                {
                    try
                    {
                        keeper.Name = typeName;
                    }
                    catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                    {
                        BH.Engine.Base.Compute.RecordWarning($"Pad foundation family type could not be renamed to '{typeName}'. ElementId: {keeper.Id.Value()}");
                    }
                }

                if (!keeper.IsActive)
                    keeper.Activate();

                double thickness = padFoundation.Thickness();
                if (!double.IsNaN(thickness))
                    keeper.LookupParameter("BHE_Depth")?.SetParameter(thickness, document);
            }

            if (document.IsModifiable)
            {
                using (SubTransaction str = new SubTransaction(document))
                {
                    str.Start();
                    Cleanup();
                    str.Commit();
                }
            }
            else
            {
                using (Transaction tr = new Transaction(document, "Clean freeform pad family types"))
                {
                    tr.Start();
                    Cleanup();
                    tr.Commit();
                }
            }
        }

        /***************************************************/

    }
}


