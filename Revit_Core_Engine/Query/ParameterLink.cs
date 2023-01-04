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

using Autodesk.Revit.DB;
using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns first ParameterLink inside ParameterMap for given parameter.")]
        [Input("parameterLinks", "Collection of ParameterLinks to be searched.")]
        [Input("parameter", "Property, which name is to be sought for.")]
        [Output("parameterLink")]
        public static IParameterLink ParameterLink(this IEnumerable<IParameterLink> parameterLinks, Parameter parameter)
        {
            if (parameterLinks == null || parameter == null || string.IsNullOrWhiteSpace(parameter.Definition.Name))
                return null;

            return parameterLinks.FirstOrDefault(x => x.ParameterNames.Any(y => y == parameter.Definition.Name));
        }

        /***************************************************/
    }
}





