/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Adapters.Revit.Settings;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
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

            Type aType = typeMap.Type;

            if (aType == null)
                return typeMap;

            TypeMap aTypeMap = typeMap.GetShallowClone() as TypeMap;
            IEnumerable<PropertyInfo> aPropertyInfos = Query.MapPropertyInfos(aType);
            if (aPropertyInfos == null || aPropertyInfos.Count() == 0)
                return typeMap;

            foreach (PropertyInfo aPropertyInfo in aPropertyInfos)
                if (aPropertyInfo.Name == sourceName)
                {
                    if (aTypeMap.Map == null)
                        aTypeMap.Map = new Dictionary<string, HashSet<string>>();

                    HashSet<string> aHashSet = null;
                    if (!aTypeMap.Map.TryGetValue(sourceName, out aHashSet))
                    {
                        aHashSet = new HashSet<string>();
                        aTypeMap.Map[sourceName] = aHashSet;
                    }

                    foreach (string aDestinationName in destinationNames)
                        if (!string.IsNullOrWhiteSpace(aDestinationName))
                            aHashSet.Add(aDestinationName);

                    return aTypeMap;
                }

            return aTypeMap;
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

            TypeMap aTypeMap = Create.TypeMap(type);
            aTypeMap = aTypeMap.AddMap(sourceName, destinationName);
            if (aTypeMap == null)
                return mapSettings;

            return mapSettings.AddTypeMap(aTypeMap, true);            
        }

        /***************************************************/
    }
}
