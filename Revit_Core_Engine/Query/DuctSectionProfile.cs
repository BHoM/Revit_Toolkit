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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
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
        
        [Description("Query a Revit duct to extract a BHoM duct section profile.")]
        [Input("revitDuct", "Revit duct to be converted into a BHoM section profile.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("profile", "BHoM section profile for a duct extracted from a Revit duct.")]
        public static BH.oM.MEP.SectionProperties.SectionProfile DuctSectionProfile(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IProfile profile = revitDuct.Profile(settings, refObjects);

            // Lining thickness
            //double liningThickness = revitDuct.LookupParameterDouble("Lining Thickness");
            double liningThickness = revitDuct.LookupParameterDouble(BuiltInParameter.RBS_REFERENCE_LINING_THICKNESS);

            // Insulation thickness
            //double insulationThickness = revitDuct.LookupParameterDouble("Insulation Thickness");
            double insulationThickness = revitDuct.LookupParameterDouble(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);

            // Create a section profile
            if (profile != null)
            {
                return BH.Engine.MEP.Create.SectionProfile(profile as dynamic, liningThickness, insulationThickness);
            }
            else
            {
                return null;
            }
        }

        /***************************************************/
    }
}