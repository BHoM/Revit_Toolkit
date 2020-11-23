/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.MEP.System.SectionProperties;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using BH.oM.MEP.System.MaterialFragments;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Query a Revit cable tray to extract a BHoM cable tray section property.")]
        [Input("revitCableTray", "Revit cable tray to be queried for information required for a BHoM section property.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("sectionProperty", "BHoM cable tray section property extracted from a Revit cable tray.")]
        public static BH.oM.MEP.System.SectionProperties.CableTraySectionProperty CableTraySectionProperty(this Autodesk.Revit.DB.Electrical.CableTray revitCableTray, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            // Cable Tray section profile
            SectionProfile sectionProfile = revitCableTray.CableTraySectionProfile(settings);

            // Cable Tray section property
            return BH.Engine.MEP.Create.CableTraySectionProperty(new CableTrayMaterial(), sectionProfile);
        }

        /***************************************************/
    }
}
