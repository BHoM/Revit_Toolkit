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
using Autodesk.Revit.UI;
using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Adapter.Core
{
    public partial class RevitListenerAdapter
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public Output<List<object>, bool> HighlightByColors(Dictionary<string, object> input, ActionConfig actionConfig = null)
        {
            Output<List<object>, bool> output = new Output<List<object>, bool>() { Item1 = null, Item2 = false };

            // Validate and extract BHoMObjects
            if (!input.TryGetValue("BHoMObjects", out object objects) || !(objects is IEnumerable<BHoMObject> bHoMObjects) || !bHoMObjects.Any())
            {
                BH.Engine.Base.Compute.RecordError("BHoMObjects input is missing or invalid.");
                return output;
            }

            // Validate and extract Color
            if (!input.TryGetValue("Color", out object colorObj) || !(colorObj is System.Drawing.Color sysColor))
            {
                BH.Engine.Base.Compute.RecordError("Color input is missing or invalid.");
                return output;
            }

            // Convert System.Drawing.Color to Autodesk.Revit.DB.Color
            Autodesk.Revit.DB.Color revitColor = new Autodesk.Revit.DB.Color(sysColor.R, sysColor.G, sysColor.B);

            // Extract ElementIds from BHoMObjects
            List<ElementId> elementIds = bHoMObjects
                .Select(b => (b?.Fragments?.FirstOrDefault(f => f is RevitIdentifiers) as RevitIdentifiers)?.ElementId)
                .Where(id => id.HasValue)
                .Select(id => new ElementId(id.Value))
                .ToList();

            if (!elementIds.Any())
            {
                BH.Engine.Base.Compute.RecordError("No valid ElementIds found in BHoMObjects.");
                return output;
            }

            Document doc = this.Document;
            View view = doc.ActiveView;

            // Create OverrideGraphicSettings
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetProjectionLineColor(revitColor);
            ogs.SetSurfaceForegroundPatternColor(revitColor);

            // Retrieve solid fill pattern
            FillPatternElement solidFill = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(f => f.GetFillPattern().IsSolidFill);

            if (solidFill != null)
            {
                ogs.SetSurfaceForegroundPatternId(solidFill.Id);
            }

            // Apply overrides within a transaction
            using (Transaction tx = new Transaction(doc, "Apply Color Overrides"))
            {
                tx.Start();
                foreach (ElementId id in elementIds)
                {
                    view.SetElementOverrides(id, ogs);
                }
                tx.Commit();
            }

            output.Item1 = bHoMObjects.Cast<object>().ToList();
            output.Item2 = true;
            return output;
        }
    }
}