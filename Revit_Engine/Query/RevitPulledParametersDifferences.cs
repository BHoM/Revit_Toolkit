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

        [Description("Returns Property differences between RevitParameters (RevitPulledParameters) owned by the two input objects." +
            "This method can be added as Func delegate to the DiffingConfig; if so, it is automatically triggered when Diffing the two objects.")]
        [Input("obj1", "Past object being compared.")]
        [Input("obj2", "Following object being compared.")]
        [Input("comparisonConfig", "Comparison Config to be used during comparison.")]
        [Output("parametersDifferences", "Differences in terms of RevitPulledParameters found on the two input objects.")]
        public static List<PropertyDifference> RevitPulledParametersDifferences(this object obj1, object obj2, BaseComparisonConfig comparisonConfig)
        {
            return RevitParametersDifferences<RevitPulledParameters>(obj1, obj2, comparisonConfig);
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        [Description("Returns Property differences between RevitParameters (RevitPulledParameters/RevitParametersToPush) owned by the two input objects." +
            "This method can be added as Func delegate to the DiffingConfig; if so, it is automatically triggered when Diffing the two objects.")]
        [Input("obj1", "Past object being compared.")]
        [Input("obj2", "Following object being compared.")]
        [Input("comparisonConfig", "Comparison Config to be used during comparison.")]
        [Output("parametersDifferences", "Differences in terms of RevitParameters (RevitPulledParameters/RevitParametersToPush) found on the two input objects.")]
        private static List<PropertyDifference> RevitParametersDifferences<T>(this object obj1, object obj2, BaseComparisonConfig comparisonConfig) where T : class, IRevitParameterFragment
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
            HashSet<PropertyDifference> result = new HashSet<PropertyDifference>();

            List<string> overlappingParams = allParamsDict_obj1.Values.Select(p => p.Name).Intersect(allParamsDict_obj2.Values.Select(p => p.Name)).ToList();

            // Check if we have a RevitComparisonConfig input.
            RevitComparisonConfig rcc = comparisonConfig as RevitComparisonConfig;

            // Check if parameters with the same name have different values between obj1 and obj2.
            foreach (string paramName in overlappingParams)
            {
                KeyValuePair<string, RevitParameter> overlappingParamTuple_1 = allParamsDict_obj1.First(p => p.Value.Name == paramName);
                KeyValuePair<string, RevitParameter> overlappingParamTuple_2 = allParamsDict_obj2.First(p => p.Value.Name == paramName);

                RevitParameter parameter1 = overlappingParamTuple_1.Value;
                RevitParameter parameter2 = overlappingParamTuple_2.Value;
                string differenceFullName = overlappingParamTuple_2.Key;

                if (parameter1.Value?.Equals(parameter2?.Value) ?? false)
                    continue; // parameters are equal in value. Continue.

                // Check if effectively the parameter difference is to be considered.
                if (rcc != null && !Query.ComparisonInclusion(parameter1, parameter2, differenceFullName, rcc).Include)
                    continue;

                // If we got here, the two parameters are different, and we want to record this difference.
                PropertyDifference parameterDiff = ParameterDifference(paramName, differenceFullName, parameter1.Value, parameter2.Value);
                result.Add(parameterDiff);
            }

            // By default, check if there is any deleted parameter (parameters that obj1 has and obj2 has not).
            if (rcc == null || rcc.ConsiderDeletedParameters)
            {
                // Find deleted parameters
                List<string> deletedParameters = allParamsDict_obj1.Values.Select(p => p.Name).Except(allParamsDict_obj2.Values.Select(p => p.Name)).ToList();

                foreach (string deletedParamName in deletedParameters)
                {
                    KeyValuePair<string, RevitParameter> deletedParamTuple = allParamsDict_obj1.First(p => p.Value.Name == deletedParamName);
                    string differenceFullName = deletedParamTuple.Key;
                    RevitParameter deletedParameter = deletedParamTuple.Value;

                    // Check if effectively the parameter difference is to be considered.
                    if (rcc != null && !Query.ComparisonInclusion(deletedParameter, null, differenceFullName, rcc).Include)
                        continue;

                    // If we got here, the two parameters are different, and we want to record this difference.
                    PropertyDifference parameterDiff = ParameterDifference(deletedParamName, differenceFullName, deletedParameter.Value, null);
                    result.Add(parameterDiff);
                }
            }

            // By default, check if there is any added parameter (parameters that obj2 has and obj1 has not).
            if (rcc == null || rcc.ConsiderAddedParameters)
            {
                // Find added parameters
                List<string> addedParameters = allParamsDict_obj2.Values.Select(p => p.Name).Except(allParamsDict_obj1.Values.Select(p => p.Name)).ToList();

                foreach (string addedParamName in addedParameters)
                {
                    KeyValuePair<string, RevitParameter> addedParamTuple = allParamsDict_obj2.First(p => p.Value.Name == addedParamName);
                    string differenceFullName = addedParamTuple.Key;
                    RevitParameter addedParameter = addedParamTuple.Value;

                    // Check if effectively the parameter difference is to be considered.
                    if (rcc != null && !Query.ComparisonInclusion(null, addedParameter, differenceFullName, rcc).Include)
                        continue;

                    // If we got here, the two parameters are different, and we want to record this difference.
                    PropertyDifference parameterDiff = ParameterDifference(addedParamName, differenceFullName, addedParameter.Value, null);
                    result.Add(parameterDiff);
                }
            }

            return result.ToList();
        }

        /***************************************************/

        private static PropertyDifference ParameterDifference(string paramName, string propertyFullName, object pastValue, object followingValue)
        {                
            PropertyDifference parameterDiff = new PropertyDifference();
            parameterDiff.DisplayName = paramName + " (RevitParameter)";
            parameterDiff.FullName = propertyFullName; // The FullName is important to track where exactly is the RevitParameter coming from in the Fragments list.
            parameterDiff.PastValue = pastValue;
            parameterDiff.FollowingValue = followingValue;
            
            return parameterDiff;
        }
    }
}



