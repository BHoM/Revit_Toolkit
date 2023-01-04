/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Enums;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns Property differences between RevitParameters that are of a specific type.")]
        [Input("revitParamDifferences", "Revit parameter differences that we want to obtain a subset of.")]
        [Input("revitParameterDifferenceType", "Type of RevitParameterDifference we want to get.")]
        [Output("parametersDifferences", "RevitParamDifferences that are of the input type.")]
        public static List<RevitParameterDifference> OfType(this IEnumerable<RevitParameterDifference> revitParamDifferences, RevitParameterDifferenceType revitParameterDifferenceType)
        {
            if (revitParamDifferences == null)
                return new List<RevitParameterDifference>();

            return revitParamDifferences.Where(rpd => rpd.DifferenceType == revitParameterDifferenceType).ToList();
        }
    }
}




