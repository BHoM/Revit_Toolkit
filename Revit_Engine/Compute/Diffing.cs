/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Diffing;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'currentObjects'.")]
        [Input("currentObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "Defaults to `ElementId`. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId` or `UniqueId`. For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff Diffing(IEnumerable<IBHoMObject> pastObjects, IEnumerable<IBHoMObject> currentObjects, string revitIdName = "ElementId", HashSet<string> propertiesToConsider = null, DiffingConfig diffConfig = null)
        {
            // Set configurations if diffConfig is null. Clone it for immutability in the UI.
            DiffingConfig diffingConfigClone = diffConfig == null ? new DiffingConfig() { IncludeUnchangedObjects = true } : diffConfig.DeepClone();

            if (propertiesToConsider != null)
                diffingConfigClone.ComparisonConfig.PropertiesToConsider.AddRange(propertiesToConsider);
            diffingConfigClone.ComparisonConfig.PropertiesToConsider = diffingConfigClone.ComparisonConfig.PropertiesToConsider.Distinct().ToList();
            diffingConfigClone.ComparisonConfig.TypeExceptions.Add(typeof(IRevitParameterFragment));
         
            // // - Specify a propertyFullName modifier, so that Revit Parameters are considered as object declared properties.
            diffingConfigClone.ComparisonConfig.ComparisonFunctions = new ComparisonFunctions() { PropertyFullNameModifier = PropertyFullNameModifier_RevitParameterNameAsPropertyName };

            // // - Compute the diffing using DiffWithFragmentId. 
            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(pastObjects, currentObjects, typeof(RevitIdentifiers), revitIdName, diffingConfigClone);

            return diff;
        }

        /***************************************************/
        /****             Private Methods               ****/
        /***************************************************/

        // This method is to be passed to the DiffingConfig.ComparisonConfig.ComparisonFunctions (as Func delegate).
        // It will ensure that Revit Parameters are treated as main properties of an object,
        // e.g. we can specify as exception to the diffing `someRevitObj.SomeParameter`, as like `SomeParameter` was a property of the object (when instead it's stored on a RevitParameter fragment).
        private static string PropertyFullNameModifier_RevitParameterNameAsPropertyName(string propertyFullName, object obj)
        {
            // If the object is not a BHoMObject, just return the propertyFullName as-is.
            IBHoMObject bHoMObject = obj as IBHoMObject;
            if (bHoMObject == null)
                return propertyFullName;

            // Check if the propertyFullName is identifying a Revit Parameter: e.g. SomeObject.Fragments[0].Parameters[0].Value
            if (!(propertyFullName.Contains("Fragments") && propertyFullName.Contains(nameof(IRevitParameterFragment.Parameters)) && propertyFullName.Contains(nameof(RevitParameter.Value))))
                return propertyFullName;

            // Get the parameter name, and modify the input propertyFullName as if the parameter was a declared property of the object.
            string parameterName = BH.Engine.Reflection.Query.PropertyValue(obj, "Name").ToString();
            string modifiedPropertyFullName = propertyFullName.Split(new string[] { "Fragments" }, StringSplitOptions.None).First() + parameterName;

            return modifiedPropertyFullName;
        }

        /***************************************************/

        // Alternative Diffing approach where the RevitParameters are stored in CustomData. Not currently used.
        private static Diff Diffing_RevitParamsCopiedInCustomData(IEnumerable<IBHoMObject> pastObjects, IEnumerable<IBHoMObject> currentObjects, string revitIdName, HashSet<string> propertiesToConsider, DiffingConfig diffingConfigClone)
        {

            // - Alternative solution that copies the RevitParameters in the object CustomData.
            string parameterKeyPrefix = "RevitParameter_";

            // - Copy objects to ensure immutability in UI, because we are going to modify their CustomData.
            List<IBHoMObject> pastObjs = pastObjects.DeepClone().ToList();
            List<IBHoMObject> currentObjs = currentObjects.DeepClone().ToList();

            List<string> pastObjsParamNames = pastObjs.SelectMany(o => CopyRevitParametersToCustomData(o, parameterKeyPrefix)).ToList();
            List<string> currentObjsParamNames = currentObjs.SelectMany(o => CopyRevitParametersToCustomData(o, parameterKeyPrefix)).ToList();

            List<string> parametersToConsider_pastObjs = propertiesToConsider.Intersect(pastObjsParamNames).ToList();
            List<string> parametersToConsider_currentObjs = propertiesToConsider.Intersect(currentObjsParamNames).ToList();

            diffingConfigClone.ComparisonConfig.CustomdataKeysToInclude.AddRange(parametersToConsider_pastObjs.Select(ptc => parameterKeyPrefix + ptc));
            diffingConfigClone.ComparisonConfig.CustomdataKeysToInclude.AddRange(parametersToConsider_currentObjs.Select(ptc => parameterKeyPrefix + ptc));

            Diff diff = BH.Engine.Diffing.Compute.DiffWithFragmentId(pastObjs, currentObjs, typeof(RevitIdentifiers), revitIdName, diffingConfigClone);

            return RemoveAllParametersFromCustomData(diff, parameterKeyPrefix);
        }

        /***************************************************/

        // Not currently used. Only called from Diffing_RevitParamsCopiedInCustomData, that is not used.
        private static Diff RemoveAllParametersFromCustomData(Diff diff, string parameterKeyPrefix)
        {
            return new Diff(
                diff.AddedObjects.Select(o => RemoveRevitParametersFromCustomData((IBHoMObject)o, parameterKeyPrefix)),
                diff.RemovedObjects.Select(o => RemoveRevitParametersFromCustomData((IBHoMObject)o, parameterKeyPrefix)),
                diff.ModifiedObjects.Select(o => RemoveRevitParametersFromCustomData((IBHoMObject)o, parameterKeyPrefix)),
                diff.DiffingConfig,
                diff.ModifiedPropsPerObject,
                diff.UnchangedObjects.Select(o => RemoveRevitParametersFromCustomData((IBHoMObject)o, parameterKeyPrefix))
                );
        }

        /***************************************************/

        // Not currently used. Only called from Diffing_RevitParamsCopiedInCustomData, that is not used.
        private static List<string> CopyRevitParametersToCustomData(IBHoMObject o, string parameterKeyPrefix)
        {
            var parameters = o.GetRevitParameters();

            parameters.ForEach(param => o.CustomData[parameterKeyPrefix + param.Name] = param.Value);

            return parameters.Select(param => param.Name).ToList();
        }

        /***************************************************/

        // Not currently used. Only called from Diffing_RevitParamsCopiedInCustomData, that is not used.
        private static IBHoMObject RemoveRevitParametersFromCustomData(IBHoMObject o, string parameterKeyPrefix)
        {
            if (o == null)
                return null;

            o.CustomData = o.CustomData.Where(kv => !kv.Key.StartsWith(parameterKeyPrefix)).ToDictionary(kv => kv.Key, kv => kv.Value);

            return o;
        }
    }
}
