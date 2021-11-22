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

using BH.Engine.Reflection;
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
        public static string HashString(this RevitParameter revitParameter, string propertyFullName, BaseComparisonConfig comparisonConfig)
        {
            // Null check.
            if (revitParameter == null)
                return null;

            string hashString = revitParameter.Name + revitParameter.Value;

            // Check if we have a RevitComparisonConfig input.
            RevitComparisonConfig rcc = comparisonConfig as RevitComparisonConfig;
            if (rcc != null)
            {
                // If there is at least one name in the ParametersToConsider, make sure that the current revitParameter's Name is contained in the list.
                if ((rcc.ParametersToConsider?.Any() ?? false) && !rcc.ParametersToConsider.Contains(revitParameter.Name))
                    // The parameter is not within the ParametersToConsider. RevitParameter must be skipped
                    return null;

                // Check if the current revitParameter is within the ParametersExceptions.
                if (rcc.ParametersExceptions?.Contains(revitParameter.Name) ?? false)
                    // The parameter is not within the ParametersToConsider. RevitParameter must be skipped
                    return null;

                // If the RevitParameter is numeric, take care of custom tolerances/significant figures.
                if (revitParameter.Value.GetType().IsNumeric())
                {
                    // If we didn't specify any custom tolerance/significant figures, just return the input.
                    if (rcc.NumericTolerance == double.MinValue && rcc.SignificantFigures == int.MaxValue
                        && (!rcc.ParameterNumericTolerances?.Any() ?? true) && (!rcc.ParameterSignificantFigures?.Any() ?? true))
                        return hashString;

                    // Otherwise, return the approximated value.
                    return BH.Engine.Base.Query.NumericalApproximation(
                        double.Parse(revitParameter.Value.ToString()), propertyFullName, rcc.ParameterNumericTolerances, rcc.NumericTolerance, rcc.ParameterSignificantFigures, rcc.SignificantFigures)
                        .ToString();
                }
            }

            // Pass the RevitParameter (do not skip it). Return a string that uniquely represents this object.
            return revitParameter.Name + revitParameter.Value;
        }
    }
}


