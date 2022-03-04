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
        public virtual List<string> ParametersExceptions { get; set; } = new List<string>() { };

        [Description("Names of the Revit Parameters that will be considered for the comparison." +
            "By default, this list is empty, so all parameters are considered (except possibly those included in the other property `ParametersExceptions`)." +
            "If this list is populated with one or more values, it takes higher priority over `ParametersExceptions`.")]
        public virtual List<string> ParametersToConsider { get; set; } = new List<string>() { };

        [Description("Tolerance used for individual RevitParameters. When computing Hash or the property Diffing, if the analysed property name is found in this collection, the corresponding tolerance is applied." +
            "\nSupports * wildcard in the property name matching. E.g. `StartNode.Point.*, 2`." +
            "\nIf a match is found, this take precedence over the global `NumericTolerance`." +
            "\nIf conflicting values/multiple matches are found among the Configurations on numerical precision, the largest approximation among all (least precise number) is registered.")]
        public virtual HashSet<NamedNumericTolerance> ParameterNumericTolerances { get; set; } = new HashSet<NamedNumericTolerance>();

        [Description("Number of significant figures allowed for numerical data on a per-RevitParameter base. " +
             "\nSupports * wildcard in the property name matching. E.g. `StartNode.Point.*, 2`." +
             "\nIf a match is found, this take precedence over the global `SignificantFigures`." +
             "\nIf conflicting values/multiple matches are found among the Configurations on numerical precision, the largest approximation among all (least precise number) is registered.")]
        public virtual HashSet<NamedSignificantFigures> ParameterSignificantFigures { get; set; } = new HashSet<NamedSignificantFigures>();

        [Description("If true, if an object gets a new RevitParameter added to it, it will be considered 'Modified' and the Comparison will return this difference. " +
            "Defaults to true.")]
        public virtual bool ConsiderAddedParameters { get; set; } = true;

        [Description("If true, if an object has a RevitParameter deleted from it, it will be considered 'Modified' and the Comparison will return this difference. " +
            "Defaults to true.")]
        public virtual bool ConsiderDeletedParameters { get; set; } = true;

        [Description("If true, considers all differences including parameters that are null (Unassigned). Defaults to true. If false:\n" +
            "- a RevitParameter that was null in the past object and is deleted in the following object is not considered as a difference.\n" +
            "- a RevitParameter that is null in the following object and was not present in the pastobject is not considered as a difference.")]
        public virtual bool ConsiderUnassignedParameters { get; set; } = true;

        /***************************************************/
    }
}


