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

        [Description("Adds TypeMap to existing MapSettings.")]
        [Input("mapSettings", "MapSettings to be extended.")]
        [Input("typeMap", "TypeMap to be added.")]
        [Input("merge", "If there is an existing TypeMap in MapSettings that maps same BHoM type, if true: merge the one to be added with it, if false: leave both TypeMaps separate.")]
        [Output("mapSettings")]
        public static MapSettings AddTypeMap(this MapSettings mapSettings, TypeMap typeMap, bool merge = true)
        {
            if (mapSettings == null)
                return null;

            if (typeMap == null || typeMap.Type == null)
                return mapSettings;

            MapSettings typeMapSettings = mapSettings.GetShallowClone() as MapSettings;
            if (typeMapSettings.TypeMaps == null)
                typeMapSettings.TypeMaps = new List<TypeMap>();

            TypeMap mapType = typeMapSettings.TypeMaps.Find(x => typeMap.Type.Equals(x.Type));
            if(mapType == null)
            {
                typeMapSettings.TypeMaps.Add(typeMap);
            }
            else
            {
                TypeMap tempTypeMap = typeMap;

                if (merge)
                {
                    tempTypeMap = typeMap.GetShallowClone() as TypeMap;

                    foreach (KeyValuePair<string, HashSet<string>> keyValuePair in mapType.Map)
                    {
                        HashSet<string> hashSet = null;
                        if(!tempTypeMap.Map.TryGetValue(keyValuePair.Key, out hashSet))
                        {
                            hashSet = new HashSet<string>();
                            tempTypeMap.Map[keyValuePair.Key] = hashSet;
                        }

                        foreach (string name in keyValuePair.Value)
                            hashSet.Add(name);
                    }
                }

                typeMapSettings.TypeMaps.Remove(mapType);
                typeMapSettings.TypeMaps.Add(tempTypeMap);
            }

            return typeMapSettings;
        }

        /***************************************************/
    }
}

