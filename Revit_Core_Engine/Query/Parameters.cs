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

        [Description("Returns all unique parameters of the elements.")]
        [Input("elements", "Elements to get the parameters from.")]
        [Input("includeHiddenParameters", "True for including hidden parameters (not visible in the Revit UI).")]
        [Output("parameters", "Unique parameters of the given elements.")]
        public static List<Parameter> Parameters(this List<Element> elements, bool includeHiddenParameters = false)
        {
            var parameters = new List<Parameter>();

            foreach (var elem in elements)
                parameters.AddRange(elem.Parameters(includeHiddenParameters));

            parameters = parameters.GroupBy(x => x.Id.IntegerValue).Select(x => x.First()).ToList();

            return parameters;
        }

        /***************************************************/

        [Description("Returns all unique parameters of the element.")]
        private static List<Parameter> Parameters(this Element element, bool includeHiddenParameters = false)
        {
            var doc = element.Document;
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