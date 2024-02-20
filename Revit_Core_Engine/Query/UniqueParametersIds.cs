/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Base;
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

        [Description("Returns unique instance parameter ids for all elements of category.")]
        [Input("document", "Document where elements belong.")]
        [Input("category", "Category of the elements.")]
        [Output("parameterIds", "Unique instance parameters ids for the collection of the elements.")]
        public static HashSet<int> UniqueInstanceParametersIds(this Document document, BuiltInCategory category)
        {
            IEnumerable<Element> elements = new FilteredElementCollector(document).OfCategory(category).WhereElementIsNotElementType();
            
            return UniqueInstanceParametersIds(elements);
        }

        /***************************************************/

        [Description("Returns unique type parameter ids for all elements of category.")]
        [Input("document", "Document where elements belong.")]
        [Input("category", "Category of the elements.")]
        [Output("parameterIds", "Unique type parameters ids for the collection of the elements.")]
        public static HashSet<int> UniqueTypeParametersIds(this Document document, BuiltInCategory category)
        {
            IEnumerable<Element> elements = new FilteredElementCollector(document).OfCategory(category).WhereElementIsNotElementType();

            return UniqueTypeParametersIds(elements);
        }

        /***************************************************/

        [Description("Returns unique instance parameter ids for the collection of the elements.")]
        [Input("elementsFromOneDocument", "Elements from the same document to get the unique parameter ids from.")]
        [Output("parameterIds", "Unique ids of the instance parameters for the collection of the elements.")]
        public static HashSet<int> UniqueInstanceParametersIds(this IEnumerable<Element> elementsFromOneDocument)
        {
            HashSet<int> parameterIds = new HashSet<int>();

            if (elementsFromOneDocument == null || !elementsFromOneDocument.Any())
                return parameterIds;

            Document doc = elementsFromOneDocument.FirstOrDefault().Document;
            Dictionary<ElementId, List<Element>> elementsByCategory = elementsFromOneDocument.GroupBy(x => x.Category.Id).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var elementsByCatPair in elementsByCategory)
            {
                List<Element> elementsOfCat = elementsByCatPair.Value;
                parameterIds.UnionWith(elementsOfCat.UniqueParametersIds());
            }

            return parameterIds;
        }

        /***************************************************/

        [Description("Returns unique type parameter ids for the collection of the elements.")]
        [Input("elementsFromOneDocument", "Elements from the same document to get the unique type parameter ids from.")]
        [Output("parameterIds", "Unique ids of the instance parameters for the collection of the elements.")]
        public static HashSet<int> UniqueTypeParametersIds(this IEnumerable<Element> elementsFromOneDocument)
        {
            HashSet<int> parameterIds = new HashSet<int>();

            if (elementsFromOneDocument == null || !elementsFromOneDocument.Any())
                return parameterIds;

            Document doc = elementsFromOneDocument.FirstOrDefault().Document;
            Dictionary<ElementId, List<Element>> elementsByCategory = elementsFromOneDocument.GroupBy(x => x.Category.Id).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var elementsByCatPair in elementsByCategory)
            {
                List<Element> elementsOfCat = elementsByCatPair.Value;
                IEnumerable<Element> elementTypes = elementsOfCat.UniqueTypeIds().Select(x => doc.GetElement(new ElementId(x)));
                parameterIds.UnionWith(elementTypes.UniqueParametersIds());
            }

            return parameterIds;
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        [Description("Returns unique parameter ids for the collection of the elements.")]
        [Input("elementsFromOneDocument", "Elements to get the unique parameter ids from.")]
        [Output("ids", "Unique ids for the collection of the elements.")]
        private static HashSet<int> UniqueParametersIds(this IEnumerable<Element> elementsFromOneDocument)
        {
            HashSet<int> ids = new HashSet<int>();
            HashSet<string> visitedFamilies = new HashSet<string>();

            foreach (Element element in elementsFromOneDocument)
            {
                if (element is ElementType)
                {
                    string familyName = ((ElementType)element).FamilyName;

                    if (visitedFamilies.Contains(familyName))
                        continue;
                    else
                        visitedFamilies.Add(familyName);
                }

                foreach (Parameter parameter in element.ParametersMap)
                {
                    ids.Add(parameter.Id.IntegerValue);
                }
            }

            return ids;
        }

        /***************************************************/
    }
}
