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

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System;
using System.ComponentModel;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns ParameterMap for given type inside ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be queried.")]
        [Input("type", "Type to be sought for.")]
        [Output("parameterMap")]
        public static ParameterLink ParameterLink(this ParameterSettings parameterSettings, Type type, string propertyName)
        {
            if (parameterSettings == null || parameterSettings.ParameterMaps == null || type == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            ParameterMap parameterMap = parameterSettings.ParameterMap(type);
            if (parameterMap == null)
                return null;

            return parameterMap.ParameterLink(propertyName);
        }

        /***************************************************/

        [Description("Returns ParameterMap for given type inside ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be queried.")]
        [Input("type", "Type to be sought for.")]
        [Output("parameterMap")]
        public static ParameterLink ParameterLink(this ParameterMap parameterMap, string propertyName)
        {
            if (parameterMap == null || parameterMap.ParameterLinks == null || string.IsNullOrWhiteSpace(propertyName))
                return null;

            return parameterMap.ParameterLinks.Find(x => x.PropertyName == propertyName);
        }

        /***************************************************/
    }
}


