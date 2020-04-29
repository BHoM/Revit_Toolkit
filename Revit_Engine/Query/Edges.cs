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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns edges of given BHoMObject captured on Pull. This value is stored in CustomData under key Revit_edges.")]
        [Input("bHoMObject", "BHoMObject to be queried.")]
        [Output("edges")]
        public static List<oM.Geometry.ICurve> Edges(this IBHoMObject bHoMObject)
        {
            if (bHoMObject == null)
                return null;

            object value = null;
            if (bHoMObject.CustomData.TryGetValue(Convert.Edges, out value))
            {
                if(value is IEnumerable<oM.Geometry.ICurve>)
                    return (value as IEnumerable<oM.Geometry.ICurve>).ToList();
            }

            return null;
        }

        /***************************************************/
    }
}
