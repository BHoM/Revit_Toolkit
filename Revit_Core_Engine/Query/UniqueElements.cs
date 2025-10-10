/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns a list of unique elements from the input collection, removing duplicates based on document path and element ID.")]
        [Input("elements", "Collection of Revit elements to filter for uniqueness.")]
        [Output("uniqueElements", "List of unique elements with duplicates removed.")]
        public static List<Element> UniqueElements(this List<Element> elements)
        {
            HashSet<string> uniqueElementsKeys = new HashSet<string>();
            List<Element> uniqueElements = new List<Element>();

            foreach (Element element in elements)
            {
                if (uniqueElementsKeys.Add(element.UniqueKey()))
                {
                    uniqueElements.Add(element);
                }
            }

            return uniqueElements;
        }

        /***************************************************/

        private static string UniqueKey(this Element element)
        {
            if (element == null)
                return string.Empty;

            return $"{element.Document.PathName}-{element.Id}";
        }

        /***************************************************/
    }
}

