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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Searches a Revit Element for the first existing parameter under one of the given names.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterNames", "Names of the parameter to be iterated over in search for the parameter.")]
        [Input("parameterGroups", "If not null, only the Revit parameters from the given parameter groups will be parsed.")]
        [Output("parameter", "Parameter extracted from the input Revit element.")]
        public static Parameter LookupParameter(this Element element, IEnumerable<string> parameterNames, IEnumerable<BuiltInParameterGroup> parameterGroups = null)
        {
            if (parameterNames == null || !parameterNames.Any())
                return null;

            foreach (string name in parameterNames)
            {
                foreach (Parameter p in element.Parameters)
                {
                    if (p != null && p.HasValue && p.Definition.Name == name)
                    {
                        if (parameterGroups == null || parameterGroups.Any(x => x == p.Definition.ParameterGroup))
                            return p;
                    }
                }
            }

            return null;
        }

        [Description("Searches a Revit Family Instance for the first existing parameter of a given name, by first looking for Instance parameters and then, optionally, for Type parameters.")]
        [Input("element", "Revit element to be queried.")]
        [Input("parameterName", "Names of the parameter to be iterated over in search for the parameter.")]
        [Input("allowTypeParameters", "Optional, whether or not to also look for Type parameters if no Instance parameters were found.")]
        [Output("parameter", "Parameter extracted from the input Revit Family Instance.")]
        public static Parameter LookupParameter(this Element element, string parameterName, bool allowTypeParameters)
        {
            if (element == null || string.IsNullOrEmpty(parameterName))
                return null;
           
            // Try to return an Instance Parameter
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null)
                return parameter;

            if (!allowTypeParameters)
                return null;
            
            // Try to return a Type Parameter
            Element elementSymbol = element.Document.GetElement(element.GetTypeId());
            return elementSymbol?.LookupParameter(parameterName);
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given name and returns this value as a double.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterName", "Name of the parameter to be queried.")]
        [Input("convertUnits", "If true, the output will be converted from Revit internal units to SI.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static double LookupParameterDouble(this Element element, string parameterName, bool convertUnits = true)
        {
            double value = double.NaN;

            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = value.ToSI(p.Definition.GetDataType());
            }

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of the first existing parameter under one of the given names and returns this value as a double.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterNames", "Names of the parameter to be iterated over in search for the parameter.")]
        [Input("convertUnits", "If true, the output will be converted from Revit internal units to SI.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static double LookupParameterDouble(this Element element, IEnumerable<string> parameterNames, bool convertUnits = true)
        {
            double value = double.NaN;
            foreach (string name in parameterNames)
            {
                value = element.LookupParameterDouble(name, convertUnits);
                if (!double.IsNaN(value))
                    return value;
            }

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of the first existing parameter under one of the given names and returns this value as a double.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterNames", "Names of the parameter to be iterated over in search for the parameter.")]
        [Input("parameterGroups", "If not null, only the Revit parameters from the given parameter groups will be parsed.")]
        [Input("convertUnits", "If true, the output will be converted from Revit internal units to SI.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static double LookupParameterDouble(this Element element, IEnumerable<string> parameterNames, IEnumerable<BuiltInParameterGroup> parameterGroups, bool convertUnits = true)
        {
            double value = double.NaN;
            Parameter p = element.LookupParameter(parameterNames, parameterGroups);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = value.ToSI(p.Definition.GetDataType());
            }

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given BuiltInParameter and returns this value as a double.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("builtInParameter", "BuiltInParameter identifier of the parameter to be queried.")]
        [Input("convertUnits", "If true, the output will be converted from Revit internal units to SI.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static double LookupParameterDouble(this Element element, BuiltInParameter builtInParameter, bool convertUnits = true)
        {
            double value = double.NaN;

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = value.ToSI(p.Definition.GetDataType());
            }

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given name and returns this value as an interger.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterName", "Name of the parameter to be queried.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static int LookupParameterInteger(this Element element, string parameterName)
        {
            int value = -1;

            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
                value = p.AsInteger();

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given BuiltInParameter and returns this value as an integer.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("builtInParameter", "BuiltInParameter identifier of the parameter to be queried.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static int LookupParameterInteger(this Element element, BuiltInParameter builtInParameter)
        {
            int value = -1;

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
                value = p.AsInteger();

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given name and returns this value as a Revit ElementId.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterName", "Name of the parameter to be queried.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static ElementId LookupParameterElementId(this Element element, string parameterName)
        {
            ElementId value = new ElementId(-1);

            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
                value = p.AsElementId();

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given BuiltInParameter and returns this value as a Revit ElementId.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("builtInParameter", "BuiltInParameter identifier of the parameter to be queried.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static ElementId LookupParameterElementId(this Element element, BuiltInParameter builtInParameter)
        {
            ElementId value = new ElementId(-1);

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
                value = p.AsElementId();

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of the first existing parameter under one of the given names and returns this value as a string.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterNames", "Names of the parameter to be iterated over in search for the parameter.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static string LookupParameterString(this Element element, IEnumerable<string> parameterNames)
        {
            string value = null;
            foreach (string name in parameterNames)
            {
                value = element.LookupParameterString(name);
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return value;
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given name and returns this value as a string.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("parameterName", "Name of the parameter to be queried.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static string LookupParameterString(this Element element, string parameterName)
        {
            return element.LookupParameter(parameterName).StringValue();
        }

        /***************************************************/

        [Description("Queries a Revit Element for the value of a parameter under the given BuiltInParameter and returns this value as a string.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("builtInParameter", "BuiltInParameter identifier of the parameter to be queried.")]
        [Output("value", "Parameter value extracted from the input Revit element.")]
        public static string LookupParameterString(this Element element, BuiltInParameter builtInParameter)
        {
            return element.get_Parameter(builtInParameter).StringValue();
        }

        /***************************************************/

        [Description("Queries a Revit Element for a parameter under the given name, additionally taking the mapping settings into account.")]
        [Input("element", "Revit Element to be queried.")]
        [Input("mappingSettings", "MappingSettings containing the information about the relationships between parameter names as they stand on BHoM objects and parameter names of correspondent Revit elements.")]
        [Input("type", "BHoM type, of which parameter mapping will be applied.")]
        [Input("name", "Name of the parameter to be queried.")]
        [Input("mustHaveValue", "If true, only parameters with values will be returned.")]
        [Output("parameter", "Parameter extracted from the input Revit element.")]
        public static Parameter LookupParameter(this Element element, MappingSettings mappingSettings, Type type, string name, bool mustHaveValue = true)
        {
            if (element == null || mappingSettings == null || type == null)
                return null;

            // Lookup element parameter.
            List<string> names = new List<string> { name };
            HashSet<string> paramNames = mappingSettings.ParameterNames(type, name, false);
            if (paramNames != null)
                names.AddRange(paramNames);
            
            foreach (string val in names)
            {
                Parameter parameter = element.LookupParameter(val);
                if (parameter == null)
                    continue;

                if (parameter.HasValue || !mustHaveValue)
                    return parameter;
            }

            // Lookup element type parameter (if specified in mappingSettings).
            paramNames = mappingSettings.ParameterNames(type, name, true);
            if (paramNames == null || paramNames.Count == 0)
                return null;

            Element elementType = element.Document.GetElement(element.GetTypeId());
            if (elementType == null)
                return null;

            foreach (string val in paramNames)
            {
                Parameter parameter = elementType.LookupParameter(val);
                if (parameter == null)
                    continue;

                if (parameter.HasValue || !mustHaveValue)
                    return parameter;
            }

            return null;
        }

        /***************************************************/

    }
}



