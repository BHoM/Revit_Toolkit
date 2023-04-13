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
using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Lighting.Elements;
using BH.oM.Physical.Elements;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts Revit FamilySymbol to BHoM LuminaireType.")]
        [Input("familySymbol", "Revit FamilySymbol to be queried.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("LuminaireType", "BH.oM.Elements.Lighting.LuminaireType extracted from the input Revit FamilySymbol.")]
        public static LuminaireType LuminaireTypeFromRevit(this FamilySymbol familySymbol, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            LuminaireType luminaireType = refObjects.GetValue<LuminaireType>(familySymbol.Id);
            if (luminaireType != null)
                return luminaireType;

            luminaireType = new LuminaireType
            {
                Name = familySymbol.FamilyTypeFullName(),
            };

            //Set identifiers, parameters & custom data
            luminaireType.SetIdentifiers(familySymbol);
            luminaireType.CopyParameters(familySymbol, settings.MappingSettings);
            luminaireType.SetProperties(familySymbol, settings.MappingSettings);

            refObjects.AddOrReplace(familySymbol.Id, luminaireType);
            return luminaireType;
        }

        /***************************************************/
    }
}

