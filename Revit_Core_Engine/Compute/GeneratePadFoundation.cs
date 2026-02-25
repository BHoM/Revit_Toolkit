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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.Physical.Elements;
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

        [Description("Generates a Revit family type (and its parent family, if not loaded yet) that represents the profile of a given BHoM pad foundation." +
                     "\nThe profile is created based on the template families stored in C:\\ProgramData\\Resources\\Revit.")]
        [Input("padFoundation", "BHoM pad foundation to generate the Revit profile for.")]
        [Input("document", "Revit document, in which the family type will be created.")]
        [Input("settings", "Settings to be used when generating the family type.")]
        [Output("symbol", "Created Revit family type that represents the profile of the input BHoM pad foundation.")]
        public static FamilySymbol GeneratePadFoundation(this PadFoundation padFoundation, Document document, RevitSettings settings = null)
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

            settings = settings.DefaultIfNull();

            Family family = new FilteredElementCollector(document).OfClass(typeof(Family)).FirstOrDefault(x => x.Name == familyName) as Family;
            if (family != null)
            {
                List<FamilySymbol> symbols = family.GetFamilySymbolIds().Select(x => document.GetElement(x) as FamilySymbol).Where(x => x != null).ToList();
                FamilySymbol result = symbols.FirstOrDefault(x => x?.Name == padFoundation.Name);
                if (result == null && symbols.Count != 0)
                {
                    result = symbols[0].Duplicate(padFoundation.Name) as FamilySymbol;
                    result.Activate();
                    padFoundation.ICopyFoundationDimensions(result, settings);
                }

                return result;
            }
            else
            {
                family = document.GenerateFamilyFromTemplate(padFoundation, familyName, settings);
                if (family == null)
                {
                    family = document.GenerateFreeformPadFoundation(padFoundation, familyName, settings);
                    if (family == null)
                        return null;
                }

                FamilySymbol result = document.GetElement(family.GetFamilySymbolIds().FirstOrDefault()) as FamilySymbol;
                if (result == null)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Generation of a Revit family representing the BHoM pad foundation failed due to an internal error. BHoM_Guid: {padFoundation.BHoM_Guid}");
                    return null;
                }

                result.Activate();
                result.Name = padFoundation.PadFoundationTypeName() + " " + padFoundation.PadFoundationFamilyName();
                return result;
            }
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Family SaveAndLoadFamily(Document document, Document familyDocument, string familyName, PadFoundation padFoundation, RevitSettings settings) //clean duplicate IBhom ob ject etc.
        {
            Family result = null;
            string tempFolder = Path.GetTempPath();
            string tempLocation = SanitizePathPadFoundation($"{tempFolder}\\{familyName}.rfa");

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

            return result;
        }

        /***************************************************/

        private static string SanitizePathPadFoundation(string inputPath) //clean duplicate
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
        private static Family GenerateFamilyFromTemplate(this Document document, PadFoundation padFoundation, string familyName, RevitSettings settings = null)
        {
            string templateFamilyName = padFoundation.PadFoundationFamilyName();
            if (string.IsNullOrWhiteSpace(templateFamilyName))
                return null;

            string path = Path.Combine(m_FamilyDirectory, $"{templateFamilyName}.rfa");

            Family result = null;
            UIDocument uidoc = new UIDocument(document);
            Document familyDocument = uidoc.Application.Application.OpenDocumentFile(path);

            try
            {
                result = SaveAndLoadFamily(document, familyDocument, familyName, padFoundation, settings);
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
                    padFoundation.ICopyFoundationDimensions(symbol, settings);
            }

            return result;
        }

        /***************************************************/

        private static Family GenerateFreeformPadFoundation(this Document document, PadFoundation padFoundation, string familyName, RevitSettings settings = null)
        {
            throw new NotImplementedException("GenerateFreeformPadFoundation is not implemented.");
        }

        /**************************************************/


        private static string PadFoundationFamilyName(this PadFoundation padFoundation)
        {
            string name = padFoundation?.Name;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            if (name.Contains(':'))
                return name.Split(':')[0].Trim();
            else
            {
                // Add pad foundation name with StructuralFoundations prefix
                Regex pattern = new Regex(@"\d([\d\.\/\-xX ])*\d");
                return $"StructuralFoundations_{pattern.Replace(name, "").Replace("  ", " ").Trim()}";
            }
        }

        /***************************************************/

        private static string PadFoundationTypeName(this PadFoundation padFoundation) //clean up duplicate
        {
            string name = padFoundation?.Name;
            if (string.IsNullOrWhiteSpace(name))
                return null;

            if (name.Contains(':'))
                return name.Split(':')[1].Trim();
            else
                return name;
        }

        /***************************************************/

        private static void ICopyFoundationDimensions(this PadFoundation padFoundation, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            CopyFoundationDimensions(padFoundation as dynamic, targetSymbol, settings);
        }

        /***************************************************/

        private static void CopyFoundationDimensions(this PadFoundation padFoundation, FamilySymbol targetSymbol, RevitSettings settings = null)
        {
            List<BH.oM.Geometry.Line> boundary = padFoundation.ExtractBoundary();
            if (boundary == null)
            {
                BH.Engine.Base.Compute.RecordError($"PadFoundation boundary extraction failed. BHoM_Guid: {padFoundation.BHoM_Guid}");
                return;
            }

            var (width, length) = boundary.GetRectangleDimensions();
            double depth = padFoundation.GetThicknessFromConstr();

            Parameter widthParam = targetSymbol.LookupParameter("Width");
            if (widthParam != null)
                widthParam.Set(width);

            Parameter lengthParam = targetSymbol.LookupParameter("Length");
            if (lengthParam != null)
                lengthParam.Set(length);

            Parameter depthParam = targetSymbol.LookupParameter("Depth");
            if (depthParam != null)
                depthParam.Set(depth);
        }

        /***************************************************/
    }
}



