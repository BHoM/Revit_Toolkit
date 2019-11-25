/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Properties;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static InstanceProperties ToBHoMInstanceProperties(this Autodesk.Revit.DB.ElementType elementType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            InstanceProperties instanceProperties = pullSettings.FindRefObject<InstanceProperties>(elementType.Id.IntegerValue);
            if (instanceProperties != null)
                return instanceProperties;

            instanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(elementType.FamilyName, elementType.Name);

            instanceProperties = Modify.SetIdentifiers(instanceProperties, elementType) as InstanceProperties;
            if (pullSettings.CopyCustomData)
                instanceProperties = Modify.SetCustomData(instanceProperties, elementType) as InstanceProperties;

            instanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.FamilyName] = elementType.FamilyName;
            instanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.FamilyTypeName] = elementType.Name;

            instanceProperties = instanceProperties.UpdateValues(pullSettings, elementType) as InstanceProperties;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(instanceProperties);

            return instanceProperties;
        }

        /***************************************************/

        public static InstanceProperties ToBHoMInstanceProperties(this Autodesk.Revit.DB.GraphicsStyle graphicStyle, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            InstanceProperties instanceProperties = pullSettings.FindRefObject<InstanceProperties>(graphicStyle.Id.IntegerValue);
            if (instanceProperties != null)
                return instanceProperties;

            instanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(null, null);
            instanceProperties.Name = graphicStyle.Name;

            instanceProperties = Modify.SetIdentifiers(instanceProperties, graphicStyle) as InstanceProperties;
            if (pullSettings.CopyCustomData)
                instanceProperties = Modify.SetCustomData(instanceProperties, graphicStyle) as InstanceProperties;

            instanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.CategoryName] = graphicStyle.GraphicsStyleCategory.Parent.Name;

            instanceProperties = instanceProperties.UpdateValues(pullSettings, graphicStyle) as InstanceProperties;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(instanceProperties);

            return instanceProperties;
        }

        /***************************************************/
    }
}
