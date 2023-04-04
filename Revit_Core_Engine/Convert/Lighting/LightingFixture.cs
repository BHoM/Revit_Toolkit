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

using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Convert a Revit family instance that is a fitting or an accessory to a BHoM Fitting.")]
        [Input("revitLightingFixture", "Revit family instance to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("fitting", "BHoM fitting object converted from a Revit family instance element.")]
        public static BH.oM.Lighting.Elements.Luminaire LuminaireFromRevit (this FamilyInstance revitLightingFixture, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            
            // Reuse a BHoM fitting from refObjects it it has been converted before
            BH.oM.Lighting.Elements.Luminaire bhomLight = refObjects.GetValue<BH.oM.Lighting.Elements.Luminaire>(revitLightingFixture.Id);
            if (bhomLight != null)
                return bhomLight;

            bhomLight = new BH.oM.Lighting.Elements.Luminaire()
            {
                Position = (revitLightingFixture.Location as LocationPoint)?.Point?.PointFromRevit(),
                Direction = (revitLightingFixture.GetTransform().BasisZ.VectorFromRevit())
            };
            bhomLight.LuminaireType = new oM.Lighting.Elements.LuminaireType()
            { Name = revitLightingFixture.FamilyTypeFullName() };

            //Set type
            revitLightingFixture.CopyTypeToFragment(bhomLight, settings, refObjects);

            //Set identifiers, parameters & custom data
            bhomLight.SetIdentifiers(revitLightingFixture);
            bhomLight.CopyParameters(revitLightingFixture, settings.MappingSettings);
            bhomLight.SetProperties(revitLightingFixture, settings.MappingSettings);

            refObjects.AddOrReplace(revitLightingFixture.Id, bhomLight);
            return bhomLight;
        }
        
        /***************************************************/
    }
}



