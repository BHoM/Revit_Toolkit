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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Collections.Generic;
using BH.oM.Base;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Data.Requests;
using BH.oM.Adapters.Revit;
using System.Reflection;
using System;
using System.Linq;
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
        [Input("activate", "Perform calculation.")]
        [Output("bHoMObjects", "List of BHoMObjects with the applied calculation to the specified Revit parameter.")]
        public static List<IBHoMObject> ApplyFormula(List<IBHoMObject> elements, ParameterFormula formula, RevitParameter toParameter, bool activate = false)
        {
            Func<List<object>, object> function = GenerateMethod<List<object>, object>(formula);
            List<IBHoMObject> bHoMObjects = new List<IBHoMObject>();

            for (int i = 0; i < elements.Count; i++)
            {
                //Check toParameter if it is a parameterType or parameterInstance
                bool isInstanceParameter = toParameter.IsInstance;

                bool isValidInput = true;
                List<object> inputs = new List<object>();

                int j = 0;
                while (j < formula.InputParameters.Count && isValidInput)
                {
                    bool isTypeParameter = !formula.InputParameters[j].IsInstance;

                    //type parameter only accepts type parameters as input
                    isValidInput &= isInstanceParameter || isTypeParameter;

                    inputs.Add(elements[i].GetRevitParameterValue(formula.InputParameters[j].Name));

                    j++;
                }

                var result = (isValidInput) ? function(inputs) : BH.Engine.Base.Compute.RecordError($"Input invalid at {elements[i].ElementId()}, TypeParameter output only accepts TypeParameterInput"); ;

                if (activate)
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

        private static Func<T, TResult> GenerateMethod<T, TResult>(ParameterFormula formula)
        {
            throw new NotImplementedException();
        }
        /***************************************************/
    }
}






