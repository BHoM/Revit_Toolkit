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

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Data.Requests;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Deprecated("3.1", "This is a duplicate of BH.Engine.Base.Query.FilterByType method.", typeof(BH.Engine.Base.Query), "FilterByType")]
        [Description("Filters objects by type.")]
        [Input("objects", "Objects to be filtered by Type")]
        [Input("type", "Type")]
        [Output("Objects")]
        public static IEnumerable<object> FilterByType(this IEnumerable<object> objects, Type type)
        {
            if (type == null)
                return objects;

            if (objects == null || objects.Count() == 0)
                return objects;

            List<object> result = new List<object>();
            foreach (object obj in objects)
                if (obj.GetType() == type)
                    result.Add(obj);

            return result;
        }

        /***************************************************/
    }
}
