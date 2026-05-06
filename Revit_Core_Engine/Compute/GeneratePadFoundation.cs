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
            if (padFoundation == null)
            {
                BH.Engine.Base.Compute.RecordError($"The BHoM pad foundation is null. BHoM_Guid: {padFoundation?.BHoM_Guid}");
                return null;
            }

            string familyName = padFoundation.PadFoundationFamilyName();
            if (string.IsNullOrWhiteSpace(familyName))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a Revit pad foundation family failed because the BHoM pad foundation does not have a name. BHoM_Guid: {padFoundation.BHoM_Guid}");
                return null;
            }

            string typeName = padFoundation.PadFoundationTypeName();
            if (string.IsNullOrWhiteSpace(typeName))
            {
                BH.Engine.Base.Compute.RecordError($"Creation of a Revit pad foundation type failed because the BHoM pad foundation does not have a name. BHoM_Guid: {padFoundation.BHoM_Guid}");
                return null;
            }

            settings = settings.DefaultIfNull();
            //TODO: remember to distinct between rectangular, triangle and freeform pad foundation
            // e.g. for rectangle just set the dimension params
            // for triangle most likely the same, just some sort of radius param (look up the family)
            // freeform needs to set the actual outline - if not implemented yet, ignore for now (assume only rectangles and tris)
            Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).FirstOrDefault(x => x.Name == familyName) as Family;
            if (family != null)
            {
                List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(x => document.GetElement(x) as FamilySymbol).Where(x => x != null).ToList();
                FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == typeName);
                if (result == null && symbols.Count != 0)
                {
                    result = symbols[0].Duplicate(padFoundation.Name) as FamilySymbol;
                    result.Activate();
                    padFoundation.CopyFoundationOutline(result, settings);
                }

                return result;
            }
            //TODO: remember to distinct between rectangular, triangle and freeform pad foundation
            else
            {
                FamilySymbol templateSymbol = document.LoadPadFoundationTemplate(settings);

                if (templateSymbol != null)
                {
                    Family familyFromTemplate = templateSymbol.Family;

                    FamilySymbol result = templateSymbol.Duplicate(typeName) as FamilySymbol;
                    result.Activate();

                    padFoundation.CopyFoundationOutline(result, settings);
                    return result;
                }

                family = document.GenerateFamilyFromTemplate(padFoundation, familyName, settings);

                if (family == null)
                    return null;

                FamilySymbol fallbackResult = document.GetElement(
                    family.GetFamilySymbolIds().FirstOrDefault()
                ) as FamilySymbol;

                if (fallbackResult == null)
                    return null;

                fallbackResult.Activate();
                fallbackResult.Name = typeName;

                return fallbackResult;
            }
        }
        /***************************************************/
        //description check
        [Description("Loads or activates the rectangular PadFoundation family template in the document.")]
        [Input("document", "Revit document where the family symbol should be loaded/activated.")]
        [Input("settings", "Revit adapter settings used for loading the family symbol.")]
        [Output("symbol", "FamilySymbol for the rectangular PadFoundation template, or null if not found.")]
        public static FamilySymbol LoadPadFoundationTemplate(this Document document, RevitSettings settings)
        {
            settings = settings.DefaultIfNull();


            var families = new List<(string familyName, string typeName)>
            {
                ("BHE_StructuralFoundations_Pad-Rectangular", "1000x1000x500 DP"),
                ("BHE_StructuralFoundations_Pad-Triangular", "A"),
                ("BHE_StructuralFoundations_Pad-FreeForm", "1000x1000x200 DP"),
            };


            foreach (var (familyName, typeName) in families)
            {
                Family existingFamily = new FilteredElementCollector(document)
                    .OfClass(typeof(Family))
                    .FirstOrDefault(x => x.Name == familyName) as Family;

                if (existingFamily != null)
                {
                    FamilySymbol symbol = existingFamily.GetFamilySymbolIds()
                        .Select(id => document.GetElement(id) as FamilySymbol)
                        .FirstOrDefault(x => x != null && x.Name == typeName);

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

                if (File.Exists(path))
                {
                    Family family;
                    if (document.LoadFamily(path, out family) && family != null)
                    {
                        FamilySymbol symbol = family.GetFamilySymbolIds()
                            .Select(id => document.GetElement(id) as FamilySymbol)
                            .FirstOrDefault(x => x != null && x.Name == typeName);

                        if (symbol != null)
                        {
                            if (!symbol.IsActive)
                                symbol.Activate();

                            return symbol;
                        }
                    }
                }
            }

            BH.Engine.Base.Compute.RecordError($"Could not load any pad foundation template family from provided list.");

            return null;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Family GenerateFamilyFromTemplate(this Document document, PadFoundation padFoundation, string familyName, RevitSettings settings = null)
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
                    padFoundation.CopyFoundationOutline(symbol, settings);
            }

            return result;
        }

        /***************************************************/

        private static string PadFoundationFamilyName(this PadFoundation padFoundation)
        {
            if (padFoundation?.Name != null && padFoundation.Name.Contains(':'))
                return padFoundation.Name.Split(':')[0].Trim();

            PadFoundationOutlineShape shape = PadFoundationOutlineShape.Rectangle;
            Polyline outline = padFoundation.Boundary();
            if (outline != null)
                outline.TryClassifyPadOutline(out shape);

            //TODO: here need distinct between rectangular, triangle and freeform pad foundation, maybe by looking at the outline or by adding a property to the BHoM object

            switch (shape)
            {
                case PadFoundationOutlineShape.Rectangle:
                    return "BHE_StructuralFoundations_PadFoundation-Rectangular";
                default:
                    return "BHE_StructuralFoundations_Pad-Rectangular"; // for test this is free form
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

        //TODO: rename to copy foundation outline or ismilar
        private static void CopyFoundationOutline(this PadFoundation padFoundation, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            Polyline outline = padFoundation.Boundary();
            if (outline == null || !outline.TryPadOutlinePlacementInXY(out _, out _, out double length, out double width))
                return;

            double depth = padFoundation.Thickness();

            // if rectangular/triangle, just set params
            // else set the actual outline
            targetSymbol.LookupParameter("BHE_Width")?.Set(width);
            targetSymbol.LookupParameter("BHE_Length")?.Set(length);
            targetSymbol.LookupParameter("BHE_Depth")?.Set(depth);
        }

        /***************************************************/

    }
}


