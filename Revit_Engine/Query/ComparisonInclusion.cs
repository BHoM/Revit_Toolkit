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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Diffing;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        [Description("When Diffing (Comparing objects), determine if and how RevitParameters should be considered.")]
        [Input("parameter1", "First RevitParameter that is being compared.")]
        [Input("parameter2", "Second RevitParameter that is being compared.")]
        [Input("propertyFullName", "The Full Name of the RevitParameter stored on the object whose Comparison is being computed.")]
        [Input("comparisonConfig", "Additional comparison configurations. You can specify a `ComparisonConfig` or a `RevitComparisonConfig`.")]
        public static ComparisonInclusion ComparisonInclusion(this RevitParameter parameter1, RevitParameter parameter2, string propertyFullName, BaseComparisonConfig comparisonConfig)
        {
            ComparisonInclusion result = new ComparisonInclusion();
            result.DisplayName = parameter1.Name + " (RevitParameter)"; // differences in any property of RevitParameters will be displayed like this.

            // Check if we have a RevitComparisonConfig input.
            RevitComparisonConfig rcc = comparisonConfig as RevitComparisonConfig;
            if (rcc != null)
            {
                // Check the ParametersToConsider: if there is at least one name in the list, make sure that the current revitParameter's Name is contained in the list.
                if ((rcc.ParametersToConsider?.Any() ?? false) && !rcc.ParametersToConsider.Contains(parameter1.Name) || !rcc.ParametersToConsider.Any(ptc => parameter1.Name.WildcardMatch(ptc)))
                {
                    // The parameter is not within the ParametersToConsider.
                    result.Include = false; // RevitParameter must be skipped
                    return result;
                }

                // Check if the current revitParameter is within the ParametersExceptions.
                if ((rcc.ParametersExceptions?.Any() ?? false) && rcc.ParametersExceptions.Contains(parameter1.Name) || rcc.ParametersExceptions.Any(ptc => parameter1.Name.WildcardMatch(ptc)) 
                {
                    result.Include = false; // RevitParameter must be skipped
                    return result;
                }

                // Check the difference in the RevitParameters is a numerical difference, and if so whether it should be included given the input tolerances/significantFigures.
                if (!BH.Engine.Diffing.Query.NumericalDifferenceInclusion(parameter1.Value, parameter2.Value, parameter1.Name, rcc.NumericTolerance, rcc.SignificantFigures, rcc.ParameterNumericTolerances, rcc.ParameterSignificantFigures))
                {
                    result.Include = false; // RevitParameter must be skipped
                    return result;
                }
            }

            return result; // pass the RevitParameter (do not skip it)
        }
    }
}


