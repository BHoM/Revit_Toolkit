/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Sets a given numerical value to first parameter of a given Revit Element found under given name.")]
        [Input("element", "Revit element to be modified.")]
        [Input("parameterName", "Name of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Input("convertUnits", "If true, the input value will be converted to internal Revit units, otherwise not.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, string parameterName, double value, bool convertUnits = true)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                if (convertUnits)
                    value = value.FromSI(parameter.Definition.GetSpecTypeId());

                return parameter.Set(value);
            }

            return false;
        }

        /***************************************************/

        [Description("Sets a given numerical value to the parameter of a given Revit Element found under given identifier.")]
        [Input("element", "Revit element to be modified.")]
        [Input("builtInParam", "Identifier of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Input("convertUnits", "If true, the input value will be converted to internal Revit units, otherwise not.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, double value, bool convertUnits = true)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Double)
            {
                if (convertUnits)
                    value = value.FromSI(parameter.Definition.GetSpecTypeId());

                return parameter.Set(value);
            }

            return false;
        }

        /***************************************************/

        [Description("Sets a given integer value to first parameter of a given Revit Element found under given name.")]
        [Input("element", "Revit element to be modified.")]
        [Input("parameterName", "Name of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, string parameterName, int value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Integer)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        [Description("Sets a given integer value to the parameter of a given Revit Element found under given identifier.")]
        [Input("element", "Revit element to be modified.")]
        [Input("builtInParam", "Identifier of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, int value)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.Integer)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        [Description("Sets a given text value to first parameter of a given Revit Element found under given name.")]
        [Input("element", "Revit element to be modified.")]
        [Input("parameterName", "Name of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, string parameterName, string value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        [Description("Sets a given text value to the parameter of a given Revit Element found under given identifier.")]
        [Input("element", "Revit element to be modified.")]
        [Input("builtInParam", "Identifier of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, string value)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        [Description("Sets a given value in a form of Revit ElementId to first parameter of a given Revit Element found under given name.")]
        [Input("element", "Revit element to be modified.")]
        [Input("parameterName", "Name of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, string parameterName, ElementId value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.ElementId)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        [Description("Sets a given value in a form of Revit ElementId to the parameter of a given Revit Element found under given identifier.")]
        [Input("element", "Revit element to be modified.")]
        [Input("builtInParam", "Identifier of the parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Output("success", "True if an existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameter(this Element element, BuiltInParameter builtInParam, ElementId value)
        {
            Parameter parameter = element.get_Parameter(builtInParam);
            if (parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.ElementId)
                return parameter.Set(value);

            return false;
        }

        /***************************************************/

        [Description("Sets a given Revit Parameter with the given value.")]
        [Input("parameter", "Revit Parameter to be set.")]
        [Input("value", "Value to be set to the parameter.")]
        [Input("document", "Revit Document to be used when processing the parameter.")]
        [Output("success", "True if the input parameter got successfully set with the input value.")]
        public static bool SetParameter(this Parameter parameter, object value, Document document = null)
        {
            // Skip null and read-only parameters
            if (parameter == null || parameter.IsReadOnly)
                return false;

            // Workset parameters
            // Filter for worksets
            if (parameter.Id.IntegerValue == (int)BuiltInParameter.ELEM_PARTITION_PARAM)
            {
                // Find an existing workset with a specified name if it exists
                string worksetName = value as string;
                Workset workset = document.Workset(worksetName);

                // Set the "Workset" parameter to the specified existing workset
                // Ensure the Query method hasn't returned a null, which can happen if the document is not workshared or the workset name is empty
                if (workset != null)
                {
                    // Set the parameter to a workset with the specified name if it exists
                    return parameter.Set(workset.Id.IntegerValue);
                }
                else
                {
                    // A workset with the specified name doesn't exist
                    BH.Engine.Base.Compute.RecordWarning("Cannot set the Workset parameter because a workset with the specified name doesn't exist.");
                    return false;
                }
            }

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
                                dbl = Convert.FromSI(dbl, parameter.Definition.GetSpecTypeId());
                            }
                            catch
                            {
                                dbl = double.NaN;
                            }

                            if (!double.IsNaN(dbl))
                            {
                                if (parameter.Id.IntegerValue == (int)BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE)
                                    dbl = dbl.NormalizeAngleDomain();

                                return parameter.Set(dbl);
                            }
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
                            return parameter.Set(System.Convert.ToInt32(value));
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

                                return parameter.Set(current);
                            }
                        }
                        else if (value is System.Drawing.Color)
                        {
                            System.Drawing.Color color = (System.Drawing.Color)value;
                            return parameter.Set(color.R | color.G << 8 | color.B << 16);
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

        [Description("Sets the given value to all parameters of a given Revit Element under a given name.")]
        [Input("element", "Revit element to be modified.")]
        [Input("name", "Name of the parameters to be set.")]
        [Input("value", "Value to be set to the parameters.")]
        [Output("success", "True if at least one existing parameter has been found under the input name and got successfully set with the input value.")]
        public static bool SetParameters(this Element element, string name, object value)
        {
            bool success = false;
            foreach (Parameter param in element.Parameters)
            {
                if (param.Definition.Name == name)
                    success |= param.SetParameter(value, element.Document);
            }

            return success;
        }

        /***************************************************/

        [Description("Sets the given value to all parameters of a given Revit Element under given names.")]
        [Input("element", "Revit element to be modified.")]
        [Input("names", "Names of the parameters to be set.")]
        [Input("value", "Value to be set to the parameters.")]
        [Output("success", "True if at least one existing parameter has been found under one of the input names and got successfully set with the input value.")]
        public static bool SetParameters(this Element element, IEnumerable<string> names, object value)
        {
            bool success = false;
            foreach (Parameter param in element.Parameters)
            {
                if (names.Any(x => param.Definition.Name == x))
                    success |= param.SetParameter(value, element.Document);
            }

            return success;
        }

        /***************************************************/
    }
}


