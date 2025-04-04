/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates an entity defining the relationship between property name of an object (or name of a RevitParameter attached to it) and sets of their correspondent Revit element parameter names.")]
        [InputFromProperty("propertyName")]
        [InputFromProperty("parameterNames")]
        [Output("parameterLink")]
        public static ElementParameterLink ElementParameterLink(string propertyName, IEnumerable<string> parameterNames)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                BH.Engine.Base.Compute.RecordError("It is impossible to create parameter links for an empty property name.");
                return null;
            }

            if (parameterNames == null || !parameterNames.Any())
            {
                BH.Engine.Base.Compute.RecordError("It is impossible to create parameter links for an empty parameter name collection.");
                return null;
            }

            return new ElementParameterLink { PropertyName = propertyName, ParameterNames = new HashSet<string>(parameterNames) };
        }

        /***************************************************/
    }
}






