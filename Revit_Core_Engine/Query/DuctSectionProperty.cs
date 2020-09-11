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
using BH.Engine.MEP;
using BH.Engine.Spatial;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
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

        [Description("Duct section property converted from a Revit duct.")]
        [Input("revitDuct", "Revit duct to be conerted into a section property.")]
        [Input("settings", "Revit settings.")]
        [Input("refObjects", "Referenced objects.")]
        [Output("DuctSectionProperty", "BHoM duct section property.")]
        public static BH.oM.MEP.SectionProperties.DuctSectionProperty DuctSectionProperty(this Autodesk.Revit.DB.Mechanical.Duct revitDuct, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Duct section profile
            SectionProfile sectionProfile = revitDuct.DuctSectionProfile(settings, refObjects);

            // Duct section property
            // This is being constructed manually because BH.Engine.MEP.Create.DuctSectionProperty doesn't work with round ducts, as it attempts to calculate the circular equivalent of a round duct.
            // Solid Areas
            double elementSolidArea = sectionProfile.ElementProfile.Area();
            double liningSolidArea = sectionProfile.LiningProfile.Area() - sectionProfile.ElementProfile.Area();
            double insulationSolidArea = sectionProfile.InsulationProfile.Area();

            // Void Areas
            double elementVoidArea = sectionProfile.ElementProfile.VoidArea();
            double liningVoidArea = sectionProfile.LiningProfile.VoidArea();
            double insulationVoidArea = sectionProfile.InsulationProfile.VoidArea();

            // Get the duct shape, which is either circular, rectangular, oval or null
            Autodesk.Revit.DB.ConnectorProfileType ductShape = BH.Revit.Engine.Core.Query.DuctShape(revitDuct, settings);//revitDuct.DuctType.Shape;

            // Duct specific properties
            // Circular equivalent diameter
            double circularEquivalent = 0;
            // Is the duct rectangular?
            if (ductShape == Autodesk.Revit.DB.ConnectorProfileType.Rectangular)
            {
                circularEquivalent = sectionProfile.ElementProfile.ICircularEquivalentDiameter();
            }

            // Hydraulic diameter
            double hydraulicDiameter = BH.Engine.MEP.Query.HydraulicDiameter(sectionProfile as IProfile, elementSolidArea);

            return new BH.oM.MEP.SectionProperties.DuctSectionProperty(sectionProfile, elementSolidArea, liningSolidArea, insulationSolidArea, elementVoidArea, liningVoidArea, insulationVoidArea, hydraulicDiameter, circularEquivalent);
        }

        /***************************************************/
    }
}