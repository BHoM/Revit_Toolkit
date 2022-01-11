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

        [Description("Filters ElementIds of elements and types in a Revit document based on a collection of strings that represent Revit UniqueIds.")]
        [Input("document", "Revit document to be processed.")]
        [Input("uniqueIds", "Collection of strings representing Revit UniqueIds.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByUniqueIds(this Document document, IEnumerable<string> uniqueIds, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            if (ids != null && ids.Count() == 0)
                return result;

            if (uniqueIds != null)
            {
                HashSet<string> corruptIds = new HashSet<string>();
                foreach (string uniqueID in uniqueIds)
                {
                    if (!string.IsNullOrEmpty(uniqueID))
                    {
                        Element element = document.GetElement(uniqueID);
                        if (element != null)
                            result.Add(element.Id);
                        else
                            corruptIds.Add(uniqueID);
                    }
                    else
                        BH.Engine.Base.Compute.RecordError("An attempt to use empty Unique Revit Id has been found.");
                }

                if (corruptIds.Count != 0)
                    BH.Engine.Base.Compute.RecordError(String.Format("Elements have not been found in the document. Unique Revit Ids: {0}", string.Join(", ", corruptIds)));

                if (ids != null)
                    result.IntersectWith(ids);
            }

            return result;
        }

        /***************************************************/
    }
}

