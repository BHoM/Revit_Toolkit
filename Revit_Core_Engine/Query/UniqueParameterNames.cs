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
        [Output("paramterNames", "Output containing instance parameter names as Item1 and type paramter names as Item2.")]
        public static Output<HashSet<string>, HashSet<string>> UniqueParametersNames(this IEnumerable<Element> elements)
        {
            Output<HashSet<string>, HashSet<string>> parameterNames = new Output<HashSet<string>, HashSet<string>>
            {
                Item1 = new HashSet<string>(),
                Item2 = new HashSet<string>()
            };

            if (elements == null || !elements.Any())
                return parameterNames;

            Dictionary<Document, List<Element>> elementsByDocumentDictionary = elements.ElementsByDocumentDictionary();

            foreach (var elementsInDocPair in elementsByDocumentDictionary)
            {
                Document doc = elementsInDocPair.Key;
                List<Element> elementsFromOneDocument = elementsInDocPair.Value;

                if (!elementsFromOneDocument.Any())
                    continue;

                Dictionary<ElementId, List<Element>> elementsByCategoryDictionary = elementsFromOneDocument.ElementsByCategoryDictionary();

                foreach (var elementsByCat in elementsByCategoryDictionary)
                {
                    List<Element> elementsOfCat = elementsByCat.Value;

                    if (!elementsOfCat.Any()) 
                        continue;

                    HashSet<int> instanceParameterIds = elementsOfCat.UniqueParametersIds();
                    IEnumerable<string> instanceParameterNames = instanceParameterIds.Select(x => doc.ParameterName(x));

                    foreach (string name in instanceParameterNames)
                        parameterNames.Item1.Add(name);

                    IEnumerable<Element> elementTypes = elementsOfCat.UniqueTypeIds().Select(x => doc.GetElement(new ElementId(x)));
                    HashSet<int> typeParametereIds = elementTypes.UniqueParametersIds();
                    IEnumerable<string> typeParameterNames = typeParametereIds.Select(x => doc.ParameterName(x));

                    foreach (string name in typeParameterNames)
                        parameterNames.Item2.Add(name);
                }
            }

            return parameterNames;
        }

        /***************************************************/

        [Description("Creates dictionary for elements per document.")]
        private static Dictionary<Document, List<Element>> ElementsByDocumentDictionary(this IEnumerable<Element> elements)
        {
            Dictionary<Document, List<Element>> elementsByDoc = new Dictionary<Document, List<Element>>();

            foreach (Element element in elements)
            {
                if (elementsByDoc.ContainsKey(element.Document))
                    elementsByDoc[element.Document].Add(element);
                else
                    elementsByDoc.Add(element.Document, new List<Element> { element });
            }

            return elementsByDoc;
        }

        /***************************************************/

        [Description("Creates dictionary for elements per category.")]
        private static Dictionary<ElementId, List<Element>> ElementsByCategoryDictionary(this IEnumerable<Element> elements)
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
            
            foreach (Element element in elementsFromOneDocument)
            {
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
            Document doc = elementsFromOneDocument.FirstOrDefault().Document;

            foreach (Element element in elementsFromOneDocument)
            {
                Element elementType = doc.GetElement(element.GetTypeId());

                if (elementType != null)
                    ids.Add(elementType.Id.IntegerValue);
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
                parameterName = (doc.GetElement(new ElementId(id)) as ParameterElement).GetDefinition().Name;

            return parameterName;
        }

        /***************************************************/
    }
}
