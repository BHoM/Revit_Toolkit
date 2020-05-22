/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns first ParameterLink inside ParameterMap for given parameter.")]
        [Input("parameterMap", "ParameterMap to be queried.")]
        [Input("parameter", "Property, which name is to be sought for.")]
        [Output("parameterLink")]
        public static ParameterLink ParameterLink(this oM.Adapters.Revit.Generic.ParameterMap parameterMap, Parameter parameter)
        {
            if (parameterMap == null || parameterMap.ParameterLinks == null || parameter == null || string.IsNullOrWhiteSpace(parameter.Definition.Name))
                return null;

            return parameterMap.ParameterLinks.Find(x => x.ParameterNames.Any(y => y == parameter.Definition.Name));
        }

        /***************************************************/
    }
}


