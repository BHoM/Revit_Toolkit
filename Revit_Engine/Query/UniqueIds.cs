/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns Revit UniqueIds of given BHoMObjects (stored in CustomData).")]
        [Input("bHoMObjects", "Collection of BHoMObjects")]
        [Output("UniqueIds")]
        public static List<string> UniqueIds(IEnumerable<IBHoMObject> bHoMObjects, bool removeNulls = true)
        {
            if (bHoMObjects == null)
                return null;

            List<string> aUniqueIdList = new List<string>();
            foreach (IBHoMObject aBHoMObject in bHoMObjects)
            {
                string aUniqueId = UniqueId(aBHoMObject);
                if (string.IsNullOrEmpty(aUniqueId) && removeNulls)
                    continue;

                aUniqueIdList.Add(aUniqueId);
            }

            return aUniqueIdList;
        }

        /***************************************************/

        [Description("Returns Revit UniqueIds for given filterQuery (Example: SelectionFilterQuery).")]
        [Input("filterQuery", "FilterQuery")]
        [Output("UniqueIds")]
        public static IEnumerable<string> UniqueIds(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.UniqueIds))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.UniqueIds] as IEnumerable<string>;
        }

        /***************************************************/
    }
}
