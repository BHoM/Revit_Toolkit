/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Adapters.Revit.Elements.Sheet to a Revit ViewSheet.")]
        [Input("sheet", "BH.oM.Adapters.Revit.Elements.Sheet to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("viewSheet", "Revit ViewSheet resulting from converting the input BH.oM.Adapters.Revit.Elements.Sheet.")]
        public static ViewSheet ToRevitSheet(this Sheet sheet, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (sheet == null)
                return null;

            ViewSheet viewSheet = refObjects.GetValue<ViewSheet>(document, sheet.BHoM_Guid);
            if (viewSheet != null)
                return viewSheet;

            settings = settings.DefaultIfNull();

            viewSheet = ViewSheet.Create(document, ElementId.InvalidElementId);
            viewSheet.Name = sheet.SheetName;
            viewSheet.SheetNumber = sheet.SheetNumber;

            // Copy parameters from BHoM object to Revit element
            viewSheet.CopyParameters(sheet, settings);

            refObjects.AddOrReplace(sheet, viewSheet);
            return viewSheet;
        }

        /***************************************************/
    }
}



