/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit ElementType to BH.oM.Adapters.Revit.Properties.InstanceProperties.")]
        [Input("elementType", "Revit ElementType to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("properties", "BH.oM.Adapters.Revit.Properties.InstanceProperties resulting from converting the input Revit ElementType.")]
        public static InstanceProperties InstancePropertiesFromRevit(this ElementType elementType, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            InstanceProperties instanceProperties = refObjects.GetValue<InstanceProperties>(elementType.Id);
            if (instanceProperties != null)
                return instanceProperties;

            instanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(elementType.FamilyName, elementType.Name);
            instanceProperties.CategoryName = elementType.Category?.Name;

            //Set identifiers, parameters & custom data
            instanceProperties.SetIdentifiers(elementType);
            instanceProperties.CopyParameters(elementType, settings.MappingSettings);
            instanceProperties.SetProperties(elementType, settings.MappingSettings);
            
            refObjects.AddOrReplace(elementType.Id, instanceProperties);
            return instanceProperties;
        }

        /***************************************************/

        [Description("Converts a Revit GraphicsStyle to BH.oM.Adapters.Revit.Properties.InstanceProperties.")]
        [Input("graphicStyle", "Revit GraphicsStyle to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("properties", "BH.oM.Adapters.Revit.Properties.InstanceProperties resulting from converting the input Revit GraphicsStyle.")]
        public static InstanceProperties InstancePropertiesFromRevit(this GraphicsStyle graphicStyle, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            InstanceProperties instanceProperties = refObjects.GetValue<InstanceProperties>(graphicStyle.Id);
            if (instanceProperties != null)
                return instanceProperties;

            instanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(null, null);
            instanceProperties.Name = graphicStyle.Name;
            instanceProperties.CategoryName = graphicStyle.Category?.Name;

            //Set identifiers, parameters & custom data
            instanceProperties.SetIdentifiers(graphicStyle);
            instanceProperties.CopyParameters(graphicStyle, settings.MappingSettings);
            instanceProperties.SetProperties(graphicStyle, settings.MappingSettings);
            
            refObjects.AddOrReplace(graphicStyle.Id, instanceProperties);
            return instanceProperties;
        }

        /***************************************************/
    }
}



