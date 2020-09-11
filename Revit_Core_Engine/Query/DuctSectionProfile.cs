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

        [Description("Section profile of round and rectangular ducts converted from a Revit duct.")]
        [Input("revitDuct", "Revit duct to be converted into a section profile.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("Duct Section Profile", "Converted BHoM duct section profile converted.")]
        public static BH.oM.MEP.SectionProperties.SectionProfile DuctSectionProfile(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            IProfile profile = revitDuct.ProfileFromRevit(settings, refObjects);

            // Lining thickness
            double liningThickness = revitDuct.LookupParameterDouble("Lining Thickness");

            // Insulation thickness
            double insulationThickness = revitDuct.LookupParameterDouble("Insulation Thickness");
            
            // Get the duct shape, which is either circular, rectangular, oval or null
            Autodesk.Revit.DB.ConnectorProfileType ductShape = revitDuct.DuctType.Shape;

            // Is the duct circular, rectangular or oval?
            switch (ductShape)
            {
                case Autodesk.Revit.DB.ConnectorProfileType.Round:
                    return BH.Engine.MEP.Create.SectionProfile((TubeProfile)profile, liningThickness, insulationThickness);
                case Autodesk.Revit.DB.ConnectorProfileType.Rectangular:
                    return BH.Engine.MEP.Create.SectionProfile((BoxProfile)profile, liningThickness, insulationThickness);
                case Autodesk.Revit.DB.ConnectorProfileType.Oval:
                    // Create an oval profile
                    // There is currently no section profile for an oval duct in BHoM_Engine. This part will be implemented once the relevant section profile becomes available.
                    BH.Engine.Reflection.Compute.RecordWarning("There is currently no section profile for an oval duct. Element ID: " + revitDuct.Id);
                    return null;
                default:
                    return null;
            }
        }

        /***************************************************/
    }
}