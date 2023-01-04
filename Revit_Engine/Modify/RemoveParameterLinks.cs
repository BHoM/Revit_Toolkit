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
using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
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

        [Description("Removes links between Revit parameters and object's properties (or name of a RevitParameter attached to it) inside existing ParameterMap.")]
        [Input("parameterMap", "ParameterMap to be modified.")]
        [Input("propertyNames", "Names of type properties (or RevitParameters), for which the ParameterLinks are meant to be removed.")]
        [Output("parameterMap")]
        public static ParameterMap RemoveParameterLinks(this ParameterMap parameterMap, IEnumerable<string> propertyNames)
        {
            if (parameterMap == null)
                return null;

            if (parameterMap.Type == null || parameterMap.ParameterLinks == null || propertyNames == null || propertyNames.Count() == 0)
                return parameterMap;

            ParameterMap clonedMap = parameterMap.ShallowClone();
            clonedMap.ParameterLinks = parameterMap.ParameterLinks.Where(x => propertyNames.All(y => x.PropertyName != y)).ToList();
            return clonedMap;
        }

        /***************************************************/

        [Description("Removes links between Revit parameters and object's properties (or name of a RevitParameter attached to it) inside existing MappingSettings.")]
        [Input("mappingSettings", "MappingSettings to be modified.")]
        [Input("type", "Type related to ParameterMap meant to be modified.")]
        [Input("propertyNames", "Names of type properties (or RevitParameters), for which the ParameterLinks are meant to be removed.")]
        [Output("mappingSettings")]
        public static MappingSettings RemoveParameterLinks(this MappingSettings mappingSettings, Type type, IEnumerable<string> propertyNames)
        {
            if (mappingSettings == null)
                return null;

            if (type == null || propertyNames == null || propertyNames.Count() == 0)
                return mappingSettings;

            ParameterMap parameterMap = mappingSettings.ParameterMaps.Find(x => x.Type == type);
            if (parameterMap == null)
                return mappingSettings;

            MappingSettings cloneSettings = mappingSettings.ShallowClone();
            cloneSettings.ParameterMaps = new List<ParameterMap>(mappingSettings.ParameterMaps);
            cloneSettings.ParameterMaps.Remove(parameterMap);
            cloneSettings.ParameterMaps.Add(parameterMap.RemoveParameterLinks(propertyNames));
            return cloneSettings;
        }

        /***************************************************/
    }
}




