/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
        /****             Internal methods              ****/
        /***************************************************/

        internal static InstanceProperties ToBHoMInstanceProperties(this Autodesk.Revit.DB.ElementType elementType, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            InstanceProperties aInstanceProperties = pullSettings.FindRefObject<InstanceProperties>(elementType.Id.IntegerValue);
            if (aInstanceProperties != null)
                return aInstanceProperties;

            aInstanceProperties = BH.Engine.Adapters.Revit.Create.InstanceProperties(elementType.FamilyName, elementType.Name);

            aInstanceProperties = Modify.SetIdentifiers(aInstanceProperties, elementType) as InstanceProperties;
            if (pullSettings.CopyCustomData)
                aInstanceProperties = Modify.SetCustomData(aInstanceProperties, elementType, pullSettings.ConvertUnits) as InstanceProperties;

            aInstanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.FamilyName] = elementType.FamilyName;
            aInstanceProperties.CustomData[BH.Engine.Adapters.Revit.Convert.FamilyTypeName] = elementType.Name;

            aInstanceProperties = aInstanceProperties.UpdateValues(pullSettings, elementType) as InstanceProperties;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aInstanceProperties);

            return aInstanceProperties;
        }

        /***************************************************/
    }
}
