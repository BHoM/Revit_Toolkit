/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds a parent supported Revit API type for an unsupported one (unsupported types are the ones that exist in the API but cannot be interacted with).\n" +
                     "If the input type is not unsupported, it will be returned.")]
        [Input("type", "Type to be queried for its parent supported Revit API type.")]
        [Output("supportedType", "Supported Revit API type - either the input type or the first of its parents that is supported.")]
        public static Type SupportedAPIType(this Type type)
        {
            foreach (KeyValuePair<Type, Type[]> kvp in UnsupportedAPITypes)
            {
                if (kvp.Value.Any(x => x == type))
                    return kvp.Key;
            }

            return type;
        }

        /***************************************************/
    }
}
