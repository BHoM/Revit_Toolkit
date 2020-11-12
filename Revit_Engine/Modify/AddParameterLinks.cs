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

using BH.Engine.Base;
using BH.oM.Adapters.Revit.Parameters;
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

        [Description("Adds ParameterLinks to existing ParameterMap.")]
        [Input("parameterMap", "ParameterMap to be extended.")]
        [Input("parameterLinks", "ParameterLinks to be added.")]
        [Input("merge", "In case when PropertyName of input ParameterLink already exists in a ParameterMap: if true, the parameterNames will be added to the existing collection of parameter names, if false, they will overwrite it.")]
        [Output("parameterMap")]
        public static ParameterMap AddParameterLinks(this ParameterMap parameterMap, IEnumerable<IParameterLink> parameterLinks, bool merge = true)
        {
            if (parameterMap == null)
                return null;

            if (parameterMap.Type == null || parameterLinks == null || parameterLinks.Count() == 0)
                return parameterMap;

            IEnumerable<PropertyInfo> propertyInfos = Query.MapPropertyInfos(parameterMap.Type);
            if (propertyInfos == null)
                return parameterMap;

            ParameterMap clonedMap = parameterMap.ShallowClone() as ParameterMap;
            if (clonedMap.ParameterLinks == null)
                clonedMap.ParameterLinks = new List<IParameterLink>();
            else
                clonedMap.ParameterLinks = new List<IParameterLink>(parameterMap.ParameterLinks);

            foreach (IParameterLink parameterLink in parameterLinks)
            {
                Type linkType = parameterLink.GetType();
                IParameterLink existingLink = clonedMap.ParameterLinks.Find(x => x.PropertyName == parameterLink.PropertyName && x.GetType() == linkType);
                if (existingLink == null)
                    clonedMap.ParameterLinks.Add(parameterLink);
                else
                {
                    clonedMap.ParameterLinks.Remove(existingLink);
                    if (merge)
                    {
                        IParameterLink newLink = existingLink.ShallowClone() as IParameterLink;
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

        [Description("Adds ParameterLinks to existing ParameterSettings.")]
        [Input("parameterSettings", "ParameterSettings to be extended.")]
        [Input("type", "Type, for which the ParameterLinks are meant to be added.")]
        [Input("parameterLinks", "ParameterLinks to be added.")]
        [Input("merge", "In case when PropertyName of input ParameterLink already exists in a ParameterMap of given type: if true, the parameterNames will be added to the existing collection of parameter names, if false, they will overwrite it.")]
        [Output("parameterSettings")]
        public static ParameterSettings AddParameterLinks(this ParameterSettings parameterSettings, Type type, IEnumerable<IParameterLink> parameterLinks, bool merge = true)
        {
            if (parameterSettings == null)
                return null;

            if (type == null || parameterLinks == null || parameterLinks.Count() == 0)
                return parameterSettings;

            ParameterSettings cloneSettings = parameterSettings.ShallowClone() as ParameterSettings;
            cloneSettings.ParameterMaps = new List<ParameterMap>(parameterSettings.ParameterMaps);

            ParameterMap parameterMap = cloneSettings.ParameterMap(type);
            if (parameterMap == null)
                cloneSettings.ParameterMaps.Add(new ParameterMap { Type = type, ParameterLinks = new List<IParameterLink>(parameterLinks) });
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

