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

using BH.oM.Adapters.Revit;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns an IRequest that will filter elements of a given BHoM type. If ids argument is not null, filter will be narrowed down to elements with relevant Revit ElementIds or UniqueIds.")]
        [Input("type", "BHoM type of elements to be filtered")]
        [Input("ids", "Optional argument, a list of Revit ElementIds or UniqueIds to be used to narrow down the search.")]
        [Output("Request")]
        public static IRequest Request(this Type type, IEnumerable ids)
        {
            List<int> elementIds = new List<int>();
            List<string> uniqueIds = new List<string>();
            if (ids != null)
            {
                foreach (object obj in ids)
                {
                    if (obj is int)
                        elementIds.Add((int)obj);
                    else if (obj is string)
                    {
                        string stringId = (string)obj;
                        int id;
                        if (int.TryParse(stringId, out id))
                            elementIds.Add(id);
                        else
                            uniqueIds.Add(stringId);
                    }
                }
            }

            if (elementIds.Count == 0 && uniqueIds.Count == 0)
                return new FilterRequest() { Type = type };
            else
            {
                ElementIdsRequest elementIdsRequest = new ElementIdsRequest { ElementIds = elementIds };
                UniqueIdsRequest uniqueIdsRequest = new UniqueIdsRequest { UniqueIds = uniqueIds };
                return BH.Engine.Data.Create.LogicalAndRequest(new FilterRequest() { Type = type }, BH.Engine.Data.Create.LogicalOrRequest(elementIdsRequest, uniqueIdsRequest));
            }
        }

        /***************************************************/
    }
}
