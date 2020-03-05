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
using BH.oM.Base;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool SetParameter(this Element element, string parameterName, double value, bool convertUnits = true)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                if (convertUnits)
                    value = value.FromSI(parameter.Definition.UnitType);

                return parameter.Set(value);
            }

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, double value, bool convertUnits = true)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                if (convertUnits)
                    value = value.FromSI(parameter.Definition.UnitType);

                return parameter.Set(value);
            }

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, string parameterName, int value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Integer)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, int value)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Integer)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, string parameterName, string value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, string value)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, string parameterName, ElementId value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.ElementId)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, ElementId value)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.ElementId)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        public static bool SetParameter(this Parameter parameter, object value, Document document = null)
        {
            if (parameter == null || parameter.IsReadOnly)
                return false;

            switch (parameter.StorageType)
            {
                case StorageType.Double:
                    {
                        double dbl = double.NaN;

                        if (value is double)
                            dbl = (double)value;
                        else if (value is int || value is byte || value is float || value is long)
                            dbl = System.Convert.ToDouble(value);
                        else if (value is bool)
                        {
                            if ((bool)value)
                                dbl = 1.0;
                            else
                                dbl = 0.0;
                        }
                        else if (value is string)
                        {
                            if (!double.TryParse((string)value, out dbl))
                                dbl = double.NaN;
                        }

                        if (!double.IsNaN(dbl))
                        {
                            try
                            {
                                dbl = Convert.FromSI(dbl, parameter.Definition.UnitType);
                            }
                            catch
                            {
                                dbl = double.NaN;
                            }

                            if (!double.IsNaN(dbl))
                                return parameter.Set(dbl);
                        }
                        break;
                    }
                case StorageType.ElementId:
                    {
                        ElementId elementID = null;

                        if (value is int)
                            elementID = new ElementId((int)value);
                        else if (value is string)
                        {
                            int num;
                            if (int.TryParse((string)value, out num))
                                elementID = new ElementId(num);
                        }
                        else if (value is IBHoMObject)
                        {
                            elementID = (value as IBHoMObject).ElementId();
                        }
                        else if (value != null)
                        {
                            int num;
                            if (int.TryParse(value.ToString(), out num))
                                elementID = new ElementId(num);
                        }

                        if (elementID != null)
                        {
                            bool exists = false;
                            if (elementID == ElementId.InvalidElementId)
                                exists = true;

                            if (!exists)
                            {
                                if (document == null)
                                    exists = true;
                                else
                                {
                                    Element element = document.GetElement(elementID);
                                    exists = element != null;
                                }

                            }

                            if (exists)
                                return parameter.Set(elementID);
                        }
                        break;
                    }
                case StorageType.Integer:
                    {
                        if (value is int)
                            return parameter.Set((int)value);
                        else if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal)
                            parameter.Set(System.Convert.ToInt32(value));
                        else if (value is bool)
                        {
                            if ((bool)value)
                                return parameter.Set(1);
                            else
                                return parameter.Set(0);
                        }
                        else if (value is string)
                        {
                            string valueString = (string)value;
                            int num = 0;
                            if (int.TryParse(valueString, out num))
                                return parameter.Set(num);

                            if (parameter.HasValue && parameter.Definition.ParameterType == ParameterType.Invalid)
                            {
                                string val = parameter.AsValueString();
                                if (val == valueString)
                                    break;

                                int current = parameter.AsInteger();
                                int k = 0;

                                string before = null;
                                while (before != val)
                                {
                                    if (k == current)
                                    {
                                        k++;
                                        continue;
                                    }

                                    try
                                    {
                                        before = val;

                                        parameter.Set(k);
                                        val = parameter.AsValueString();
                                        if (val == valueString)
                                            return true;

                                        k++;
                                    }
                                    catch
                                    {
                                        break;
                                    }
                                }

                                parameter.Set(current);
                            }
                        }
                        break;
                    }
                case StorageType.String:
                    {
                        if (value == null)
                            return parameter.Set(value as string);
                        else if (value is string)
                            return parameter.Set((string)value);
                        else
                            return parameter.Set(value.ToString());
                    }
            }

            return false;
        }
        
        /***************************************************/

        public static void SetParameters(this Element element, IBHoMObject bHoMObject, IEnumerable<BuiltInParameter> builtInParametersIgnore = null)
        {
            if (bHoMObject == null || element == null)
                return;

            foreach (KeyValuePair<string, object> kvp in bHoMObject.CustomData)
            {
                IList<Parameter> parameters = element.GetParameters(kvp.Key);
                if (parameters == null || parameters.Count == 0)
                    continue;

                foreach (Parameter parameter in parameters)
                {
                    if (parameter == null || parameter.IsReadOnly)
                        continue;

                    if (builtInParametersIgnore != null && parameter.Id.IntegerValue < 0 && builtInParametersIgnore.Contains((BuiltInParameter)parameter.Id.IntegerValue))
                        continue;

                    SetParameter(parameter, kvp.Value, element.Document);
                }
            }
        }

        /***************************************************/
    }
}
