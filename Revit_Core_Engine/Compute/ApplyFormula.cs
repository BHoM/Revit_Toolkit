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

using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Collections.Generic;
using BH.oM.Base;
using BH.oM.Adapters.Revit.Parameters;
using System;
using BH.Engine.Adapters.Revit;

namespace BH.Revit.Engine.Core
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
        public static List<IBHoMObject> ApplyFormula(List<IBHoMObject> elements, ParameterFormula formula, RevitParameter toParameter,bool startCalculation = false, bool applyCalculation = false)
        {
            if (elements == null || formula == null || toParameter == null)
            {
                BH.Engine.Base.Compute.RecordError("One or more input parameters are null");
                return null;
            }

            if (startCalculation)
            {
                BH.Engine.Base.Compute.RecordError("Inputs received, waiting for calculation");
                return null;
            }

            Func<List<object>, object> function = GenerateMethod<List<object>, object>(formula, toParameter.Value.GetType());
            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();

            for (int i = 0; i < elements.Count; i++)
            {
                //Check toParameter if it is a parameterType or parameterInstance
                bool? isInstanceParameter = toParameter.IsInstance;

                bool isValidInput = true;
                List<object> inputs = new List<object>();

                int j = 0;
                while (j < formula.InputParameters.Count && isValidInput)
                {
                    bool isAllInputsTypeParam = !formula.InputParameters[j].IsInstance.Value;

                    //type parameter only accepts type parameters as input
                    isValidInput &= isInstanceParameter.Value || isAllInputsTypeParam;

                    inputs.Add(elements[i].GetRevitParameterValue(formula.InputParameters[j].Name));

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
                    result = function(inputs);
                }

                if (applyCalculation)
                {
                    elements[i].SetRevitParameter(toParameter.Name, result);
                    bHoMObjects.Add(elements[i]);
                }
            }

            return bHoMObjects;
        }


        [Description("")]
        [Input("", "")]
        [Input("", "")]
        [Input("", "")]
        [Input("", "")]
        [Output("", "")]

        private static Func<T, TResult> GenerateMethod<T, TResult>(ParameterFormula input, System.Type ouputType)
        {
            throw new NotImplementedException();
        }
        /***************************************************/
    }
}






