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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Find a relevant Revit => BHoM convert method for a given Revit Element and target BHoM type.")]
        [Input("from", "Revit Element to find a convert method for.")]
        [Input("targetBHoMType", "BHoM type, to which the Revit Element is meant to be converted.")]
        [Output("method", "Relevant Revit => BHoM convert method for a given Revit Element and target BHoM type.")]
        public static MethodInfo ConvertMethod(this Element from, Type targetBHoMType)
        {
            if (from == null)
            {
                BH.Engine.Base.Compute.RecordError("Unable to extract convert method for a null Revit element.");
                return null;
            }

            if (targetBHoMType == null)
            {
                BH.Engine.Base.Compute.RecordError("Unable to extract convert method for a null target BHoM type.");
                return null;
            }

            Type revitType = from.GetType();
            foreach (KeyValuePair<Tuple<Type, Type>, MethodInfo> kvp in AllConvertMethods())
            {
                if (kvp.Key.Item1.IsAssignableFrom(revitType) && (kvp.Key.Item2.IsAssignableFrom(targetBHoMType) || (typeof(IEnumerable<IBHoMObject>).IsAssignableFrom(kvp.Key.Item2) && kvp.Key.Item2.GetGenericArguments()[0].IsAssignableFrom(targetBHoMType))))
                    return kvp.Value;
            }

            return null;
        }
        
        /***************************************************/
    }
}



