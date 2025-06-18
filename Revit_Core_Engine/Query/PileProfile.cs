/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Spatial.ShapeProfiles;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Attempts to extract profile information from a pile family using alternative parameter sources.")]
        [Input("familyInstance", "Revit FamilyInstance representing a pile.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("profile", "BHoM profile extracted from pile family parameters, or null if extraction fails.")]
        public static IProfile ExtractPileProfile(this FamilyInstance familyInstance, RevitSettings settings = null)
        {
            if (familyInstance?.Symbol == null)
                return null;

            settings = settings.DefaultIfNull();

            // Try to extract diameter from common pile parameter names
            double diameter = 0;
            string[] diameterParamNames = { "Diameter", "Pile Diameter", "Width", "D", "Size", "Nominal Diameter" };
            
            foreach (string paramName in diameterParamNames)
            {
                Parameter param = familyInstance.Symbol.LookupParameter(paramName);
                if (param != null && param.HasValue && param.StorageType == StorageType.Double)
                {
                    diameter = param.AsDouble();
                    if (diameter > 0)
                    {
                        // Convert from Revit internal units to meters
                        diameter = diameter.ToSI(SpecTypeId.Length);
                        return BH.Engine.Spatial.Create.CircleProfile(diameter / 2.0); // CircleProfile takes radius
                    }
                }
            }

            // Try to extract width and height for rectangular piles
            double width = 0, height = 0;
            string[] widthParamNames = { "Width", "B", "Pile Width" };
            string[] heightParamNames = { "Height", "H", "Pile Height", "Depth" };

            foreach (string paramName in widthParamNames)
            {
                Parameter param = familyInstance.Symbol.LookupParameter(paramName);
                if (param != null && param.HasValue && param.StorageType == StorageType.Double)
                {
                    width = param.AsDouble().ToSI(SpecTypeId.Length);
                    if (width > 0) break;
                }
            }

            foreach (string paramName in heightParamNames)
            {
                Parameter param = familyInstance.Symbol.LookupParameter(paramName);
                if (param != null && param.HasValue && param.StorageType == StorageType.Double)
                {
                    height = param.AsDouble().ToSI(SpecTypeId.Length);
                    if (height > 0) break;
                }
            }

            if (width > 0 && height > 0)
            {
                return BH.Engine.Spatial.Create.RectangleProfile(height, width, 0); // Rectangle profile with no corner radius
            }

            return null;
        }

        /***************************************************/

        [Description("Creates a default pile profile based on common pile family type naming conventions.")]
        [Input("familyInstance", "Revit FamilyInstance representing a pile.")]
        [Output("profile", "Default BHoM profile for the pile, or null if no suitable default can be determined.")]
        public static IProfile CreateDefaultPileProfile(this FamilyInstance familyInstance)
        {
            if (familyInstance?.Symbol == null)
                return null;

            string familyName = familyInstance.Symbol.FamilyName?.ToLowerInvariant() ?? "";
            string typeName = familyInstance.Symbol.Name?.ToLowerInvariant() ?? "";
            
            // Check for circular pile indicators in family/type names
            if (familyName.Contains("circular") || familyName.Contains("round") || familyName.Contains("pipe") ||
                typeName.Contains("circular") || typeName.Contains("round") || typeName.Contains("pipe"))
            {
                // Try to extract diameter from type name using common patterns
                double diameter = ExtractDimensionFromName(typeName, new[] { "dia", "d", "ø" });
                if (diameter <= 0)
                    diameter = ExtractDimensionFromName(familyName, new[] { "dia", "d", "ø" });
                
                if (diameter > 0)
                {
                    return BH.Engine.Spatial.Create.CircleProfile(diameter / 2.0);
                }
                
                // Default circular pile if no dimension found
                return BH.Engine.Spatial.Create.CircleProfile(0.15); // 300mm diameter
            }

            // Check for square/rectangular pile indicators
            if (familyName.Contains("square") || familyName.Contains("rectangular") || familyName.Contains("concrete") ||
                typeName.Contains("square") || typeName.Contains("rectangular") || typeName.Contains("concrete"))
            {
                // Try to extract dimensions from type name
                double width = ExtractDimensionFromName(typeName, new[] { "x", "×", "*" });
                double height = width; // Assume square if only one dimension found
                
                if (width > 0)
                {
                    return BH.Engine.Spatial.Create.RectangleProfile(height, width, 0);
                }
                
                // Default square pile if no dimension found
                return BH.Engine.Spatial.Create.RectangleProfile(0.3, 0.3, 0); // 300mm x 300mm
            }

            return null;
        }

        /***************************************************/

        [Description("Extracts a dimension value from a text string using common naming patterns.")]
        [Input("text", "Text string to search for dimension values.")]
        [Input("separators", "Array of separator characters that typically precede or follow dimension values.")]
        [Output("dimension", "Extracted dimension in meters, or 0 if no valid dimension found.")]
        private static double ExtractDimensionFromName(string text, string[] separators)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            // Look for patterns like "300", "300mm", "0.3m", "12in", etc.
            var parts = text.Split(new char[] { ' ', '-', '_', '(', ')', '[', ']', '{', '}' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string part in parts)
            {
                string cleanPart = part.Trim();
                
                // Look for numeric values with optional units
                for (int i = 0; i < cleanPart.Length; i++)
                {
                    if (char.IsDigit(cleanPart[i]) || cleanPart[i] == '.')
                    {
                        string numStr = "";
                        int j = i;
                        while (j < cleanPart.Length && (char.IsDigit(cleanPart[j]) || cleanPart[j] == '.'))
                        {
                            numStr += cleanPart[j];
                            j++;
                        }
                        
                        if (double.TryParse(numStr, out double value) && value > 0)
                        {
                            // Check for unit indicators
                            string remainder = cleanPart.Substring(j).ToLowerInvariant();
                            
                            if (remainder.StartsWith("mm"))
                                return value / 1000.0; // Convert mm to m
                            else if (remainder.StartsWith("cm"))
                                return value / 100.0; // Convert cm to m
                            else if (remainder.StartsWith("m") && !remainder.StartsWith("mm"))
                                return value; // Already in meters
                            else if (remainder.StartsWith("in") || remainder.StartsWith("\""))
                                return value * 0.0254; // Convert inches to m
                            else if (remainder.StartsWith("ft") || remainder.StartsWith("'"))
                                return value * 0.3048; // Convert feet to m
                            else if (value < 10) // Assume meters for small values without units
                                return value;
                            else if (value < 1000) // Assume mm for larger values without units
                                return value / 1000.0;
                        }
                        break;
                    }
                }
            }

            return 0;
        }

        /***************************************************/
    }
} 