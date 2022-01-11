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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on a collection of objects shipped as a part of FilterRequest.")]
        [Input("document", "Revit document to be processed.")]
        [Input("idObjects", "Collection of objects that are either strings representing Revit UniqueIds or integers representing Revit ElementIds.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByIdObjects(this Document document, IList idObjects, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            List<int> elementIds = new List<int>();
            List<string> uniqueIds = new List<string>();
            if (idObjects != null && idObjects.Count != 0)
            {
                foreach (object obj in idObjects)
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
            else
                return ids;
            
            HashSet<ElementId> result = new HashSet<ElementId>();
            result.UnionWith(document.ElementIdsByInts(elementIds, ids));
            result.UnionWith(document.ElementIdsByUniqueIds(uniqueIds, ids));
            return result;
        }

        /***************************************************/
    }
}

