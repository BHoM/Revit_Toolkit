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
        [Input("toParameter", "RevitParameter to which the result of the formula will be set.")]
        [Input("startCalculation", "Flag to start the calculation process.")]
        [Input("applyCalculation", "Flag to apply the calculation result to the Revit parameter.")]
        [Output("bHoMObjects", "List of BHoMObjects with the applied calculation to the specified Revit parameter.")]
        public static List<object> ApplyFormula(List<IBHoMObject> elements, ParameterFormula formula, RevitParameter toParameter, bool startCalculation = false, bool applyCalculation = false)
        {
            if (elements == null || formula == null || toParameter == null)
            {
                BH.Engine.Base.Compute.RecordError("One or more input parameters are null");
                return null;
            }

            if (!startCalculation)
            {
                BH.Engine.Base.Compute.RecordError("Inputs received, waiting for calculation");
                return null;
            }

            Delegate function = CreateFormula(formula);

            List<object> results = new List<object>();

            for (int i = 0; i < elements.Count; i++)
            {
                //Check toParameter if it is a parameterType or parameterInstance
                bool? isInstanceParameter = toParameter.IsInstance;

                bool isValidInput = true;
                object[] inputs = new object[formula.InputParameters.Count];

                int j = 0;
                while (j < formula.InputParameters.Count && isValidInput)
                {
                    bool isAllInputsTypeParam = !formula.InputParameters[j].IsInstance.Value;

                    //type parameter only accepts type parameters as input
                    isValidInput &= isInstanceParameter.Value || isAllInputsTypeParam;
                    inputs[j] = elements[i].GetRevitParameterValue(formula.InputParameters[j].Name);

                    j++;
                }

                object result;

                if (!isValidInput)
                {
                    result = null;
                    BH.Engine.Base.Compute.RecordError($"Input invalid at {elements[i].ElementId()}, TypeParameter output only accepts TypeParameterInput");
                }
                else
                {
                    result = function.DynamicInvoke(inputs);
                }

                if (applyCalculation)
                {
                    var elementParameter = elements[i].GetRevitParameters().Where(p => p.Name == toParameter.Name).FirstOrDefault();
                    if (elementParameter != null && !elementParameter.IsReadOnly)
                    {
                        elements[i].SetRevitParameter(toParameter.Name, result);
                    }
                    else
                    {
                        BH.Engine.Base.Compute.RecordError($"Parameter is not found for ID: {elements[i].ElementId()}, Result: {result}");
                    }
                    results.Add(elements[i]);
                }
                else
                {
                    results.Add(result);
                }
            }
            return results;
        }



        [Description("Applies a formula to a single BHoMObject and returns the result.")]
        [Input("element", "BHoMObject to which the formula will be applied.")]
        [Input("formula", "ParameterFormula containing the formula to be applied.")]
        [Input("startCalculation", "Flag to start the calculation process.")]
        [Input("applyCalculation", "Flag to apply the calculation result to the Revit parameter.")]
        [Output("result", "Result of the applied formula.")]
        public static object ApplyFormula(IBHoMObject element, ParameterFormula formula, bool startCalculation = false, bool applyCalculation = false)
        {
            if (element == null || formula == null)
            {
                BH.Engine.Base.Compute.RecordError("One or more input parameters are null");
                return null;
            }

            if (!startCalculation)
            {
                BH.Engine.Base.Compute.RecordError("Inputs received, waiting for calculation");
                return null;
            }

            Delegate function = CreateFormula(formula);

            object[] inputs = new object[formula.InputParameters.Count];

            int i = 0;
            while (i < formula.InputParameters.Count)
            {
                bool isAllInputsTypeParam = !formula.InputParameters[i].IsInstance.Value;

                //type parameter only accepts type parameters as input
                inputs[i] = element.GetRevitParameterValue(formula.InputParameters[i].Name);

                i++;
            }

            return function.DynamicInvoke(inputs);
        }


        [Description("Applies a formula to a ParameterFormula objects with Input Parameters' values and returns the result. Just for test")]
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

            Delegate function = CreateFormula(formula);

            object[] inputs = new object[formula.InputParameters.Count];

            int i = 0;
            while (i < formula.InputParameters.Count)
            {
                bool isAllInputsTypeParam = !formula.InputParameters[i].IsInstance.Value;

                //type parameter only accepts type parameters as input
                inputs[i] = formula.InputParameters[i].Value;

                i++;
            }

            return function.DynamicInvoke(inputs);
        }

        /***************************************************/
    }
}

