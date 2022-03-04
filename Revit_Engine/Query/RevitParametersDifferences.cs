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

using BH.Engine.Base;
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Diffing;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns Property differences between RevitParameters (RevitPulledParameters/RevitParametersToPush) owned by the two input objects." +
            "This method can be added as Func delegate to the DiffingConfig; if so, it is automatically triggered when Diffing the two objects.")]
        [Input("obj1", "Past object being compared.")]
        [Input("obj2", "Following object being compared.")]
        [Input("comparisonConfig", "Comparison Config to be used during comparison.")]
        [Output("parametersDifferences", "Differences in terms of RevitParametersToPush found on the two input objects.")]
        public static List<PropertyDifference> RevitParametersDifferences<T>(this object obj1, object obj2, BaseComparisonConfig comparisonConfig) where T : class, IRevitParameterFragment
        {
            List<PropertyDifference> result = new List<PropertyDifference>();

            Dictionary<string, RevitParameter> allParamsDict_obj1 = (obj1 as IBHoMObject)?.ParametersDictionary<T>();
            Dictionary<string, RevitParameter> allParamsDict_obj2 = (obj2 as IBHoMObject)?.ParametersDictionary<T>();

            if ((!allParamsDict_obj1?.Any() ?? true) || (!allParamsDict_obj2?.Any() ?? true))
                return result; // no pulled parameters found.

            result = ParameterDifferences(allParamsDict_obj1, allParamsDict_obj2, comparisonConfig);

            return result;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Returns a Dictionary whose Key is the full property path of the parameter (the FullName of the property holding the parameter), " +
            "and whose Value is the RevitParameter.")]
        private static Dictionary<string, RevitParameter> ParametersDictionary<T>(this IBHoMObject obj) where T : class, IRevitParameterFragment
        {
            Dictionary<string, RevitParameter> result = new Dictionary<string, RevitParameter>();

            if (!obj?.Fragments?.Any() ?? true)
                return result;

            string objFullName = obj.GetType().FullName;

            for (int i = 0; i < obj.Fragments.Count(); i++)
            {
                T revitParams = obj.Fragments[i] as T;
                if (revitParams != null)
                {
                    for (int j = 0; j < revitParams.Parameters.Count; j++)
                    {
                        // The key is the full property path of the parameter (the FullName of the property holding the parameter).
                        string parameterFullPath = $"{objFullName}.Fragments[{i}].Parameters[{j}]";
                        result[parameterFullPath] = revitParams.Parameters[j];
                    }
                }
            }

            return result;
        }

        /***************************************************/

        [Description("Given two input sets of parameters, finds those with the same Name between the two sets, and returns a List of PropertyDifferences containing any difference found.")]
        private static List<PropertyDifference> ParameterDifferences(Dictionary<string, RevitParameter> allParamsDict_obj1, Dictionary<string, RevitParameter> allParamsDict_obj2, BaseComparisonConfig comparisonConfig)
        {
            List<PropertyDifference> result = new List<PropertyDifference>();

            List<string> overlappingParams = allParamsDict_obj1.Values.Select(p => p.Name).Intersect(allParamsDict_obj2.Values.Select(p => p.Name)).ToList();

            foreach (string paramName in overlappingParams)
            {
                KeyValuePair<string, RevitParameter> overlappingParamTuple_1 = allParamsDict_obj1.First(p => p.Value.Name == paramName);
                KeyValuePair<string, RevitParameter> overlappingParamTuple_2 = allParamsDict_obj2.First(p => p.Value.Name == paramName);

                RevitParameter parameter1 = overlappingParamTuple_1.Value;
                RevitParameter parameter2 = overlappingParamTuple_2.Value;
                string differenceFullName = overlappingParamTuple_2.Key;

                if (parameter1.Value?.Equals(parameter2?.Value) ?? false)
                    continue; // parameters are equal in value. Continue.

                // Check if we have a RevitComparisonConfig input.
                RevitComparisonConfig rcc = comparisonConfig as RevitComparisonConfig;

                // Check if effectively the parameter difference is to be considered.
                if (!Query.ComparisonInclusion(parameter1, parameter2, differenceFullName, rcc).Include)
                    continue;

                // If we got here, the two parameters are different, and we want to record this difference.
                PropertyDifference parameterDiff = new PropertyDifference();
                parameterDiff.DisplayName = paramName + " (RevitParameter)";
                parameterDiff.FullName = differenceFullName; // The FullName is important to track where exactly is the RevitParameter coming from in the Fragments list.
                parameterDiff.PastValue = parameter1.Value;
                parameterDiff.FollowingValue = parameter2.Value;

                result.Add(parameterDiff);
            }

            return result;
        }
    }
}



