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

using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates an object that contains the information about the relationship between object's property names (or names of RevitParameters attached to it) and Revit parameter names.")]
        [InputFromProperty("type")]
        [InputFromProperty("parameterLinks")]
        [Output("parameterMap")]
        public static ParameterMap ParameterMap(Type type, IEnumerable<IParameterLink> parameterLinks = null)
        {
            if (type == null)
                return null;

            ParameterMap parameterMap = new ParameterMap { Type = type };

            if (parameterLinks != null)
                parameterMap = parameterMap.AddParameterLinks(parameterLinks);

            return parameterMap;
        }

        /***************************************************/
    }
}





