﻿/*
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

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Removes ParameterMaps correspondent to given types from existing ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be modified.")]
        [Input("type", "Type related to ParameterMap meant to be removed.")]
        [Output("parameterSettings")]
        public static ParameterSettings RemoveParameterMaps(this ParameterSettings parameterSettings, IEnumerable<Type> types)
        {
            if (parameterSettings == null)
                return null;

            if (parameterSettings.ParameterMaps == null)
                return parameterSettings;

            ParameterSettings cloneSettings = parameterSettings.GetShallowClone() as ParameterSettings;
            cloneSettings.ParameterMaps = parameterSettings.ParameterMaps.Where(x => types.All(y=> x.Type != y)).ToList();
            return cloneSettings;
        }

        /***************************************************/
    }
}

