﻿/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Revit.Enums;
using BH.oM.Revit.Views;
using BH.Revit.Engine.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;
using FilterRule = BH.oM.Revit.Elements.FilterRule;
using OverrideGraphicSettings = BH.oM.Adapters.Revit.Elements.OverrideGraphicSettings;
using View = Autodesk.Revit.DB.View;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit View to BH.oM.Adapters.Revit.Elements.View.")]
        [Input("revitView", "Revit View to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("view", "BH.oM.Adapters.Revit.Elements.View resulting from converting the input Revit View.")]
        public static oM.Adapters.Revit.Elements.View ViewFromRevit(this View revitView, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.View view = refObjects.GetValue<oM.Adapters.Revit.Elements.View>(revitView.Id.IntegerValue);
            if (view != null)
                return view;

            /*1. Transfer ViewFilters and corresponding OverrideGraphicSettings into ViewFilterWithOverrides Objects - via STREAMS */
            List<ViewFilterWithOverrides> filtersWithOverrides = revitView.GetFilters().ToDictionary<ElementId, ViewFilter, OverrideGraphicSettings>
                                                        (elId => Convert.ViewFilterFromRevit((ParameterFilterElement)revitView.Document.GetElement(elId)),
                                                         elId => overrideGraphicSettingsFromRevit(revitView, revitView.GetFilterOverrides(elId))).ToList()
                                                         .Select(kvp => new ViewFilterWithOverrides { Filter = kvp.Key, Overrides = kvp.Value }).ToList();

            /*2. Create BHoM View Object with Name and FilterWithOverrides objects */
            view = new oM.Adapters.Revit.Elements.View { Name = revitView.Name, FiltersWithOverrides = filtersWithOverrides };

            //Set identifiers, parameters & custom data
            view.SetIdentifiers(revitView);
            view.CopyParameters(revitView, settings.MappingSettings);
            view.SetProperties(revitView, settings.MappingSettings);

            refObjects.AddOrReplace(revitView.Id, view);
            return view;
        }

        /***************************************************/

        private static OverrideGraphicSettings overrideGraphicSettingsFromRevit(this View element, Autodesk.Revit.DB.OverrideGraphicSettings revitOverrides)
        {
            // Initialize BHoM OverrideGraphicsSettings object
            OverrideGraphicSettings overrideGraphicsSettings = new OverrideGraphicSettings();

            // Convert COLORS 
            overrideGraphicsSettings.LineColor = System.Drawing.Color.FromArgb(revitOverrides.CutLineColor.Red, revitOverrides.CutLineColor.Green, revitOverrides.CutLineColor.Blue);
            overrideGraphicsSettings.CutColor = System.Drawing.Color.FromArgb(revitOverrides.CutBackgroundPatternColor.Red, revitOverrides.CutBackgroundPatternColor.Green, revitOverrides.CutBackgroundPatternColor.Blue);
            overrideGraphicsSettings.SurfaceColor = System.Drawing.Color.FromArgb(revitOverrides.SurfaceBackgroundPatternColor.Red, revitOverrides.SurfaceBackgroundPatternColor.Green, revitOverrides.SurfaceBackgroundPatternColor.Blue);

            // Convert LINE PATTERNS
            String linePatternName = element.Document.GetElement(revitOverrides.CutLinePatternId).Name;

            if (linePatternName.ToUpper().Contains("DASH")) { overrideGraphicsSettings.LinePattern = oM.Revit.Enums.LinePattern.DASH; }
            else if (linePatternName.ToUpper().Contains("DOT")) { overrideGraphicsSettings.LinePattern = oM.Revit.Enums.LinePattern.DOT; }
            else if (linePatternName.ToUpper().Contains("HIDDEN")) { overrideGraphicsSettings.LinePattern = oM.Revit.Enums.LinePattern.HIDDEN; }
            else if (linePatternName.ToUpper().Contains("SOLID")) { overrideGraphicsSettings.LinePattern = oM.Revit.Enums.LinePattern.SOLID; }
            else { BH.Engine.Base.Compute.RecordWarning($"The Revit Line Pattern {linePatternName} is not implemented yet in the BHoM.\n By default, the Line Pattern {linePatternName} will be set to SOLID."); }


            // Convert CUT PATTERNS
            String cutPatternName = element.Document.GetElement(revitOverrides.CutBackgroundPatternId).Name;

            if (cutPatternName.ToUpper().Contains("CROSSHATCH") && !cutPatternName.ToUpper().Contains("DIAGONAL")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.CROSSHATCH; }
            else if (cutPatternName.ToUpper().Contains("CROSSHATCH") && cutPatternName.ToUpper().Contains("DIAGONAL")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.DIAGONAL_CROSSHATCH; }
            else if (cutPatternName.ToUpper().Contains("DIAGONAL") && cutPatternName.ToUpper().Contains("DOWN")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.DIAGONAL_DOWN; }
            else if (cutPatternName.ToUpper().Contains("DIAGONAL") && cutPatternName.ToUpper().Contains("UP")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.DIAGONAL_UP; }
            else if (cutPatternName.ToUpper().Contains("HORIZONTAL")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.HORIZONTAL; }
            else if (cutPatternName.ToUpper().Contains("STEEL")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.STEEL; }
            else if (cutPatternName.ToUpper().Contains("SOLID")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.SOLID; }
            else if (cutPatternName.ToUpper().Contains("VERTICAL")) { overrideGraphicsSettings.CutPattern = oM.Revit.Enums.FillPattern.VERTICAL; }
            else { BH.Engine.Base.Compute.RecordWarning($"The Revit Fill Pattern {cutPatternName} is not implemented yet in the BHoM.\n By default, the Fill Pattern {cutPatternName} will be set to SOLID."); }


            // Convert SURFACE PATTERNS
            String surfacePatternName = element.Document.GetElement(revitOverrides.SurfaceBackgroundPatternId).Name;

            if (surfacePatternName.ToUpper().Contains("CROSSHATCH") && !surfacePatternName.ToUpper().Contains("DIAGONAL")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.CROSSHATCH; }
            else if (surfacePatternName.ToUpper().Contains("CROSSHATCH") && surfacePatternName.ToUpper().Contains("DIAGONAL")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.DIAGONAL_CROSSHATCH; }
            else if (surfacePatternName.ToUpper().Contains("DIAGONAL") && surfacePatternName.ToUpper().Contains("DOWN")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.DIAGONAL_DOWN; }
            else if (surfacePatternName.ToUpper().Contains("DIAGONAL") && surfacePatternName.ToUpper().Contains("UP")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.DIAGONAL_UP; }
            else if (surfacePatternName.ToUpper().Contains("HORIZONTAL")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.HORIZONTAL; }
            else if (surfacePatternName.ToUpper().Contains("STEEL")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.STEEL; }
            else if (surfacePatternName.ToUpper().Contains("SOLID")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.SOLID; }
            else if (surfacePatternName.ToUpper().Contains("VERTICAL")) { overrideGraphicsSettings.SurfacePattern = oM.Revit.Enums.FillPattern.VERTICAL; }
            else { BH.Engine.Base.Compute.RecordWarning($"The Revit Fill Pattern {surfacePatternName} is not implemented yet in the BHoM.\n By default, the Fill Pattern {surfacePatternName} will be set to SOLID."); }

            return overrideGraphicsSettings;
        }

    }
}