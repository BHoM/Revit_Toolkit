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
        public static List<IPropertyDifference> RevitPulledParametersDifferences(this object obj1, object obj2, BaseComparisonConfig comparisonConfig)
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
        private static List<IPropertyDifference> RevitParametersDifferences<T>(this object obj1, object obj2, BaseComparisonConfig comparisonConfig) where T : class, IRevitParameterFragment
        {
            List<IPropertyDifference> result = new List<IPropertyDifference>();

            var allParamsDict_obj1 = new Dictionary<string, Dictionary<string, RevitParameter>>();
            var allParamsDict_obj2 = new Dictionary<string, Dictionary<string, RevitParameter>>();

            (obj1 as IBHoMObject)?.ParametersDictionary<T>(ref allParamsDict_obj1);
            (obj2 as IBHoMObject)?.ParametersDictionary<T>(ref allParamsDict_obj2);

            if ((!allParamsDict_obj1?.Any() ?? true) || (!allParamsDict_obj2?.Any() ?? true))
                return result; // no pulled parameters found.

            result = ParameterDifferences(allParamsDict_obj1, allParamsDict_obj2, comparisonConfig);

            return result;
        }

        /***************************************************/

        [Description("Returns a Dictionary whose Key is the nesting level of the object owning RevitParameters. " +
            "The Value is another Dictionary whose Key is the full property path of the parameter (the FullName of the property holding the parameter), " +
            "and whose Value is the RevitParameter." +
            "This kind of grouping is needed to perform correct Diffing for RevitParameters.")]
        private static void ParametersDictionary<T>(this IBHoMObject obj, ref Dictionary<string, Dictionary<string, RevitParameter>> result, string nestingPath = "") where T : class, IRevitParameterFragment
        {
            string fullPath = string.IsNullOrWhiteSpace(nestingPath) ? obj.GetType().FullName : nestingPath;
            result[fullPath] = new Dictionary<string, RevitParameter>();

            for (int i = 0; i < obj.Fragments.Count(); i++)
            {
                T revitParams = obj.Fragments[i] as T;
                if (revitParams != null)
                {
                    for (int j = 0; j < revitParams.Parameters.Count; j++)
                    {
                        // The key is the full property path of the parameter (the FullName of the property holding the parameter).
                        string parameterFullPath = $"{fullPath}.Fragments[{i}].Parameters[{j}]";
                        result[fullPath][parameterFullPath] = revitParams.Parameters[j];
                    }
                }
            }

            var bhomObjProperties = obj.GetType().GetProperties().Where(p => typeof(IBHoMObject).IsAssignableFrom(p.PropertyType));
            if (!bhomObjProperties?.Any() ?? true)
                return;

            foreach (var prop in bhomObjProperties)
            {
                ParametersDictionary<T>((IBHoMObject)prop.GetValue(obj), ref result, $"{fullPath}.{prop.Name}");
            }
        }

        /***************************************************/

        [Description("Given two input sets of parameters, finds those with the same Name between the two sets, and returns a List of PropertyDifferences containing any difference found.")]
        private static List<IPropertyDifference> ParameterDifferences(
            Dictionary<string, Dictionary<string, RevitParameter>> allParamsDict_obj1, 
            Dictionary<string, Dictionary<string, RevitParameter>> allParamsDict_obj2, 
            BaseComparisonConfig comparisonConfig)
        {
            List<IPropertyDifference> result = new List<IPropertyDifference>();

            // Check if we have a RevitComparisonConfig input.
            RevitComparisonConfig rcc = comparisonConfig as RevitComparisonConfig;

            // Find the overlapping nesting levels, i.e. those object property levels where RevitParameters were found, and which are common between obj1 and obj2.
            // We look for differences in Parameters only in those. If there is a difference in nesting levels between obj1 and obj2,
            // the difference is picked as an object property difference, which is higher-importance than RevitParameters.
            var overlappingNestingLevels = allParamsDict_obj1.Keys.Intersect(allParamsDict_obj2.Keys);
            foreach (var overlappingNestingLevel in overlappingNestingLevels)
            {
                List<string> overlappingParams = allParamsDict_obj1[overlappingNestingLevel].Values.Select(p => p.Name)
                    .Intersect(allParamsDict_obj2[overlappingNestingLevel].Values.Select(p => p.Name)).ToList();

                // Check if parameters with the same name have different values between obj1 and obj2.
                foreach (string paramName in overlappingParams)
                {
                    KeyValuePair<string, RevitParameter> overlappingParamTuple_1 = allParamsDict_obj1[overlappingNestingLevel].First(p => p.Value.Name == paramName);
                    KeyValuePair<string, RevitParameter> overlappingParamTuple_2 = allParamsDict_obj2[overlappingNestingLevel].First(p => p.Value.Name == paramName);

                    RevitParameter parameter1 = overlappingParamTuple_1.Value;
                    RevitParameter parameter2 = overlappingParamTuple_2.Value;
                    string differenceFullName = overlappingParamTuple_2.Key;

                    if (parameter1.Value?.Equals(parameter2?.Value) ?? false)
                        continue; // parameters are equal in value. Continue.

                    // Check if effectively the parameter difference is to be considered.
                    if (rcc != null && !Query.ComparisonInclusion(parameter1, parameter2, differenceFullName, rcc).Include)
                        continue;

                    // If we got here, the two parameters are different, and we want to record this difference.
                    string description = $"A Revit Parameter with name `{paramName}` was modified on the object. It had value: `{parameter1.Value}` and was updated to value: `{parameter2.Value}`";
                    result.AddParameterDifference($"{paramName} (RevitParameter)", differenceFullName, parameter1.Value, parameter2.Value, DifferenceType.Modified, description);
                }


                // By default, check if there is any deleted parameter (parameters that obj1 has and obj2 has not).
                if (rcc == null || rcc.ConsiderRemovedParameters)
                {
                    // Find deleted parameters
                    List<string> deletedParameters = allParamsDict_obj1[overlappingNestingLevel].Values.Select(p => p.Name)
                        .Except(allParamsDict_obj2[overlappingNestingLevel].Values.Select(p => p.Name)).ToList();

                    foreach (string deletedParamName in deletedParameters)
                    {
                        KeyValuePair<string, RevitParameter> deletedParamTuple = allParamsDict_obj1[overlappingNestingLevel].First(p => p.Value.Name == deletedParamName);
                        string differenceFullName = deletedParamTuple.Key;
                        RevitParameter deletedParameter = deletedParamTuple.Value;

                        // Check if effectively the parameter difference is to be considered.
                        if (rcc != null && !Query.ComparisonInclusion(deletedParameter, null, differenceFullName, rcc).Include)
                            continue;

                        // Check if we want to consider null parameters.
                        if (rcc != null && !rcc.ConsiderUnassignedParameters && deletedParameter.Value == null)
                            continue;

                        // If we got here, the two parameters are different, and we want to record this difference.
                        string description = $"A Revit Parameter with name `{deletedParamName}` was removed from the object. It had value: {deletedParameter.Value}";
                        result.AddParameterDifference($"{deletedParamName} (RevitParameter)", differenceFullName, deletedParameter.Value, null, DifferenceType.Removed, description);
                    }
                }

                // By default, check if there is any added parameter (parameters that obj2 has and obj1 has not).
                if (rcc == null || rcc.ConsiderAddedParameters)
                {
                    // Find added parameters
                    List<string> addedParameters = allParamsDict_obj2[overlappingNestingLevel].Values.Select(p => p.Name)
                        .Except(allParamsDict_obj1[overlappingNestingLevel].Values.Select(p => p.Name)).ToList();

                    foreach (string addedParamName in addedParameters)
                    {
                        KeyValuePair<string, RevitParameter> addedParamTuple = allParamsDict_obj2[overlappingNestingLevel].First(p => p.Value.Name == addedParamName);
                        string differenceFullName = addedParamTuple.Key;
                        RevitParameter addedParameter = addedParamTuple.Value;

                        // Check if effectively the parameter difference is to be considered.
                        if (rcc != null && !Query.ComparisonInclusion(null, addedParameter, differenceFullName, rcc).Include)
                            continue;

                        // Check if we want to consider null parameters.
                        if (rcc != null && !rcc.ConsiderUnassignedParameters && addedParameter.Value == null)
                            continue;

                        // If we got here, the two parameters are different, and we want to record this difference.
                        string description = $"A Revit Parameter `{addedParamName}` with value `{addedParameter.Value}` was added to the object.";
                        result.AddParameterDifference($"{addedParamName} (RevitParameter)", differenceFullName, null, addedParameter.Value, DifferenceType.Added, description);
                    }
                }
            }

            return result;
        }

        /***************************************************/

        private static void AddParameterDifference(this List<IPropertyDifference> objectDiff, string displayName, string propertyFullName, object pastValue, object followingValue, DifferenceType diffType, string description = null)
        {
            RevitParameterDifference parameterDiff = new RevitParameterDifference();
            parameterDiff.Name = displayName;
            parameterDiff.Description = description;
            parameterDiff.FullName = propertyFullName; // The FullName is important to track where exactly is the RevitParameter coming from in the Fragments list.
            parameterDiff.PastValue = pastValue;
            parameterDiff.FollowingValue = followingValue;
            parameterDiff.DifferenceType = diffType;

            objectDiff.Add(parameterDiff);
        }
    }
}



