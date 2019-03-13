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

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Common.Properties;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static Object2DProperties ToBHoMObject2DProperties(this WallType wallType, PullSettings pullSettings = null)
        {
            return ToBHoMObject2DProperties((HostObjAttributes)wallType);
        }

        /***************************************************/

        internal static Object2DProperties ToBHoMObject2DProperties(this HostObjAttributes hostObjAttributes, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            Object2DProperties aObject2DProperties = pullSettings.FindRefObject<Object2DProperties>(hostObjAttributes.Id.IntegerValue);
            if (aObject2DProperties != null)
                return aObject2DProperties;

            aObject2DProperties = new Object2DProperties()
            {
                CompoundLayers = Query.CompoundLayers(hostObjAttributes, pullSettings)
            };

            aObject2DProperties = Modify.SetIdentifiers(aObject2DProperties, hostObjAttributes) as Object2DProperties;
            if (pullSettings.CopyCustomData)
                aObject2DProperties = Modify.SetCustomData(aObject2DProperties, hostObjAttributes, pullSettings.ConvertUnits) as Object2DProperties;

            aObject2DProperties = aObject2DProperties.UpdateValues(pullSettings, hostObjAttributes) as Object2DProperties;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aObject2DProperties);

            return aObject2DProperties;
        }
    }
}