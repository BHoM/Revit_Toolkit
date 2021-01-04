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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

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
            instanceProperties.CopyParameters(elementType, settings.ParameterSettings);
            instanceProperties.SetProperties(elementType, settings.ParameterSettings);
            
            refObjects.AddOrReplace(elementType.Id, instanceProperties);
            return instanceProperties;
        }

        /***************************************************/

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
            instanceProperties.CopyParameters(graphicStyle, settings.ParameterSettings);
            instanceProperties.SetProperties(graphicStyle, settings.ParameterSettings);
            
            refObjects.AddOrReplace(graphicStyle.Id, instanceProperties);
            return instanceProperties;
        }

        /***************************************************/
    }
}


