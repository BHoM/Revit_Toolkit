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

using BH.Engine.Base;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        //[Description("Filters the given collection of BHoM objects to return only unique Revit elements based on their UniqueId.")]
        //[Input("bHoMObjects", "The collection of BHoM objects to filter.")]
        //[Output("A list of unique BHoM objects.")]
        //public static List<IBHoMObject> FilterUniqueRevitElements(this IEnumerable<IBHoMObject> bHoMObjects)
        //{						
        //    return bHoMObjects
        //        .GroupBy(x => x.UniqueId())
        //        .Select(g => g.First())
        //        .ToList();
        //}

        ///***************************************************/

        //TODO: move to Base_Engine?
        public static IEnumerable<IObject> Unique(this IEnumerable<IObject> objects, BaseComparisonConfig comparisonConfig = null)
        {
            HashSet<string> hashes = new HashSet<string>();
            foreach (IObject obj in objects)
            {
                if (objects == null)
                    continue;

                string hash = obj.Hash(comparisonConfig);
                if (!hashes.Contains(hash))
                {
                    hashes.Add(hash);
                    yield return obj;
                }
            }
        }

        /***************************************************/
    }
}








