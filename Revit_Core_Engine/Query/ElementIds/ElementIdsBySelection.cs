/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using Autodesk.Revit.UI;
using BH.oM.Base.Attributes;
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

        [Description("Filters ElementIds of currently selected elements and types in a Revit document.")]
        [Input("uIDocument", "Revit UI document to be processed.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsBySelection(this UIDocument uIDocument, IEnumerable<ElementId> ids = null)
        {
            if (uIDocument == null || uIDocument.Document == null)
                return null;

            if (uIDocument.Selection == null)
                return new List<ElementId>();

            HashSet<ElementId> result = new HashSet<ElementId>(uIDocument.Selection.GetElementIds());
            foreach (ElementId elementId in new List<ElementId>(result))
            {
                Element element = uIDocument.Document.GetElement(elementId);
                if (element is Group)
                    result.UnionWith(((Group)element).ElementIdsOfMemberElements());
            }

            if (ids != null)
                result.IntersectWith(ids);

            return result;
        }

        /***************************************************/
    }
}


