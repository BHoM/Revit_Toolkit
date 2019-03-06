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

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IObject UpdateValues(this IObject iObject, PullSettings pullSettings, Element Element)
        {
            if (iObject == null)
                return null;

            if (pullSettings == null || pullSettings.MapSettings == null)
                return iObject;

            Type aType = iObject.GetType();

            IEnumerable<PropertyInfo> aPropertyInfos = BH.Engine.Adapters.Revit.Query.MapPropertyInfos(aType);
            if (aPropertyInfos == null || aPropertyInfos.Count() == 0)
                return iObject;

            MapSettings aMapSettings = pullSettings.MapSettings;

            foreach (PropertyInfo aPropertyInfo in aPropertyInfos)
            {
                Parameter aParameter = Element.LookupParameter(aMapSettings, aType, aPropertyInfo.Name, false);
                if (aParameter == null)
                    continue;

                Type aType_PropertyInfo = aPropertyInfo.PropertyType;

                if (aType_PropertyInfo == typeof(string))
                    aPropertyInfo.SetValue(iObject, aParameter.AsString());
                else if (aType_PropertyInfo == typeof(double))
                    aPropertyInfo.SetValue(iObject, aParameter.AsDouble());
                else if (aType_PropertyInfo == typeof(int) || aType_PropertyInfo == typeof(short) || aType_PropertyInfo == typeof(long))
                {
                    if (aParameter.StorageType == StorageType.ElementId)
                        aPropertyInfo.SetValue(iObject, aParameter.AsElementId());
                    else
                        aPropertyInfo.SetValue(iObject, aParameter.AsInteger());
                }
                else if (aType_PropertyInfo == typeof(bool))
                    aPropertyInfo.SetValue(iObject, aParameter.AsInteger() == 1);
            }

            return iObject;
        }

        /***************************************************/
    }
}
