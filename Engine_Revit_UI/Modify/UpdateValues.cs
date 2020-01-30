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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IObject UpdateValues(this IObject iObject, PullSettings pullSettings, Element element)
        {
            if (iObject == null)
                return null;

            if (pullSettings == null || pullSettings.MapSettings == null || element == null)
                return iObject;

            Type type = iObject.GetType();

            IEnumerable<PropertyInfo> propertyInfos = BH.Engine.Adapters.Revit.Query.MapPropertyInfos(type);
            if (propertyInfos == null || propertyInfos.Count() == 0)
                return iObject;

            MapSettings mapSettings = pullSettings.MapSettings;

            foreach (PropertyInfo pInfo in propertyInfos)
            {
                Parameter parameter = element.LookupParameter(mapSettings, type, pInfo.Name, false);
                if (parameter == null)
                    continue;

                Type typePropertyInfo = pInfo.PropertyType;

                if (typePropertyInfo == typeof(string))
                {
                    pInfo.SetValue(iObject, parameter.AsString());
                }
                else if (typePropertyInfo == typeof(double))
                {
                    double value = parameter.AsDouble();
                    pInfo.SetValue(iObject, value);
                }
                    
                else if (typePropertyInfo == typeof(int) || typePropertyInfo == typeof(short) || typePropertyInfo == typeof(long))
                {
                    if (parameter.StorageType == StorageType.ElementId)
                        pInfo.SetValue(iObject, parameter.AsElementId());
                    else
                        pInfo.SetValue(iObject, parameter.AsInteger());
                }
                else if (typePropertyInfo == typeof(bool))
                {
                    pInfo.SetValue(iObject, parameter.AsInteger() == 1);
                }
                    
            }

            return iObject;
        }

        /***************************************************/
    }
}

