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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit ViewSheet to BH.oM.Adapters.Revit.Elements.Sheet.")]
        [Input("viewSheet", "Revit ViewSheet to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("sheet", "BH.oM.Adapters.Revit.Elements.Sheet resulting from converting the input Revit ViewSheet.")]
        public static Sheet SheetFromRevit(this ViewSheet viewSheet, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Sheet sheet = refObjects.GetValue<Sheet>(viewSheet.Id);
            if (sheet != null)
                return sheet;

            sheet = BH.Engine.Adapters.Revit.Create.Sheet(viewSheet.Name, viewSheet.SheetNumber);

            ElementType elementType = viewSheet.Document.GetElement(viewSheet.GetTypeId()) as ElementType;
            if (elementType != null)
                sheet.InstanceProperties = elementType.InstancePropertiesFromRevit(settings, refObjects);

            sheet.Name = viewSheet.Name;

            //Set identifiers, parameters & custom data
            sheet.SetIdentifiers(viewSheet);
            sheet.CopyParameters(viewSheet, settings.MappingSettings);
            sheet.SetProperties(viewSheet, settings.MappingSettings);

            refObjects.AddOrReplace(viewSheet.Id, sheet);
            return sheet;
        }

        /***************************************************/
    }
}






