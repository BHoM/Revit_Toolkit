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
        [Description("When Diffing or Hashing, determine if and how `RevitParameter`'s properties should be included.")]
        public static ComparisonInclusion ComparisonInclusion(this RevitParameter revitParameter, string propertyFullName, ComparisonConfig cc)
        {
            ComparisonInclusion result = new ComparisonInclusion();
            result.DisplayName = revitParameter.Name + " (RevitParameter)"; // differences in any property of RevitParameters will be displayed like this.

            // Check if the caller (the Diffing or Hashing process) had a RevitComparisonConfig input.
            RevitComparisonConfig rcc = cc as RevitComparisonConfig;
            if (rcc == null)
                return result;

            // If a RevitComparisonConfig was input, check the ParametersToConsider.
            // If there is at least one name in the list, make sure that the current revitParameter's Name is contained in the list.
            if ((rcc.ParametersToConsider?.Any() ?? false) && !rcc.ParametersToConsider.Contains(revitParameter.Name))
            {
                // The parameter is not within the ParametersToConsider.
                result.Include = false; // RevitParameter must be skipped
                return result; 
            }

            // If a RevitComparisonConfig was input, check if the current revitParameter is within the ParametersExceptions.
            if (rcc.ParametersExceptions?.Contains(revitParameter.Name) ?? false)
            {
                result.Include = false; // RevitParameter must be skipped
                return result;
            }

            return result; // pass the RevitParameter (do not skip it)
        }
    }
}


