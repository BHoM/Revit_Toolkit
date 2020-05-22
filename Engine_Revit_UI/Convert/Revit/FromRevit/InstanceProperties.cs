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

using BH.Adapter.Revit;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static InstanceProperties InstancePropertiesFromRevit(this Autodesk.Revit.DB.ElementType elementType, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            InstanceProperties instanceProperties = refObjects.GetValue<InstanceProperties>(elementType.Id);
            if (instanceProperties != null)
                return instanceProperties;

            instanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(elementType.FamilyName, elementType.Name);

            //Set identifiers, parameters & custom data
            instanceProperties.SetIdentifiers(elementType);
            instanceProperties.SetCustomData(elementType, settings.ParameterSettings);
            instanceProperties.SetProperties(elementType, settings.ParameterSettings);
            instanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.FamilyName] = elementType.FamilyName;
            instanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.FamilyTypeName] = elementType.Name;
            
            refObjects.AddOrReplace(elementType.Id, instanceProperties);
            return instanceProperties;
        }

        /***************************************************/

        public static InstanceProperties InstancePropertiesFromRevit(this Autodesk.Revit.DB.GraphicsStyle graphicStyle, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            InstanceProperties instanceProperties = refObjects.GetValue<InstanceProperties>(graphicStyle.Id);
            if (instanceProperties != null)
                return instanceProperties;

            instanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(null, null);
            instanceProperties.Name = graphicStyle.Name;

            //Set identifiers, parameters & custom data
            instanceProperties.SetIdentifiers(graphicStyle);
            instanceProperties.SetCustomData(graphicStyle, settings.ParameterSettings);
            instanceProperties.SetProperties(graphicStyle, settings.ParameterSettings);
            instanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.CategoryName] = graphicStyle.GraphicsStyleCategory.Parent.Name;
            
            refObjects.AddOrReplace(graphicStyle.Id, instanceProperties);
            return instanceProperties;
        }

        /***************************************************/
    }
}

