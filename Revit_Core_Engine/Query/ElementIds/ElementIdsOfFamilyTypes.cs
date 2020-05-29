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

using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;
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

        [Description("Filters ElementIds of Revit family types that belong to a given Revit family.")]
        [Input("document", "Revit document to be processed.")]
        [Input("familyId", "ElementId of the Revit family, to which filtered family types belong.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfFamilyTypes(this Document document, int familyId, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            if (ids != null && ids.Count() == 0)
                return new List<ElementId>();

            Family family = document.GetElement(new ElementId(familyId)) as Family;
            if (family == null)
            {
                BH.Engine.Reflection.Compute.RecordError(String.Format("Couldn't find a Family under ElementId {0}", familyId));
                return new List<ElementId>();
            }

            HashSet<ElementId> result = new HashSet<ElementId>(family.GetFamilySymbolIds());
            if (ids != null)
                result.IntersectWith(ids);

            return result;
        }

        /***************************************************/

        [Description("Filters ElementIds of Revit family types based on names of theirs and their parent family, with option to loose the search by leaving one or both of the input names blank.")]
        [Input("document", "Revit document to be processed.")]
        [Input("familyName", "Name of Revit family to be used as a filter. Optional: if left blank, all families will be filtered in search for specified family type name.")]
        [Input("familyTypeName", "Optional, the name of Revit family type to be used to narrow down the search. If left blank, all types within family will be returned. If family name is left blank too, all family types in model will be returned.")]
        [Input("caseSensitive", "If true: only perfect, case sensitive text match will be accepted. If false: capitals and small letters will be treated as equal.")]
        [Input("ids", "Optional, allows narrowing the search: if not null, the output will be an intersection of this collection and ElementIds filtered by the query.")]
        [Output("elementIds", "Collection of filtered ElementIds.")]
        public static IEnumerable<ElementId> ElementIdsOfFamilyTypes(this Document document, string familyName = null, string familyTypeName = null, bool caseSensitive = true, IEnumerable<ElementId> ids = null)
        {
            if (document == null)
                return null;

            HashSet<ElementId> result = new HashSet<ElementId>();
            IEnumerable<ElementType> elementTypes = new FilteredElementCollector(document).OfClass(typeof(ElementType)).Cast<ElementType>();
            if (!string.IsNullOrEmpty(familyName))
            {
                if (caseSensitive)
                    elementTypes = elementTypes.Where(x => x.FamilyName == familyName);
                else
                    elementTypes = elementTypes.Where(x => !string.IsNullOrEmpty(x.FamilyName) && x.FamilyName.ToUpper() == familyName.ToUpper());

                if (elementTypes.Count() == 0)
                {
                    BH.Engine.Reflection.Compute.RecordError("Couldn't find any family named " + familyName + ".");
                    return result;
                }
            }

            if (!string.IsNullOrEmpty(familyTypeName))
            {
                if (caseSensitive)
                    elementTypes = elementTypes.Where(x => x.Name == familyTypeName);
                else
                    elementTypes = elementTypes.Where(x => !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper() == familyTypeName.ToUpper());

                if (elementTypes.Count() > 1)
                    BH.Engine.Reflection.Compute.RecordWarning(String.Format("More than one family type named {0} has been found. It may be desirable to narrow down the search by specifying family name explicitly.", familyTypeName));
            }

            if (elementTypes.Count() == 0)
            {
                string error = "Couldn't find any family type named " + familyTypeName;
                if (!string.IsNullOrEmpty(familyName))
                    error += " in the family " + familyName + ".";
                else
                    error += ".";

                BH.Engine.Reflection.Compute.RecordError(error);
                return result;
            }

            result = new HashSet<ElementId>(elementTypes.Select(x => x.Id));
            if (ids != null)
                result.IntersectWith(ids);

            return result;
        }

        /***************************************************/
    }
}
