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

using System;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Data.Requests;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Gets Discipline for given BHoM Type.")]
        [Input("type", "BHoM Type")]
        [Output("Discipline")]
        public static Discipline? Discipline(this Type type)
        {
            if (type == null)
                return null;

            if(type.Namespace.StartsWith("BH.oM.Structure"))
                return oM.Adapters.Revit.Enums.Discipline.Structural;

            if (type.Namespace.StartsWith("BH.oM.Environment"))
                return oM.Adapters.Revit.Enums.Discipline.Environmental;

            if (type.Namespace.StartsWith("BH.oM.Architecture"))
                return oM.Adapters.Revit.Enums.Discipline.Architecture;

            if (type.Namespace.StartsWith("BH.oM.Physical"))
                return oM.Adapters.Revit.Enums.Discipline.Physical;

            return null;
        }

        /***************************************************/

        [Description("Gets Discipline for given FilterRequest.")]
        [Input("filterQuery", "FilterRequest")]
        [Output("Discipline")]
        public static Discipline? Discipline(this FilterRequest filterQuery)
        {
            if (filterQuery == null)
                return null;

            List<Discipline> aDisciplineList = new List<Discipline>();

            IEnumerable<FilterRequest> aFilterQueries = Query.FilterRequests(filterQuery);
            if (aFilterQueries != null && aFilterQueries.Count() > 0)
            {
                foreach (FilterRequest aFilterRequest in aFilterQueries)
                {
                    Discipline? aDiscipline = Discipline(filterQuery);
                    if (aDiscipline != null && aDiscipline.HasValue)
                        return aDiscipline;
                }
            }
            else
            {
                Discipline? aDiscipline = Discipline(filterQuery.Type);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;

                aDiscipline = DefaultDiscipline(filterQuery);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            return null;
        }

        /***************************************************/
    }
}