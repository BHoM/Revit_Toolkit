/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Query a Revit duct to extract a BHoM duct section property.")]
        [Input("revitDuct", "Revit duct to be queried for information required for a BHoM section property.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("sectionProfile", "BHoM duct section property extracted from a Revit duct.")]
        public static List<SectionProfile> DuctSectionProfile(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            // 
            object ductSection = revitDuct.DuctSectionProfile();

            // Duct element section
            ElementSection elementSection = new oM.MEP.System.SectionProperties.ElementSection() { ElementSize = , };


            // Duct section property
            return BH.Engine.MEP.Create.SectionProfile(elementSize, sectionProfile);
        }

        /***************************************************/
    }
}
