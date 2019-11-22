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

using System.Linq;
using System.Collections.Generic;

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****            Deprecated methods             ****/
        /***************************************************/

        [Deprecated("3.0", "The method has been moved to BH.Engine.Adapters.Revit.Query", typeof(BH.Engine.Adapters.Revit.Query), "Discipline")]
        public static Discipline Discipline(this FilterRequest filterRequest, RevitSettings revitSettings)
        {
            Discipline? aDiscipline = null;

            aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(filterRequest);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(filterRequest);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(revitSettings);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            return oM.Adapters.Revit.Enums.Discipline.Undefined;
        }

        /***************************************************/

        [Deprecated("3.0", "The method has been moved to BH.Engine.Adapters.Revit.Query", typeof(BH.Engine.Adapters.Revit.Query), "Discipline")]
        public static Discipline Discipline(this IEnumerable<FilterRequest> filterRequest, RevitSettings revitSettings)
        {
            if (filterRequest == null || filterRequest.Count() == 0)
                return oM.Adapters.Revit.Enums.Discipline.Undefined;

            Discipline? aDiscipline = null;

            foreach (FilterRequest aFilterRequest in filterRequest)
            {
                aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(aFilterRequest);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            foreach (FilterRequest aFilterRequest in filterRequest)
            {
                aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(aFilterRequest);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(revitSettings);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            return oM.Adapters.Revit.Enums.Discipline.Undefined;
        }

        /***************************************************/
    }
}
