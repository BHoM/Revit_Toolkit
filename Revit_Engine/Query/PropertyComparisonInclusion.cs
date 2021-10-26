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
        [Description("When Diffing or Hashing an object that owns a property of type of `RevitParameter`, this function determines if the RevitParameter is to be considered, " +
            "and how its name should be registered when a difference is returned.")]
        public static PropertyComparisonInclusion PropertyComparisonInclusion(this RevitParameter rp, string propertyFullName, ComparisonConfig cc)
        {
            PropertyComparisonInclusion result = new PropertyComparisonInclusion();
            result.PropertyDisplayName = rp.Name + " (RevitParameter)";

            RevitComparisonConfig rcc = cc as RevitComparisonConfig;
            if (rcc == null)
                return result; // pass the RevitParameter (do not skip it)

            if (!rcc.ParametersToConsider?.Contains(rp.Name) ?? false)
            {
                result.IncludeProperty = false;
                return result; // RevitParameter must be skipped
            }

            if (rcc.ParametersExceptions?.Contains(rp.Name) ?? false)
            {
                result.IncludeProperty = false;
                return result; // RevitParameter must be skipped
            }

            return result; // pass the RevitParameter (do not skip it)
        }
    }
}

