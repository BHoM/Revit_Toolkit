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

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects.\nThis relies on Revit's `UniqueId`: the objects must have been pulled from a Revit_Adapter (they must own a `RevitIdentifiers` fragment).")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, IEnumerable<string> propertiesOrParamsToConsider = null)
        {
            RevitComparisonConfig rcc = new RevitComparisonConfig()
            {
                PropertiesToConsider = propertiesOrParamsToConsider?.ToList(),
                ParametersToConsider = propertiesOrParamsToConsider?.ToList()
            };

            return Diffing(pastObjects, followingObjects, null, new DiffingConfig() { ComparisonConfig = rcc });
        }

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects.\nThis relies on Revit's `UniqueId`: the objects must have been pulled from a Revit_Adapter (they must own a `RevitIdentifiers` fragment)")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
                "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, IEnumerable<string> propertiesToConsider = null, IEnumerable<string> parametersToConsider = null)
        {
            RevitComparisonConfig rcc = new RevitComparisonConfig()
            {
                PropertiesToConsider = propertiesToConsider?.ToList(),
                ParametersToConsider = parametersToConsider?.ToList()
            };

            return Diffing(pastObjects, followingObjects, null, new DiffingConfig() { ComparisonConfig = rcc });
        }

        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects.\nThis relies on Revit's identifiers: the objects must have been pulled from a Revit_Adapter (they must own a `RevitIdentifiers` fragment).\nBy default, Revit's `UniqueId` is used, but an option allows to select `ElementId`.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", IEnumerable<string> propertiesOrParamsToConsider = null)
        {
            RevitComparisonConfig rcc = new RevitComparisonConfig()
            {
                PropertiesToConsider = propertiesOrParamsToConsider?.ToList(),
                ParametersToConsider = propertiesOrParamsToConsider?.ToList()
            };

            return Diffing(pastObjects, followingObjects, revitIdName, new DiffingConfig() { ComparisonConfig = rcc });
        }

        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects.\nThis relies on Revit's identifiers: the objects must have been pulled from a Revit_Adapter (they must own a `RevitIdentifiers` fragment).\nBy default, Revit's `UniqueId` is used, but an option allows to select `ElementId`.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
            "\nYou can specify also Revit parameter names.")]
        [Input("diffConfig", "Further Diffing configurations.")]
        [Output("Diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        public static Diff RevitDiffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", DiffingConfig diffConfig = null)
        {
            return Diffing(pastObjects, followingObjects, revitIdName, diffConfig);
        }

        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        [Description("Performs a Revit-specialized Diffing to find the differences between two sets of objects.\nThis relies on Revit's identifiers: the objects must have been pulled from a Revit_Adapter (they must own a `RevitIdentifiers` fragment).\nBy default, Revit's `UniqueId` is used, but an option allows to select `ElementId`.")]
        [Input("pastObjects", "Past objects. Objects whose creation precedes 'followingObjects'.")]
        [Input("followingObjects", "Following objects. Objects that were created after 'pastObjects'.")]
        [Input("revitIdName", "(Optional) Defaults to UniqueId. Name of the Revit ID that will be used to perform the diffing, and recognize what objects were modified. Appropriate choices are `ElementId`, `UniqueId` or `PersistentId` (which is BHoM's equivalent to Revit's UniqueId). For more information, see Revit documentation to see how Revit Ids work.")]
        [Input("propertiesOrParamsToConsider", "Object properties to be considered when comparing two objects for differences. If null or empty, all properties will be considered." +
        "\nYou can specify also Revit parameter names.")]
        [Input("diffConfig", "Further Diffing configurations.")]
        [Output("diff", "Holds the differences between the two sets of objects. Explode it to see all differences.")]
        private static Diff Diffing(IEnumerable<object> pastObjects, IEnumerable<object> followingObjects, string revitIdName = "UniqueId", DiffingConfig diffConfig = null)
        {
            // Checks and setup of input objects.
            if (pastObjects == null) pastObjects = new List<object>();
            if (followingObjects == null) followingObjects = new List<object>();

            // Checks and setup of revitIdName.
            if (revitIdName == "UniqueId" || revitIdName == nameof(RevitIdentifiers.PersistentId) || revitIdName.IsNullOrEmpty())
                revitIdName = nameof(RevitIdentifiers.PersistentId);
            else if (revitIdName != "ElementId")
            {
                BH.Engine.Reflection.Compute.RecordError($"The input parameter {nameof(revitIdName)} can only be 'ElementId', 'UniqueId' or '{nameof(RevitIdentifiers.PersistentId)}' (BHoM's equivalent of Revit's UniqueId), but '{revitIdName}' was specified.");
                return null;
            }

            // Checks and setup of DiffingConfig/ComparisonConfig.
            DiffingConfig diffConfigClone = diffConfig == null ? new DiffingConfig() { IncludeUnchangedObjects = true } : diffConfig.DeepClone();
            RevitComparisonConfig rcc = diffConfigClone.ComparisonConfig as RevitComparisonConfig;
            if (rcc == null)
            {
                rcc = new RevitComparisonConfig();
                BH.Engine.Reflection.Modify.CopyPropertiesFromParent(rcc, diffConfigClone.ComparisonConfig);
            }
            diffConfigClone.ComparisonConfig = rcc;

            // Check if input objects all have RevitIdentifiers assigned.
            IEnumerable<IBHoMObject> revitBHoMObjects_past = pastObjects.OfType<IBHoMObject>().Where(obj => obj.GetRevitIdentifiers() != null);
            IEnumerable<IBHoMObject> revitBHoMObjects_following = followingObjects.OfType<IBHoMObject>().Where(obj => obj.GetRevitIdentifiers() != null);

            if (revitBHoMObjects_past.Count() != pastObjects.Count())
            {
                BH.Engine.Reflection.Compute.RecordError($"Some of the {nameof(pastObjects)} do not have a {nameof(RevitIdentifiers)} fragment attached.");
                return null;
            }

            if (revitBHoMObjects_following.Count() != followingObjects.Count())
            {
                BH.Engine.Reflection.Compute.RecordError($"Some of the {nameof(followingObjects)} do not have a {nameof(RevitIdentifiers)} fragment attached.");
                return null;
            }

            // Compute the diffing through DiffWithFragmentId(), which allows us to specify a Fragment and FragmentIdProperty where to find an ID to fragment by.
            Diff revitDiff = BH.Engine.Diffing.Compute.DiffWithFragmentId(revitBHoMObjects_past, revitBHoMObjects_following, typeof(RevitIdentifiers), revitIdName, diffConfigClone);

            return revitDiff;
        }
    }
}
