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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using BH.oM.Lighting.Elements;
using BH.oM.Physical.Elements;
using BH.oM.Spatial.SettingOut;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Lighting.Elements.Luminaire to a Revit LightingFixture.")]
        [Input("luminaire", "BH.oM.Lighting.Elements.Luminaire to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("lightingFixture", "Revit LightingFixture element resulting from the BH.oM.Lighting.Elements.Luminaire.")]
        public static FamilyInstance ToRevitFamilyInstance (this Luminaire luminaire, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (luminaire == null || document == null)
                return null;

            FamilyInstance familyInstance = refObjects.GetValue<FamilyInstance>(document, luminaire.BHoM_Guid);
            if (familyInstance != null)
                return familyInstance;

            settings = settings.DefaultIfNull();

            FamilySymbol familySymbol = luminaire.ElementType(document, settings) as FamilySymbol;
            if (familySymbol == null)
            {
                Compute.ElementTypeNotFoundWarning(luminaire);
                return null;
            }

            if (luminaire.Position != null)
                return Create.FamilyInstance(document, familySymbol, (luminaire.Position).ToRevit(), luminaire.Orientation.ToRevit(), luminaire.HostElement(document, settings), settings);
            else
            {
                BH.Engine.Base.Compute.RecordError($"An element could not be created based on the given luminaire because its location was invalid. BHoM_Guid: {luminaire.BHoM_Guid}");
                return null;
            }
        }
        
        /***************************************************/
    }
}



