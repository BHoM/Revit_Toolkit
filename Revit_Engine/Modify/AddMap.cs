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
        
        [Description("Links Revit parameter with BHoM type property or CustomData inside existing TypeMap.")]
        [Input("typeMap", "TypeMap to be extended.")]
        [Input("sourceName", "BHoM type property or CustomData name.")]
        [Input("destinationName", "Revit parameter name to be mapped.")]
        [Output("typeMap")]
        public static TypeMap AddMap(this TypeMap typeMap, string sourceName, string destinationName)
        {
            if (string.IsNullOrWhiteSpace(destinationName))
                return typeMap;

            return AddMap(typeMap, sourceName, new string[] { destinationName });
        }

        /***************************************************/
        
        [Description("Links Revit parameters with BHoM type property or CustomData inside existing TypeMap.")]
        [Input("typeMap", "TypeMap to be extended.")]
        [Input("sourceName", "BHoM type property or CustomData name.")]
        [Input("destinationName", "Revit parameter name to be mapped.")]
        [Output("typeMap")]
        public static TypeMap AddMap(this TypeMap typeMap, string sourceName, IEnumerable<string> destinationNames)
        {
            if (typeMap == null)
                return null;

            if (string.IsNullOrWhiteSpace(sourceName) || destinationNames == null || destinationNames.Count() == 0)
                return typeMap;

            Type type = typeMap.Type;

            if (type == null)
                return typeMap;

            TypeMap clonedTypeMap = typeMap.GetShallowClone() as TypeMap;
            IEnumerable<PropertyInfo> propertyInfos = Query.MapPropertyInfos(type);
            if (propertyInfos == null || propertyInfos.Count() == 0)
                return typeMap;

            foreach (PropertyInfo pInfo in propertyInfos)
                if (pInfo.Name == sourceName)
                {
                    if (clonedTypeMap.Map == null)
                        clonedTypeMap.Map = new Dictionary<string, HashSet<string>>();

                    HashSet<string> hashSet = null;
                    if (!clonedTypeMap.Map.TryGetValue(sourceName, out hashSet))
                    {
                        hashSet = new HashSet<string>();
                        clonedTypeMap.Map[sourceName] = hashSet;
                    }

                    foreach (string destinationName in destinationNames)
                        if (!string.IsNullOrWhiteSpace(destinationName))
                            hashSet.Add(destinationName);

                    return clonedTypeMap;
                }

            return clonedTypeMap;
        }

        /***************************************************/

        [Description("Links Revit parameter with BHoM type property or CustomData inside existing MapSettings.")]
        [Input("mapSettings", "MapSettings to be extended.")]
        [Input("type", "BHoM type to be mapped.")]
        [Input("sourceName", "BHoM type property or CustomData name.")]
        [Input("destinationName", "Revit parameter name to be mapped.")]
        [Output("mapSettings")]
        public static MapSettings AddMap(this MapSettings mapSettings, Type type, string sourceName, string destinationName)
        {
            if (mapSettings == null)
                return null;

            if (type == null || string.IsNullOrWhiteSpace(sourceName) || string.IsNullOrWhiteSpace(destinationName))
                return mapSettings;

            TypeMap typeMap = Create.TypeMap(type);
            typeMap = typeMap.AddMap(sourceName, destinationName);
            if (typeMap == null)
                return mapSettings;

            return mapSettings.AddTypeMap(typeMap, true);            
        }

        /***************************************************/
    }
}

