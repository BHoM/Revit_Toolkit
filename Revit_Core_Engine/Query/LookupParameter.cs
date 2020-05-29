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
using System.Collections.Generic;
using System.Linq;

using BH.oM.Adapters.Revit.Settings;

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static double LookupParameterDouble(this Element element, string parameterName, bool convertUnits = true)
        {
            double value = double.NaN;

            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = value.ToSI(p.Definition.UnitType);
            }

            return value;
        }

        /***************************************************/

        public static double LookupParameterDouble(this Element element, IEnumerable<string> parameterNames, bool convertUnits = true)
        {
            double value = double.NaN;
            foreach(string name in parameterNames)
            {
                value = element.LookupParameterDouble(name, convertUnits);
                if (!double.IsNaN(value))
                    return value;
            }

            return value;
        }

        /***************************************************/

        public static double LookupParameterDouble(this Element element, BuiltInParameter builtInParameter, bool convertUnits = true)
        {
            double value = double.NaN;

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
            {
                value = p.AsDouble();
                if (convertUnits)
                    value = value.ToSI(p.Definition.UnitType);
            }

            return value;
        }

        /***************************************************/

        public static int LookupParameterInteger(this Element element, string parameterName)
        {
            int value = -1;

            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
                value = p.AsInteger();

            return value;
        }

        /***************************************************/

        public static int LookupParameterInteger(this Element element, BuiltInParameter builtInParameter)
        {
            int value = -1;

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
                value = p.AsInteger();

            return value;
        }

        /***************************************************/

        public static ElementId LookupParameterElementId(this Element element, string parameterName)
        {
            ElementId value = new ElementId(-1);

            Parameter p = element.LookupParameter(parameterName);
            if (p != null && p.HasValue)
                value = p.AsElementId();

            return value;
        }

        /***************************************************/

        public static ElementId LookupParameterElementId(this Element element, BuiltInParameter builtInParameter)
        {
            ElementId value = new ElementId(-1);

            Parameter p = element.get_Parameter(builtInParameter);
            if (p != null && p.HasValue)
                value = p.AsElementId();

            return value;
        }

        /***************************************************/

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

        public static string LookupParameterString(this Element element, string parameterName)
        {
            return element.LookupParameter(parameterName).StringValue();
        }

        /***************************************************/

        public static string LookupParameterString(this Element element, BuiltInParameter builtInParameter)
        {
            return element.get_Parameter(builtInParameter).StringValue();
        }

        /***************************************************/

        public static Parameter LookupParameter(this Element element, ParameterSettings parameterSettings, Type type, string name, bool mustHaveValue = true)
        {
            if (element == null || parameterSettings == null || type == null)
                return null;

            // Lookup element parameter.
            List<string> names = new List<string> { name };
            HashSet<string> paramNames = parameterSettings.ParameterNames(type, name, false);
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

            // Lookup element type parameter (if specified in parameterSettings).
            paramNames = parameterSettings.ParameterNames(type, name, true);
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
