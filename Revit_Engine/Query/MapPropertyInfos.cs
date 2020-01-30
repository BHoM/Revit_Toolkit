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

using System;
using System.Reflection;
using System.Collections.Generic;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<PropertyInfo> MapPropertyInfos(this Type type)
        {
            if (type == null)
                return null;

            PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (propertyInfos == null || propertyInfos.Length == 0)
                return propertyInfos;

            List<PropertyInfo> result = new List<PropertyInfo>();
            foreach(PropertyInfo pInfo in propertyInfos)
            {
                if (pInfo.GetSetMethod() == null)
                    continue;

                Type propertyType = pInfo.PropertyType;
                if (propertyType == typeof(double) || propertyType == typeof(int) || propertyType == typeof(string) || propertyType == typeof(long) || propertyType == typeof(bool) || propertyType == typeof(short))
                    result.Add(pInfo);
            }

            return result;
        }

        /***************************************************/
    }
}
