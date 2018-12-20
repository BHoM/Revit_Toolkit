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

using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns Query Type of given FilterQuery")]
        [Input("filterQuery", "FilterQuery")]
        [Output("QueryType")]
        public static oM.Adapters.Revit.Enums.QueryType QueryType(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return oM.Adapters.Revit.Enums.QueryType.Undefined;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.QueryType))
                return oM.Adapters.Revit.Enums.QueryType.Undefined;

            if (filterQuery.Equalities[Convert.FilterQuery.QueryType] is oM.Adapters.Revit.Enums.QueryType || filterQuery.Equalities[Convert.FilterQuery.QueryType] is int)
                return (oM.Adapters.Revit.Enums.QueryType)filterQuery.Equalities[Convert.FilterQuery.QueryType];

            return oM.Adapters.Revit.Enums.QueryType.Undefined;
        }

        /***************************************************/
    }
}

