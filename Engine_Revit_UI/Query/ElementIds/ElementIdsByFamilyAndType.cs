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

using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Filters ElementIds of elements and types in a Revit document based on Revit family criterion, optionally narrowing the search to a specific family type.")]
        [Input("document", "Revit document to be processed.")]
        [Input("familyName", "Name of the Revit family to be used as a filter.")]
        [Input("familyTypeName", "Optional, the name of Revit family type to be used to narrow down the search.")]
        [Input("caseSensitive", "If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsByFamilyAndType(this Document document, string familyName, string familyTypeName = null, bool caseSensitive = true, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            List<ElementId> result = new List<ElementId>();
            if (string.IsNullOrEmpty(familyName) && string.IsNullOrEmpty(familyTypeName))
            {
                BH.Engine.Reflection.Compute.RecordError("Family type query could not be executed because neither family name nor family type name has been provided.");
                return result;
            }

            foreach (ElementId elementId in document.ElementIdsOfFamilyTypes(familyName, familyTypeName, caseSensitive))
            {
                ElementType elementType = (ElementType)document.GetElement(elementId);
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
            }

            return result;
        }

        /***************************************************/
    }
}