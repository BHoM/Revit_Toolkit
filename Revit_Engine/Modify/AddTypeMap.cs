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

        [Description("Add TypeMap to the given MapSettings")]
        [Input("mapSettings", "MapSettings")]
        [Input("typeMap", "TypeMap to be added")]
        [Input("merge", "merge TypeMap if already exists in MapSettings")]
        [Output("MapSettings")]
        public static MapSettings AddTypeMap(this MapSettings mapSettings, TypeMap typeMap, bool merge = true)
        {
            if (mapSettings == null)
                return null;

            if (typeMap == null || typeMap.Type == null)
                return mapSettings;

            MapSettings aMapSettings = mapSettings.GetShallowClone() as MapSettings;
            if (aMapSettings.TypeMaps == null)
                aMapSettings.TypeMaps = new List<TypeMap>();

            TypeMap aTypeMap = aMapSettings.TypeMaps.Find(x => typeMap.Type.Equals(x.Type));
            if(aTypeMap == null)
            {
                aMapSettings.TypeMaps.Add(typeMap);
            }
            else
            {
                TypeMap aTypeMap_Temp = typeMap;

                if (merge)
                {
                    aTypeMap_Temp = typeMap.GetShallowClone() as TypeMap;

                    foreach (KeyValuePair<string, HashSet<string>> aKeyValuePair in aTypeMap.Map)
                    {
                        HashSet<string> aHashSet = null;
                        if(!aTypeMap_Temp.Map.TryGetValue(aKeyValuePair.Key, out aHashSet))
                        {
                            aHashSet = new HashSet<string>();
                            aTypeMap_Temp.Map[aKeyValuePair.Key] = aHashSet;
                        }

                        foreach (string aName in aKeyValuePair.Value)
                            aHashSet.Add(aName);
                    }
                }

                aMapSettings.TypeMaps.Remove(aTypeMap);
                aMapSettings.TypeMaps.Add(aTypeMap_Temp);
            }

            return aMapSettings;
        }

        /***************************************************/
    }
}
