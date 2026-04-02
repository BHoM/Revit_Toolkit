/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

        [Description("Returns unique type parameter ids for all elements of category.")]
        [Input("document", "Document where elements belong.")]
        [Input("category", "Category of the elements.")]
        [Input("includeNotInstantinated", "True to include all element types of category from the model, false for only instanited.")]
        [Output("parameterIds", "Unique parameter ids of types that belong to the input document and category.")]
        public static HashSet<long> UniqueTypeParametersIds(this Document document, BuiltInCategory category, bool includeNotInstantinated = false)
        {
            if (includeNotInstantinated)
                return UniqueInstanceParametersIds(new FilteredElementCollector(document).OfCategory(category).WhereElementIsElementType());
            else
                return UniqueTypeParametersIds(new FilteredElementCollector(document).OfCategory(category).WhereElementIsNotElementType());
        }

        /***************************************************/

        [Description("Returns unique type parameter ids for the collection of the elements.")]
        [Input("elementsFromOneDocument", "Elements from the same document to get the unique type parameter ids from.")]
        [Output("parameterIds", "Unique ids of type parameters for the input collection of elements.")]
        public static HashSet<long> UniqueTypeParametersIds(this IEnumerable<Element> elementsFromOneDocument)
        {
            HashSet<long> parameterIds = new HashSet<long>();

            if (elementsFromOneDocument == null || !elementsFromOneDocument.Any())
                return parameterIds;

            Document doc = elementsFromOneDocument.FirstOrDefault().Document;
            Dictionary<ElementId, List<Element>> elementsByCategory = elementsFromOneDocument.GroupBy(x => x.Category.Id).ToDictionary(x => x.Key, x => x.ToList());

            foreach (KeyValuePair<ElementId, List<Element>> elementsByCatPair in elementsByCategory)
            {
                List<Element> elementsOfCat = elementsByCatPair.Value;
                IEnumerable<Element> elementTypes = elementsOfCat.UniqueTypeIds().Select(x => doc.GetElement(x.ToElementId()));
                parameterIds.UnionWith(elementTypes.UniqueParametersIds());
            }

            return parameterIds;
        }

        /***************************************************/
    }
}


