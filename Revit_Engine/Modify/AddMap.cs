/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Settings;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Links Revit parameter with BHoM parameter for given TypeMap.")]
        [Input("typeMap", "TypeMap")]
        [Input("sourceName", "BHoM parameter name of type")]
        [Input("destinationName", "Revit parameter name to be mapped")]
        [Output("TypeMap")]
        public static TypeMap AddMap(this TypeMap typeMap, string sourceName, string destinationName)
        {
            if (string.IsNullOrWhiteSpace(destinationName))
                return typeMap;

            return AddMap(typeMap, sourceName, new string[] { destinationName });
        }

        /***************************************************/

        [Description("Links Revit parameter with BHoM parameter for given TypeMap.")]
        [Input("typeMap", "TypeMap")]
        [Input("sourceName", "BHoM parameter name of type")]
        [Input("destinationName", "Revit parameter name to be mapped")]
        [Output("TypeMap")]
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

                    foreach (string aDestinationName in destinationNames)
                        if (!string.IsNullOrWhiteSpace(aDestinationName))
                            hashSet.Add(aDestinationName);

                    return clonedTypeMap;
                }

            return clonedTypeMap;
        }

        /***************************************************/

        [Description("Links Revit parameter with BHoM parameter for given type and MapSettings.")]
        [Input("mapSettings", "MapSettings")]
        [Input("type", "BHoM type to be mapped")]
        [Input("sourceName", "BHoM parameter name of type")]
        [Input("destinationName", "Revit parameter name to be mapped")]
        [Output("MapSettings")]
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
