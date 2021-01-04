/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements of given element type in a Revit document.")]
        [Input("document", "Revit document to be processed.")]
        [Input("elementTypeId", "ElementId of the family type to be sought for.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByElementType(this Document document, int elementTypeId, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            ElementType elementType = document.GetElement(new ElementId(elementTypeId)) as ElementType;
            if (elementType == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Active Revit model does not contain a family type under ElementId {0}.", elementTypeId));
                return new List<ElementId>();
            }

            return document.ElementIdsByElementType(elementType, ids);
        }

        /***************************************************/
        
        [Description("Filters ElementIds of elements of given element type in a Revit document.")]
        [Input("document", "Revit document to be processed.")]
        [Input("elementType", "Family type to be sought for.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByElementType(this Document document, ElementType elementType, IEnumerable<ElementId> ids = null)
        {
            List<ElementId> result = new List<ElementId>();
            FilteredElementCollector collector = ids == null ? new FilteredElementCollector(document) : new FilteredElementCollector(document, ids.ToList());

            if (elementType is FamilySymbol)
                result.AddRange(collector.WherePasses(new FamilyInstanceFilter(document, elementType.Id)).ToElementIds());
            else
            {
                Type instanceType = elementType.InstanceType();
                if (instanceType != null)
                    collector = collector.OfClass(instanceType);
                else
                    collector = collector.WhereElementIsNotElementType();

                result.AddRange(collector.Where(x => x.GetTypeId() == elementType.Id).Select(x => x.Id));
            }

            return result;
        }

        /***************************************************/
    }
}
