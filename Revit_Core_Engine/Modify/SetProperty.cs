/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Iterates over a given collection of parameter names on a Revit element until an existing parameter is found. Then copies its value to a given property of a BHoM object.")]
        [Input("iObject", "Target BHoM object to copy the parameter value to.")]
        [Input("propertyInfo", "Target property of a BHoM object to copy the parameter value to.")]
        [Input("element", "Source Revit Element to copy the parameter value from.")]
        [Input("parameterNames", "Collection of parameter names to iterate over in search for one to copy.")]
        [Output("success", "True if an existing parameter has been found under one of the names and got successfully copied to the BHoM object, otherwise false.")]
        public static bool SetProperty(this IObject iObject, PropertyInfo propertyInfo, Element element, IEnumerable<string> parameterNames)
        {
            if (iObject == null || element == null || parameterNames == null)
                return false;

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

        [Description("Copies a value of Revit parameter to a given property of a BHoM object.")]
        [Input("iObject", "Target BHoM object to copy the parameter value to.")]
        [Input("propertyInfo", "Target property of a BHoM object to copy the parameter value to.")]
        [Input("parameter", "Source Revit Parameter to copy the value from.")]
        public static void SetProperty(this IObject iObject, PropertyInfo propertyInfo, Parameter parameter)
        {
            if (iObject == null || propertyInfo == null || parameter == null)
                return;

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
                double value = parameter.AsDouble().ToSI(parameter.Definition.GetDataType());
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



