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
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns Property differences between RevitParameters (RevitParametersToPush) owned by the two input objects." +
            "This method can be added as Func delegate to the DiffingConfig; if so, it is automatically triggered when Diffing the two objects.")]
        [Input("obj1", "Past object being compared.")]
        [Input("obj2", "Following object being compared.")]
        [Input("comparisonConfig", "Comparison Config to be used during comparison.")]
        [Output("parametersDifferences", "Differences in terms of RevitParametersToPush found on the two input objects.")]
        public static List<IPropertyDifference> RevitRevitParametersToPushDifferences(this object obj1, object obj2, BaseComparisonConfig comparisonConfig)
        {
            if (obj1 == null || obj2 == null)
                return new List<IPropertyDifference>();

            return RevitParametersDifferences<RevitParametersToPush>(obj1, obj2, comparisonConfig);
        }
    }
}





