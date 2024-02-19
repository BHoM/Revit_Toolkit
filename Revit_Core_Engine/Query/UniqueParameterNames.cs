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

        [Description("Returns unique parameter names for the collection of the elements.")]
        [Input("elements", "Elements to get the parameter names from.")]
        [MultiOutput(0, "instanceParameterNames", "Unique names of the instance parameters for the collection of the elements.")]
        [MultiOutput(1, "typeParameterNames", "Unique names of the type parameters for the collection of the elements.")]
        public static Output<HashSet<string>, HashSet<string>> UniqueParameterNames(this IEnumerable<Element> elements)
        {
            Output<HashSet<string>, HashSet<string>> parameterNames = new Output<HashSet<string>, HashSet<string>>
            {
                Item1 = new HashSet<string>(),
                Item2 = new HashSet<string>()
            };

            if (elements == null || !elements.Any())
                return parameterNames;

            Dictionary<Document, List<Element>> elementsByDocument = elements.GroupBy(x => x.Document).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var elementsInDocPair in elementsByDocument)
            {
                Document doc = elementsInDocPair.Key;
                List<Element> elementsFromOneDocument = elementsInDocPair.Value;
                Dictionary<ElementId, List<Element>> elementsByCategory = elementsFromOneDocument.GroupBy(x => x.Category.Id).ToDictionary(x => x.Key, x => x.ToList());

                foreach (var elementsByCatPair in elementsByCategory)
                {
                    List<Element> elementsOfCat = elementsByCatPair.Value;
                    HashSet<int> instanceParameterIds = elementsOfCat.UniqueParametersIds();
                    IEnumerable<string> instanceParameterNames = instanceParameterIds.Select(x => doc.ParameterName(x));
                    parameterNames.Item1.UnionWith(instanceParameterNames);

                    IEnumerable<Element> elementTypes = elementsOfCat.UniqueTypeIds().Select(x => doc.GetElement(new ElementId(x)));
                    HashSet<int> typeParametereIds = elementTypes.UniqueParametersIds();
                    IEnumerable<string> typeParameterNames = typeParametereIds.Select(x => doc.ParameterName(x));
                    parameterNames.Item2.UnionWith(typeParameterNames);
                }
            }

            return parameterNames;
        }

        /***************************************************/

        [Description("Creates dictionary for elements per category.")]
        private static Dictionary<ElementId, List<Element>> ElementsByCategory(this IEnumerable<Element> elements)
        {
            Dictionary<ElementId, List<Element>> elementsByCat = new Dictionary<ElementId, List<Element>>();

            foreach (Element element in elements)
            {
                if (elementsByCat.ContainsKey(element.Category.Id))
                    elementsByCat[element.Category.Id].Add(element);
                else
                    elementsByCat.Add(element.Category.Id, new List<Element> { element });
            }

            return elementsByCat;
        }

        /***************************************************/

        [Description("Returns unique parameter ids for the collection of the elements.")]
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

        [Description("Returns unique element types for the collection of the elements.")]
        private static HashSet<int> UniqueTypeIds(this IEnumerable<Element> elementsFromOneDocument)
        {
            HashSet<int> ids = new HashSet<int>();

            foreach (Element element in elementsFromOneDocument)
            {
                int elementTypeId = element.GetTypeId().IntegerValue;

                if (elementTypeId != -1)
                    ids.Add(elementTypeId);
            }

            return ids;
        }

        /***************************************************/

        [Description("Returns name of the paramter based on id and document it belongs.")]
        private static string ParameterName(this Document doc, int id)
        {
            string parameterName;

            if (id < 0)
                parameterName = LabelUtils.GetLabelFor((BuiltInParameter)id);
            else
                parameterName = (doc.GetElement(new ElementId(id)) as ParameterElement).Name;

            return parameterName;
        }

        /***************************************************/
    }
}
