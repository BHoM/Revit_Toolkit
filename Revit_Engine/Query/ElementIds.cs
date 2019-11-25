/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets integer representation of ElementIds for given FilterRequest (Example: SelectionFilterRequest).")]
        [Input("filterRequest", "FilterRequest")]
        [Output("ElementIds")]
        public static IEnumerable<int> ElementIds(this FilterRequest filterRequest)
        {
            if (filterRequest == null)
                return null;

            if (!filterRequest.Equalities.ContainsKey(Convert.FilterRequest.ElementIds))
                return null;

            IEnumerable<int> result = filterRequest.Equalities[Convert.FilterRequest.ElementIds] as IEnumerable<int>;

            if (result == null && filterRequest.Equalities[Convert.FilterRequest.ElementIds] is IEnumerable<object>)
            {
                IEnumerable<object> objects = filterRequest.Equalities[Convert.FilterRequest.ElementIds] as IEnumerable<object>;
                if (objects != null)
                    return objects.ToList().ConvertAll(x => System.Convert.ToInt32(x));
            }

            return result;
        }

        /***************************************************/
    }
}