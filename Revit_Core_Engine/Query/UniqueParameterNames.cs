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
        [Input("instanceParameters", "True to return instance parameter ids, false otherwise.")]
        [Input("typeParameters", "True to return type parameter ids, false otherwise.")]
        [MultiOutput(0, "instanceParameterNames", "Unique names of the instance parameters for the collection of the elements.")]
        [MultiOutput(1, "typeParameterNames", "Unique names of the type parameters for the collection of the elements.")]
        public static Output<HashSet<string>, HashSet<string>> UniqueParameterNames(this IEnumerable<Element> elements, bool instanceParameters, bool typeParameters)
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

                Output<HashSet<int>, HashSet<int>> parameterIds = UniqueParametersIds(elements, instanceParameters, typeParameters);
                parameterNames.Item1.UnionWith(parameterIds.Item1.Select(x => doc.ParameterName(x)));
                parameterNames.Item2.UnionWith(parameterIds.Item2.Select(x => doc.ParameterName(x)));
            }

            return parameterNames;
        }

        /***************************************************/
    }
}
