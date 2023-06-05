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
using BH.Engine.Data;
using BH.oM.Base.Attributes;
using BH.oM.Graphics.Misc;
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

        [Description("Returns all unique type parameters of the elements.")]
        [Input("elements", "Elements to get the parameters from.")]
        [Input("includeHiddenParameters", "True for including hidden parameters (not displayed in the Revit UI).")]
        [Output("parameters", "Unique type parameters of the given elements.")]
        public static List<Parameter> TypeParameters(this List<Element> elements, bool includeHiddenParameters = true)
        {
            if (elements == null || elements.Count == 0)
                return null;

            HashSet<int> elementTypeIds = new HashSet<int>();

            foreach (Element elem in elements)
            {
                ElementId elTypeId = elem.GetTypeId();

                if (elTypeId.IntegerValue != -1)
                    elementTypeIds.Add(elTypeId.IntegerValue);
            }

            Document doc = elements[0].Document;
            List<Element> elementTypes = elementTypeIds.Select(x => doc.GetElement(new ElementId(x))).ToList();

            return elementTypes.Parameters(includeHiddenParameters);
        }

        /***************************************************/

        [Description("Returns all unique parameters of the elements.")]
        [Input("elements", "Elements to get the parameters from.")]
        [Input("includeHiddenParameters", "True for including hidden parameters (not displayed in the Revit UI).")]
        [Output("parameters", "Unique parameters of the given elements.")]
        public static List<Parameter> Parameters(this List<Element> elements, bool includeHiddenParameters = true)
        {
            HashSet<int> ids = new HashSet<int>();
            List<Parameter> parameters = new List<Parameter>();

            foreach (Element elem in elements)
            {
                foreach (Parameter param in elem.Parameters(includeHiddenParameters))
                {
                    if (ids.Add(param.Id.IntegerValue))
                        parameters.Add(param);
                }
            }

            return parameters;
        }

        /***************************************************/

        [Description("Returns all unique parameters of the element.")]
        [Input("elements", "Element to get the parameters from.")]
        [Input("includeHiddenParameters", "True for including hidden parameters (not displayed in the Revit UI).")]
        [Output("parameters", "Unique parameters of the given elements.")]
        public static List<Parameter> Parameters(this Element element, bool includeHiddenParameters = true)
        {
            var parameters = new List<Parameter>();

            if (includeHiddenParameters)
            {
                foreach (Parameter param in element.Parameters)
                    parameters.Add(param);
            }
            else
            {
                foreach (Parameter param in element.ParametersMap)
                    parameters.Add(param);
            }

            return parameters;
        }

        /***************************************************/
    }
}