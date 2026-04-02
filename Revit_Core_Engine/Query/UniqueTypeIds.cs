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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts element types of the input elements and returns a set of unique integer values of their ElementIds.")]
        [Input("elementsFromOneDocument", "Elements to get the unique types from. Should belong to a single document to avoid ambiguity between same ElementIds in different documents.")]
        [Output("ids", "Unique element type ids for the collection of the elements.")]
        public static HashSet<long> UniqueTypeIds(this IEnumerable<Element> elementsFromOneDocument)
        {
            HashSet<long> ids = new HashSet<long>();

            foreach (Element element in elementsFromOneDocument)
            {
                long elementTypeId = element.GetTypeId().Value();

                if (elementTypeId != -1)
                    ids.Add(elementTypeId);
            }

            return ids;
        }

        /***************************************************/
    }
}


