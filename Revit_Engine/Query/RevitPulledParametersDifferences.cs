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
        public static List<PropertyDifference> RevitPulledParametersDifferences(this object obj1, object obj2, BaseComparisonConfig comparisonConfig)
        {
            List<PropertyDifference> result = new List<PropertyDifference>();

            Dictionary<string, RevitParameter> allParamsDict_obj1 = (obj1 as IBHoMObject)?.GetPulledParameters();
            Dictionary<string, RevitParameter> allParamsDict_obj2 = (obj2 as IBHoMObject)?.GetPulledParameters();

            if ((!allParamsDict_obj1?.Any() ?? true) || (!allParamsDict_obj2?.Any() ?? true))
                return result; // no pulled parameters found.
         
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
                parameterDiff.DisplayName = paramName + " (RevitPulledParameter)";
                parameterDiff.FullName = differenceFullName;
                parameterDiff.PastValue = parameter1.Value;
                parameterDiff.FollowingValue = parameter2.Value;

                result.Add(parameterDiff);
            }

            return result;
        }

        private static Dictionary<string, RevitParameter> GetPulledParameters(this IBHoMObject obj)
        {
            Dictionary<string, RevitParameter> result = new Dictionary<string, RevitParameter>();

            if (!obj?.Fragments?.Any() ?? true)
                return result;

            for (int i = 0; i < obj.Fragments.Count(); i++)
            {
                RevitPulledParameters pulledParams = obj.Fragments[i] as RevitPulledParameters;
                if (pulledParams != null)
                {
                    for (int j = 0; j < pulledParams.Parameters.Count; j++)
                    {
                        string parameterFullPath = $"{obj.GetType().FullName}.Fragments[{i}].Parameters[{j}]";
                        result[parameterFullPath] = pulledParams.Parameters[j];
                    }
                }
            }

            return result;
        }
    }
}



