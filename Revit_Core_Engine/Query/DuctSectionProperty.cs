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
using BH.oM.MEP.SectionProperties;
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

        [Description("Convert a Revit duct into a BHoM duct section property.")]
        [Input("revitDuct", "Revit duct to be converted into a BHoM section property.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("sectionProperty", "BHoM duct section property converted from a Revit duct.")]
        public static BH.oM.MEP.SectionProperties.DuctSectionProperty DuctSectionProperty(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Duct section profile
            SectionProfile sectionProfile = revitDuct.DuctSectionProfile(settings, refObjects);

            // Duct section property
            return BH.Engine.MEP.Create.DuctSectionProperty(sectionProfile);
        }

        /***************************************************/
    }
}