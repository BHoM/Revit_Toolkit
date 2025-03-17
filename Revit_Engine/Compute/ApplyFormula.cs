/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using System.ComponentModel;
using System.Collections.Generic;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Adapters.Revit.Parameters;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/


        [Description("Applies a formula to a list of BHoMObjects and sets the result to a specified Revit parameter.")]
        [Input("elements", "List of BHoMObjects to which the formula will be applied.")]
        [Input("formula", "ParameterFormula containing the formula to be applied.")]
        [Input("toParameter", "RevitParameter to which the result of the formula will be set. Without this object, method will return result")]
        [Input("applyResult", "Flag to transfer the result to the Revit parameters")]
        [Input("alwaysCalculate", "Flag to always calculate the formula, input parameters are set to default value if not found on element.")]
        [Output("bHoMObjects", "List of BHoMObjects with the applied calculation to the specified Revit parameter.")]
        public static List<object> ApplyFormula(List<IBHoMObject> elements, ParameterFormula formula, string toParameter = null, bool startCalculation = false, bool applyResult = false, bool alwaysCalculate = false)
        {
            if (elements == null || formula == null)
            {
                BH.Engine.Base.Compute.RecordError("One or more input parameters are null");
                return null;
            }

            if (!startCalculation)
            {
                BH.Engine.Base.Compute.RecordError("Inputs received, waiting for calculation");
                return null;
            }

            Delegate function;
            function = CreateFormula(formula);

            if (function == null)
            {
                return null;
            }

            Type[] paramType = function.Method.GetParameters().Select(p => p.ParameterType).ToArray();

            object[] results = new object[elements.Count];

            for (int i = 0; i < elements.Count; i++)
            {
                object[] inputs = BuildInputsFromElement(elements[i], formula, paramType, alwaysCalculate, out bool isAllInputsTypeParam);
                if (inputs == null)
                {
                    results[i] = null;
                    continue;
                }
                results[i] = ExecuteCalculation(elements[i], applyResult, function, inputs, isAllInputsTypeParam, toParameter);
            }
            return results.ToList();
        }

        [Description("Applies a formula to a single BHoMObject and returns the result.")]
        [Input("element", "BHoMObject to which the formula will be applied.")]
        [Input("formula", "ParameterFormula containing the formula to be applied.")]
        [Input("toParameter", "RevitParameter to which the result of the formula will be set. Without this object, method will return result")]
        [Input("applyResult", "Flag to transfer the result to the Revit parameter.")]
        [Input("alwaysCalculate", "Flag to always calculate the formula, input parameters are set to default value if not found on element.")]
        [Output("result", "Result of the applied formula.")]
        public static object ApplyFormula(IBHoMObject element, ParameterFormula formula, string toParameter = null, bool applyResult = false, bool alwaysCalculate = false)
        {
            if (element == null || formula == null)
            {
                BH.Engine.Base.Compute.RecordError($"Insufficient Inputs");
                return null;
            }

            Delegate function;
            function = CreateFormula(formula);

            if (function == null)
            {
                return null;
            }

            Type[] paramType = function.Method.GetParameters().Select(p => p.ParameterType).ToArray();

            object[] inputs = BuildInputsFromElement(element, formula, paramType, alwaysCalculate, out bool isAllInputsTypeParam);
            if (inputs == null)
            {
                return null;
            }
            return ExecuteCalculation(element, applyResult, function, inputs, isAllInputsTypeParam, toParameter);
        }

        [Description("Apply a calculation to a ParameterFormula objects with Input Parameters' values and returns the result. For testing a formula")]
        [Input("formula", "ParameterFormula containing the formula to be applied.")]
        [Input("startCalculation", "Flag to start the calculation process.")]
        [Input("applyCalculation", "Flag to apply the calculation result to the Revit parameter.")]
        [Output("result", "Result of the applied formula.")]
        public static object ApplyFormula(ParameterFormula formula, bool startCalculation = false)
        {
            if (formula == null)
            {
                BH.Engine.Base.Compute.RecordError("One or more input parameters are null");
                return null;
            }

            if (!startCalculation)
            {
                BH.Engine.Base.Compute.RecordError("Inputs received, waiting for calculation");
                return null;
            }

            Delegate function;
            function = CreateFormula(formula);

            if (function == null)
            {
                return null;
            }

            Type[] paramType = function.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            int externalDataCount = formula.ExternalData != null ? formula.ExternalData.Count : 0;
            object[] inputs = new object[formula.InputParameters.Count + externalDataCount];

            int i = 0;
            while (i < formula.InputParameters.Count)
            {
                inputs[i] = formula.InputParameters[i].Value;

                i++;
            }

            if (externalDataCount > 0)
            {
                foreach (var data in formula.ExternalData)
                {
                    inputs[i] = data.Value;
                    i++;
                }
            }

            return  function.DynamicInvoke(inputs);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        /// <summary>
        ///Check inputs nullable exception and build the inputs array for the formula
        /// </summary>
        private static object[] BuildInputsFromElement(IBHoMObject element, ParameterFormula formula,Type[] paramType,bool alwaysCalculate, out bool isAllInputsTypeParam)
        {
            int externalDataCount = formula.ExternalData?.Count?? 0;
            object[] inputs = new object[formula.InputParameters.Count + externalDataCount];

            isAllInputsTypeParam = true;

            //Add the input parameters to the inputs array, if the parameter is not found in the element, set it to default value
            int i = 0;
            while (i < formula.InputParameters.Count)
            {
                isAllInputsTypeParam &= !formula.InputParameters[i].IsInstance.Value;
                RevitParameter elementParam = element.GetRevitParameter(formula.InputParameters[i].Name);
                if (elementParam == null || !alwaysCalculate)
                {
                    BH.Engine.Base.Compute.RecordError($"Parameter {formula.InputParameters[i].Name} not found in the element {element.ElementId()}");
                    return null;
                }
                inputs[i] = elementParam.Value;
                if (inputs[i] == null)
                {
                    switch (paramType[i])
                    {
                        case Type t when t == typeof(int):
                            inputs[i] = 0;
                            break;
                        case Type t when t == typeof(double):
                            inputs[i] = 0.0;
                            break;
                        case Type t when t == typeof(string):
                            inputs[i] = "";
                            break;
                        case Type t when t == typeof(bool):
                            return null;
                        default:
                            BH.Engine.Base.Compute.RecordError($"Parameter {formula.InputParameters[i].Name} is not a valid type");
                            return null;
                    }
                }
                i++;
            }

            if (externalDataCount > 0)
            {
                foreach (var data in formula.ExternalData)
                {
                    inputs[i] = data.Value;
                    i++;
                }
            }
            return inputs;
        }

        private static object ExecuteCalculation(IBHoMObject element, bool applyResult, Delegate function, object[] inputs, bool isAllInputsTypeParam, string toParameter = null)
        {
            if (applyResult)
            {
                if (toParameter == null)
                {
                    BH.Engine.Base.Compute.RecordError("No target parameter to apply");
                    return null;
                }
                RevitParameter targetParam = element.GetRevitParameter(toParameter);
                if (targetParam == null)
                {
                    BH.Engine.Base.Compute.RecordError($"Parameter {toParameter} doesn't exist in {element.ElementId()}");
                    return null;
                }
                bool isValidTransaction = !(isAllInputsTypeParam ^ !targetParam.IsInstance.Value);
                if (isValidTransaction)
                {
                    return element.SetRevitParameter(targetParam.Name, function.DynamicInvoke(inputs));
                }
                else
                {
                    BH.Engine.Base.Compute.RecordError("Input invalid, TypeParameter output only accepts TypeParameterInput");
                    return null;
                }
            }
            else
            {
                return function.DynamicInvoke(inputs);
            }
        }
    }
}