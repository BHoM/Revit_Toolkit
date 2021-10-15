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
using BH.Engine.Diffing;
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
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, IEnumerable<string> propertiesOrParamsToConsider = null)
        {
            return BH.Engine.Diffing.Compute.IDiffing(pastObjects, followingObjects, DiffingType.Automatic, new DiffingConfig() { ComparisonConfig = new RevitComparisonConfig() { PropertiesToConsider = propertiesOrParamsToConsider?.ToList(), ParametersToConsider = propertiesOrParamsToConsider?.ToList() } });
        }

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
                "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, IEnumerable<string> propertiesToConsider = null, IEnumerable<string> parametersToConsider = null)
        {
            return BH.Engine.Diffing.Compute.IDiffing(pastObjects, followingObjects, DiffingType.Automatic, new DiffingConfig() { ComparisonConfig = new RevitComparisonConfig() { PropertiesToConsider = propertiesToConsider?.ToList(), ParametersToConsider = parametersToConsider?.ToList() } });
        }

        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null)
        {
            return Diffing(pastObjects, followingObjects, revitIdName, propertiesOrParamsToConsider, null);
        }

        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Input("diffConfig", "Further Diffing configurations.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null, DiffingConfig diffConfig = null)
        {
            return Diffing(pastObjects, followingObjects, revitIdName, propertiesOrParamsToConsider, diffConfig);
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects. This relies on the ID assigned to the objects by Revit: the objects should have been pulled from a Revit_Adapter. An option below allows to use either Revit's ElementId or UniqueId.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Input("diffConfig", "Further Diffing configurations.")]
        [Output("diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        private static Diff Diffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null, DiffingConfig diffConfig = null)
        {
            // This method can be called from BH.Engine.Diffing.IDiffing(), depending on the workflow. For this reason, better to return a Note whenever it is called.
            BH.Engine.Reflection.Compute.RecordNote("Computing the revit-specific Diffing."); 

            // Checks and setup of input objects.
            if (pastObjects == null)
                pastObjects = new List<object>();

            if (followingObjects == null)
                followingObjects = new List<object>();

            // Checks and setup of revitIdName.
            if (revitIdName == "UniqueId" || revitIdName == nameof(RevitIdentifiers.PersistentId))
                revitIdName = nameof(RevitIdentifiers.PersistentId);
            else if (revitIdName != "ElementId")
            {
                BH.Engine.Reflection.Compute.RecordError($"The input parameter {nameof(revitIdName)} can only be 'ElementId', 'UniqueId' or '{nameof(RevitIdentifiers.PersistentId)}' (BHoM's equivalent of Revit's UniqueId), but '{revitIdName}' was specified.");
                return null;
            }

            // Checks and setup of DiffingConfig/ComparisonConfig.
            DiffingConfig diffConfigClone = diffConfig == null ? new DiffingConfig() { IncludeUnchangedObjects = true } : diffConfig.DeepClone();
            RevitComparisonConfig rcc = new RevitComparisonConfig();
            if (diffConfigClone.ComparisonConfig is RevitComparisonConfig)
            {
                // If the user specified a RevitComparisonConfig, just take that.
                rcc = diffConfigClone.ComparisonConfig as RevitComparisonConfig;
            }
            else if (diffConfigClone.ComparisonConfig is ComparisonConfig)
            {
                // Take all the properties from the base ComparisonConfig and copy them to the RevitComparisonConfig.
                BH.Engine.Reflection.Modify.CopyPropertiesFromParent(rcc, diffConfigClone.ComparisonConfig);
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError($"Invalid {nameof(DiffingConfig)}.{nameof(DiffingConfig.ComparisonConfig)} was specified.");
                return null;
            }
            
            // Add all propertiesOrParamsToConsider to the PropertiesToConsider list.
            rcc.PropertiesToConsider = rcc.PropertiesToConsider ?? new List<string>();
            rcc.PropertiesToConsider.AddRange(propertiesOrParamsToConsider ?? new List<string>());

            // Add all propertiesOrParamsToConsider also to the ParametersToConsider list.
            rcc.ParametersToConsider = rcc.ParametersToConsider ?? new List<string>();
            rcc.ParametersToConsider.AddRange(propertiesOrParamsToConsider ?? new List<string>());

            // Set the RevitComparisonConfig to the DiffConfig.ComparisonConfig.
            diffConfigClone.ComparisonConfig = rcc;

            // Dispatch objects in those that belong to Revit and non-Revit namespace.
            string validNamespace = "BH.oM.Adapters.Revit";

            List<IBHoMObject> revitBHoMObjects_past = pastObjects.OfType<IBHoMObject>().Where(obj => obj.GetType().FullName.StartsWith(validNamespace)).ToList();
            List<IBHoMObject> revitBHoMObjects_following = followingObjects.OfType<IBHoMObject>().Where(obj => obj.GetType().FullName.StartsWith(validNamespace)).ToList();
            List<object> remainingObjs_past = pastObjects.Except(revitBHoMObjects_past).ToList();
            List<object> remainingObjs_following = followingObjects.Except(revitBHoMObjects_following).ToList();

            // Do something for the objects that are non-Revit.
            if (remainingObjs_past.Any() || remainingObjs_following.Any())
            {
                // Returning an error for now for clarity. We deliberately restrict this method to work only on revit objects.
                BH.Engine.Reflection.Compute.RecordError($"Please specify Revit-only objects (from the namespace {validNamespace}), or use any RevitDiffing method that does not take `{nameof(revitIdName)}` as an input.");
                return null;
            }

            // Compute the diffing through DiffWithFragmentId().
            Diff revitDiff = BH.Engine.Diffing.Compute.DiffWithFragmentId(revitBHoMObjects_past, revitBHoMObjects_following, typeof(RevitIdentifiers), revitIdName, diffConfigClone);

            return revitDiff;
        }
    }
}
