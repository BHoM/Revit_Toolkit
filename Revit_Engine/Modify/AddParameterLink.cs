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
using System.Reflection;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Links Revit parameter with BHoM type property or CustomData inside existing ParameterMap.")]
        [Input("parameterMap", "ParameterMap to be extended.")]
        [Input("sourceName", "BHoM type property or CustomData name.")]
        [Input("destinationName", "Revit parameter name to be mapped.")]
        [Output("parameterMap")]
        public static ParameterMap AddParameterLink(this ParameterMap parameterMap, string sourceName, string destinationName)
        {
            if (string.IsNullOrWhiteSpace(destinationName))
                return parameterMap;

            return AddParameterLink(parameterMap, sourceName, new string[] { destinationName });
        }

        /***************************************************/
        
        [Description("Links Revit parameters with BHoM type property or CustomData inside existing ParameterMap. In case of multiple destinationNames, first found will be considered correspondent with given type property.")]
        [Input("parameterMap", "ParameterMap to be extended.")]
        [Input("sourceName", "BHoM type property or CustomData name.")]
        [Input("destinationNames", "Revit parameter names to be mapped.")]
        [Output("parameterMap")]
        public static ParameterMap AddParameterLink(this ParameterMap parameterMap, string sourceName, IEnumerable<string> destinationNames)
        {
            if (parameterMap == null)
                return null;

            if (string.IsNullOrWhiteSpace(sourceName) || destinationNames == null || destinationNames.Count() == 0)
                return parameterMap;

            Type type = parameterMap.Type;

            if (type == null)
                return parameterMap;

            ParameterMap clonedMap = parameterMap.GetShallowClone() as ParameterMap;
            IEnumerable<PropertyInfo> propertyInfos = Query.MapPropertyInfos(type);
            if (propertyInfos == null || propertyInfos.Count() == 0)
                return parameterMap;

            foreach (PropertyInfo pInfo in propertyInfos)
                if (pInfo.Name == sourceName)
                {
                    if (clonedMap.ParameterLinks == null)
                        clonedMap.ParameterLinks = new Dictionary<string, HashSet<string>>();

                    HashSet<string> hashSet = null;
                    if (!clonedMap.ParameterLinks.TryGetValue(sourceName, out hashSet))
                    {
                        hashSet = new HashSet<string>();
                        clonedMap.ParameterLinks[sourceName] = hashSet;
                    }

                    foreach (string destinationName in destinationNames)
                        if (!string.IsNullOrWhiteSpace(destinationName))
                            hashSet.Add(destinationName);

                    return clonedMap;
                }

            return clonedMap;
        }

        /***************************************************/

        [Description("Links Revit parameter with BHoM type property or CustomData inside existing ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be extended.")]
        [Input("type", "BHoM type to be mapped.")]
        [Input("sourceName", "BHoM type property or CustomData name.")]
        [Input("destinationName", "Revit parameter name to be mapped.")]
        [Output("parameterSettings")]
        public static ParameterSettings AddParameterLink(this ParameterSettings parameterSettings, Type type, string sourceName, string destinationName)
        {
            if (parameterSettings == null)
                return null;

            if (type == null || string.IsNullOrWhiteSpace(sourceName) || string.IsNullOrWhiteSpace(destinationName))
                return parameterSettings;

            ParameterMap parameterMap = Create.ParameterMap(type);
            parameterMap = parameterMap.AddParameterLink(sourceName, destinationName);
            if (parameterMap == null)
                return parameterSettings;

            return parameterSettings.AddParameterMap(parameterMap, true);            
        }

        /***************************************************/
    }
}

