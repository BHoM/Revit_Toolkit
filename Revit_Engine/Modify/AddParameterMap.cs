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
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Adds ParameterMap to existing ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be extended.")]
        [Input("parameterMap", "ParameterMap to be added.")]
        [Input("merge", "In case when a ParameterMap with type equal to parameterMap already exists in parameterSettings: if true, parameterMap will be merged into the existing one, if false, it will overwrite it.")]
        [Output("parameterSettings")]
        public static ParameterSettings AddParameterMap(this ParameterSettings parameterSettings, ParameterMap parameterMap, bool merge = true)
        {
            if (parameterSettings == null)
                return null;

            if (parameterMap == null || parameterMap.Type == null)
                return parameterSettings;

            ParameterSettings cloneSettings = parameterSettings.GetShallowClone() as ParameterSettings;
            if (cloneSettings.ParameterMaps == null)
                cloneSettings.ParameterMaps = new List<ParameterMap>();
            else
                cloneSettings.ParameterMaps = new List<ParameterMap>(cloneSettings.ParameterMaps);

            ParameterMap cloneMap = cloneSettings.ParameterMaps.Find(x => parameterMap.Type == x.Type);
            if(cloneMap == null)
                cloneSettings.ParameterMaps.Add(parameterMap);
            else
            {
                if (merge)
                    parameterMap = parameterMap.AddParameterLinks(cloneMap.ParameterLinks, true);

                cloneSettings.ParameterMaps.Remove(cloneMap);
                cloneSettings.ParameterMaps.Add(parameterMap);
            }

            return cloneSettings;
        }

        /***************************************************/
    }
}

