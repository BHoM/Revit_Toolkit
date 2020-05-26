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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static void SetProperties(this IObject iObject, Element element, ParameterSettings settings = null)
        {
            if (iObject == null || settings == null || settings.ParameterMaps == null || settings.ParameterMaps.Count == 0 || element == null)
                return;

            Type type = iObject.GetType();

            IEnumerable<PropertyInfo> propertyInfos = type.MapPropertyInfos();
            if (propertyInfos == null || propertyInfos.Count() == 0)
                return;

            Element elementType = element.Document.GetElement(element.GetTypeId());

            foreach (PropertyInfo pInfo in propertyInfos)
            {
                HashSet<string> names = settings.Names(type, pInfo.Name);
                if (names == null)
                    continue;

                foreach (string name in names)
                {
                    Parameter parameter;
                    if (name.Replace(" ", "").ToLower().StartsWith("type:"))
                    {
                        if (elementType == null)
                            continue;

                        string[] splitName = name.Split(new[] { ':' }, 2);
                        if (splitName.Length != 2)
                            continue;
                        
                        parameter = elementType.LookupParameter(splitName[1].TrimStart());
                    }
                    else
                        parameter = element.LookupParameter(name);

                    if (parameter == null)
                        continue;

                    Type typePropertyInfo = pInfo.PropertyType;

                    if (typePropertyInfo.IsEnum)
                    {
                        string value;
                        if (parameter.StorageType == StorageType.String)
                            value = parameter.AsString();
                        else
                            value = parameter.AsValueString();

                        string[] splitValue = value.ToLower().Split(new[] { ' ' });
                        foreach(object enumValue in Enum.GetValues(typePropertyInfo))
                        {
                            string valueString = enumValue.ToString().ToLower();
                            if (splitValue.All(x => valueString.Contains(x)))
                            {
                                pInfo.SetValue(iObject, enumValue);
                                break;
                            }
                        }
                    }
                    else if (typePropertyInfo == typeof(string))
                    {
                        if (parameter.StorageType == StorageType.String)
                            pInfo.SetValue(iObject, parameter.AsString());
                        else
                            pInfo.SetValue(iObject, parameter.AsValueString());
                    }
                    else if (typePropertyInfo == typeof(double))
                    {
                        double value = parameter.AsDouble().ToSI(parameter.Definition.UnitType);
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

                    break;
                }
            }
        }

        /***************************************************/
    }
}
