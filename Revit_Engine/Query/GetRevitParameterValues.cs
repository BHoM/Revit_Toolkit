/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //TODO: to be rather solved by improving Excel_UI?
        [Description("Retrieves values of a parameter attached to a groupe of BHoM objects. It will return null values for objects that do not have  the parameter defined")]
        [Input("List<bHoMObject>", "BHoMObjects to which the parameters will be attached.")]
        [Input("parameterName", "Name of the parameter to be sought for.")]
        [Output("List of values in same order as objects' list")]
        public static List<object> GetRevitParameterValues(this List<IBHoMObject> bHoMObjects, string parameterName)
        {
            List<object> values = new List<object>();
            for (int i = 0; i < bHoMObjects.Count; i++)
            {
                object value = GetRevitParameterValue(bHoMObjects[i], parameterName);
                values.Add(value);
            }
            return values;
        }
        /***************************************************/
    }
}






