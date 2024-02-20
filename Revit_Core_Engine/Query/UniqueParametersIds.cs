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

        [Description("Returns unique parameter ids for the collection of the elements.")]
        [Input("elementsFromOneDocument", "Elements to get the unique parameter ids from.")]
        [Output("ids", "Unique ids for the collection of the elements.")]
        public static HashSet<int> UniqueParametersIds(this IEnumerable<Element> elementsFromOneDocument)
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
