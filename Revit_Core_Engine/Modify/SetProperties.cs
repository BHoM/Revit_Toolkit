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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
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
                HashSet<string> parameterNames = settings.ParameterNames(type, pInfo.Name, false);
                if (parameterNames != null && iObject.SetProperty(pInfo, element, parameterNames))
                    continue;

                if (elementType == null)
                    continue;

                parameterNames = settings.ParameterNames(type, pInfo.Name, true);
                if (parameterNames != null)
                    iObject.SetProperty(pInfo, elementType, parameterNames);
            }
        }
        
        /***************************************************/

        public static bool SetProperty(this IObject iObject, PropertyInfo propertyInfo, Element element, IEnumerable<string> parameterNames)
        {
            foreach (string name in parameterNames)
            {
                Parameter parameter = element.LookupParameter(name);
                if (parameter != null)
                {
                    iObject.SetProperty(propertyInfo, parameter);
                    return true;
                }
            }

            return false;
        }

        /***************************************************/

        public static void SetProperty(this IObject iObject, PropertyInfo propertyInfo, Parameter parameter)
        {
            Type typePropertyInfo = propertyInfo.PropertyType;

            if (typePropertyInfo.IsEnum)
            {
                string value;
                if (parameter.StorageType == StorageType.String)
                    value = parameter.AsString();
                else
                    value = parameter.AsValueString();

                string[] splitValue = value.ToLower().Split(new[] { ' ' });
                foreach (object enumValue in Enum.GetValues(typePropertyInfo))
                {
                    string valueString = enumValue.ToString();
                    string lowerValueString = enumValue.ToString().ToLower();
                    if (valueString.Count(x => char.IsUpper(x)) == splitValue.Length && splitValue.All(x => lowerValueString.Contains(x)))
                    {
                        propertyInfo.SetValue(iObject, enumValue);
                        break;
                    }
                }
            }
            else if (typePropertyInfo == typeof(string))
            {
                if (parameter.StorageType == StorageType.String)
                    propertyInfo.SetValue(iObject, parameter.AsString());
                else
                    propertyInfo.SetValue(iObject, parameter.AsValueString());
            }
            else if (typePropertyInfo == typeof(double))
            {
                double value = parameter.AsDouble().ToSI(parameter.Definition.UnitType);
                propertyInfo.SetValue(iObject, value);
            }

            else if (typePropertyInfo == typeof(int) || typePropertyInfo == typeof(short) || typePropertyInfo == typeof(long))
            {
                if (parameter.StorageType == StorageType.ElementId)
                    propertyInfo.SetValue(iObject, parameter.AsElementId());
                else
                    propertyInfo.SetValue(iObject, parameter.AsInteger());
            }
            else if (typePropertyInfo == typeof(bool))
            {
                propertyInfo.SetValue(iObject, parameter.AsInteger() == 1);
            }
        }

        /***************************************************/
    }
}

