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

        [Description("Returns unique parameter ids for all elements of category.")]
        [Input("document", "Document where elements belong.")]
        [Input("category", "Category of the elements.")]
        [Input("instanceParameters", "True to return instance parameter ids, false otherwise.")]
        [Input("typeParameters", "True to return type parameter ids, false otherwise.")]
        [MultiOutput(0, "instanceParameterNames", "Unique ids of the instance parameters for all elements of category.")]
        [MultiOutput(1, "typeParameterNames", "Unique ids of the type parameters for all elements of category.")]
        public static Output<HashSet<int>, HashSet<int>> UniqueParametersIds(this Document document, BuiltInCategory category, bool instanceParameters, bool typeParameters)
        {
            IEnumerable<Element> elements = new FilteredElementCollector(document).OfCategory(category).WhereElementIsNotElementType();
            
            return UniqueParametersIds(elements, instanceParameters, typeParameters);
        }

        /***************************************************/

        [Description("Returns unique parameter ids for the collection of the elements.")]
        [Input("elementsFromOneDocument", "Elements from the same document to get the unique parameter ids from.")]
        [Input("instanceParameters", "True to return instance parameter ids, false otherwise.")]
        [Input("typeParameters", "True to return type parameter ids, false otherwise.")]
        [MultiOutput(0, "instanceParameterNames", "Unique ids of the instance parameters for the collection of the elements.")]
        [MultiOutput(1, "typeParameterNames", "Unique ids of the type parameters for the collection of the elements.")]
        public static Output<HashSet<int>, HashSet<int>> UniqueParametersIds(this IEnumerable<Element> elementsFromOneDocument, bool instanceParameters, bool typeParameters)
        {
            Output<HashSet<int>, HashSet<int>> parameterIds = new Output<HashSet<int>, HashSet<int>>
            {
                Item1 = new HashSet<int>(),
                Item2 = new HashSet<int>()
            };

            if (elementsFromOneDocument == null || !elementsFromOneDocument.Any())
                return parameterIds;

            Document doc = elementsFromOneDocument.FirstOrDefault().Document;
            Dictionary<ElementId, List<Element>> elementsByCategory = elementsFromOneDocument.GroupBy(x => x.Category.Id).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var elementsByCatPair in elementsByCategory)
            {
                List<Element> elementsOfCat = elementsByCatPair.Value;

                if (instanceParameters)
                {
                    HashSet<int> instanceParameterIds = elementsOfCat.UniqueParametersIds();
                    parameterIds.Item1.UnionWith(instanceParameterIds);
                }
                    
                if (typeParameters)
                {
                    IEnumerable<Element> elementTypes = elementsOfCat.UniqueTypeIds().Select(x => doc.GetElement(new ElementId(x)));
                    HashSet<int> typeParametereIds = elementTypes.UniqueParametersIds();
                    parameterIds.Item2.UnionWith(typeParametereIds);
                }
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
