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

using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IEnumerable<Type> RevitTypes(this Type type)
        {
            if (m_revitTypes.ContainsKey(type))
                return m_revitTypes[type];

            Type ienumType = typeof(IEnumerable<>).MakeGenericType(type);
            HashSet<Type> revitTypes = new HashSet<Type>(AllConvertMethods().Where(x => type.IsAssignableFrom(x.Key.Item2) || ienumType.IsAssignableFrom(x.Key.Item2)).Select(x => x.Key.Item1));
            m_revitTypes.Add(type, revitTypes);
            return revitTypes;
        }


        /***************************************************/
        /****              Private Fields               ****/
        /***************************************************/

        private static Dictionary<Type, HashSet<Type>> m_revitTypes = new Dictionary<Type, HashSet<Type>>();

        /***************************************************/
    }
}

