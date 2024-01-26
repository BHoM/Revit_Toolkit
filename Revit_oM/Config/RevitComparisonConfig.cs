/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Settings to determine the uniqueness of an Object for comparison (e.g. when Diffing or computing an object Hash).")]
    public class RevitComparisonConfig : BaseComparisonConfig
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        [Description("Names of any Revit Parameter that should not be considered for the comparison.")]
        public virtual HashSet<string> ParametersExceptions { get; set; } = new HashSet<string>() { "Edited by" };

        [Description("Names of the Revit Parameters that will be considered for the comparison." +
            "By default, this list is empty, so all parameters are considered (except possibly those included in the other property `ParametersExceptions`)." +
            "If this list is populated with one or more values, it takes higher priority over `ParametersExceptions`.")]
        public virtual HashSet<string> ParametersToConsider { get; set; } = new HashSet<string>() { };

        [Description("(Defaults to `true`) If false, if an object gets a new RevitParameter with a non-null Value added to it, then the owner object is NOT considered 'Modified' and the Comparison will NOT return this difference.")]
        public virtual bool RevitParams_ConsiderAddedAssigned { get; set; } = true;

        [Description("(Defaults to `true`) If false, if an object gets a new RevitParameter with a null Value added to it, then the owner object is NOT considered 'Modified' and the Comparison will NOT return this difference.")]
        public virtual bool RevitParams_ConsiderAddedUnassigned { get; set; } = true;

        [Description("(Defaults to `true`) If false, if an object has a RevitParameter with a non-null Value deleted from it, then the owner object is NOT considered 'Modified' and the Comparison will NOT return this difference.")]
        public virtual bool RevitParams_ConsiderRemovedAssigned { get; set; } = true;

        [Description("(Defaults to `true`) If false, if an object has a RevitParameter with a null Value deleted from it, then the owner object is NOT considered 'Modified' and the Comparison will NOT return this difference.")]
        public virtual bool RevitParams_ConsiderRemovedUnassigned { get; set; } = true;

        [Description("Numeric approximation configurations for numerical property and revit parameter values." +
                     "\nThe default Numeric Tolerance for Revit is 1e-6; a rounding is applied to numbers smaller than this." +
                     "\nYou can always change this by modifying `NumericalApproximationConfig.NumericTolerance`." +
                     "\nAlso, you override this Numeric Tolerance on a per-property basis by using `NumericalApproximationConfig.PropertyNumericTolerances`." +
                     "\nIf conflicting values/multiple matches are found among the Configurations on numerical precision, the largest approximation among all (least precise number) is registered.")]
        public override NumericalApproximationConfig NumericalApproximationConfig { get; set; } = new RevitNumericalApproximationConfig() { NumericTolerance = 1e-6 };

        /***************************************************/
    }
}




