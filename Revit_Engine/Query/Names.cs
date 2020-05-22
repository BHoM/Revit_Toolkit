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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a collection of Revit parameter names associated with a given type property inside ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be queried.")]
        [Input("type", "Type to be sought for.")]
        [Input("name", "Property name to be sought for.")]
        [Output("names")]
        public static HashSet<string> Names(this ParameterSettings parameterSettings, Type type, string name)
        {
            if (parameterSettings == null || type == null || string.IsNullOrWhiteSpace(name))
                return null;

            ParameterMap parameterMap = parameterSettings.ParameterMap(type);
            if (parameterMap == null || parameterMap.ParameterLinks == null)
                return null;

            ParameterLink parameterLink = parameterMap.ParameterLinks.Find(x => x.PropertyName == name);
            if (parameterLink == null)
                return null;
            else
                return parameterLink.ParameterNames;
        }

        /***************************************************/
    }
}
