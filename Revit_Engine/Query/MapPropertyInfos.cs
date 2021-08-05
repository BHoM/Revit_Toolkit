/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a collection of PropertyInfo objects owned by given type, which can be used to map parameters with MappingSettings.")]
        [Input("type", "Type to be queried.")]
        [Output("mapPropertyInfos")]
        public static IEnumerable<PropertyInfo> MapPropertyInfos(this Type type)
        {
            if (type == null)
                return null;

            if (m_PropertyInfos.ContainsKey(type))
                return m_PropertyInfos[type];

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
            if (typeof(IBHoMObject).IsAssignableFrom(type))
                propertyInfos.Add(type.GetProperty("Name"));

            propertyInfos.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            for (int i = propertyInfos.Count - 1; i >= 0; i--)
            {
                PropertyInfo pInfo = propertyInfos[i];
                if (pInfo.GetSetMethod() == null)
                {
                    propertyInfos.RemoveAt(i);
                    continue;
                }

                Type propertyType = pInfo.PropertyType;
                if (!(propertyType.IsEnum || propertyType == typeof(double) || propertyType == typeof(int) || propertyType == typeof(string) || propertyType == typeof(long) || propertyType == typeof(bool) || propertyType == typeof(short)))
                    propertyInfos.RemoveAt(i);
            }

            m_PropertyInfos.Add(type, propertyInfos);
            return propertyInfos;
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        private static Dictionary<Type, IEnumerable<PropertyInfo>> m_PropertyInfos = new Dictionary<Type, IEnumerable<PropertyInfo>>();

        /***************************************************/
    }
}

