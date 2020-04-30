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

        [Description("Links Revit parameters with BHoM type property or CustomData inside existing ParameterMap. In case of multiple destinationNames, first found will be considered correspondent with given type property.")]
        [Input("parameterMap", "ParameterMap to be extended.")]
        [Input("propertyName", "BHoM type property or CustomData name.")]
        [Input("parameterNames", "Revit parameter names to be mapped.")]
        [Input("merge", "In case when propertyName already exists in parameterMap: if true, the parameterNames will be added to the existing collection of parameter names, if false, it will overwrite it.")]
        [Output("parameterMap")]
        public static ParameterMap AddParameterLinks(this ParameterMap parameterMap, IEnumerable<ParameterLink> parameterLinks, bool merge = true)
        {
            if (parameterMap == null)
                return null;

            if (parameterMap.Type == null || parameterLinks == null || parameterLinks.Count() == 0)
                return parameterMap;

            IEnumerable<PropertyInfo> propertyInfos = Query.MapPropertyInfos(parameterMap.Type);
            if (propertyInfos == null)
                return parameterMap;

            ParameterMap clonedMap = parameterMap.GetShallowClone() as ParameterMap;
            if (clonedMap.ParameterLinks == null)
                clonedMap.ParameterLinks = new List<ParameterLink>();
            else
                clonedMap.ParameterLinks = new List<ParameterLink>(parameterMap.ParameterLinks);

            foreach (ParameterLink parameterLink in parameterLinks)
            {
                ParameterLink existingLink = clonedMap.ParameterLinks.Find(x => x.PropertyName == parameterLink.PropertyName);
                if (existingLink == null)
                    clonedMap.ParameterLinks.Add(parameterLink);
                else
                {
                    clonedMap.ParameterLinks.Remove(existingLink);
                    if (merge)
                    {
                        ParameterLink newLink = existingLink.GetShallowClone() as ParameterLink;
                        newLink.ParameterNames = new HashSet<string>(existingLink.ParameterNames);
                        newLink.ParameterNames.UnionWith(parameterLink.ParameterNames);
                        clonedMap.ParameterLinks.Add(newLink);
                    }
                    else
                        clonedMap.ParameterLinks.Add(parameterLink);
                }
            }

            return clonedMap;
        }

        /***************************************************/

        [Description("Links Revit parameter with BHoM type property or CustomData inside existing ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be extended.")]
        [Input("type", "BHoM type to be mapped.")]
        [Input("propertyName", "BHoM type property or CustomData name.")]
        [Input("parameterName", "Revit parameter name to be mapped.")]
        [Output("parameterSettings")]
        public static ParameterSettings AddParameterLinks(this ParameterSettings parameterSettings, Type type, IEnumerable<ParameterLink> parameterLinks, bool merge = true)
        {
            if (parameterSettings == null)
                return null;

            if (type == null || parameterLinks == null || parameterLinks.Count() == 0)
                return parameterSettings;

            ParameterSettings cloneSettings = parameterSettings.GetShallowClone() as ParameterSettings;
            cloneSettings.ParameterMaps = new List<ParameterMap>(parameterSettings.ParameterMaps);

            ParameterMap parameterMap = cloneSettings.ParameterMap(type);
            if (parameterMap == null)
                cloneSettings.ParameterMaps.Add(new ParameterMap { Type = type, ParameterLinks = new List<ParameterLink>(parameterLinks) });
            else
            {
                cloneSettings.ParameterMaps.Remove(parameterMap);
                cloneSettings.ParameterMaps.Add(parameterMap.AddParameterLinks(parameterLinks, merge));
            }

            return cloneSettings;
        }

        /***************************************************/
    }
}

