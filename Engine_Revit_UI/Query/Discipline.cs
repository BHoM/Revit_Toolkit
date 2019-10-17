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

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public Discipline Discipline(this IRequest request, RevitSettings revitSettings)
        {
            Discipline? aDiscipline = null;

            if (!(request is FilterRequest))
                return oM.Adapters.Revit.Enums.Discipline.Undefined;

            FilterRequest aFilterRequest = (FilterRequest)request;

            aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(aFilterRequest);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(aFilterRequest);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(revitSettings);
            if (aDiscipline != null && aDiscipline.HasValue)
                return aDiscipline.Value;

            return oM.Adapters.Revit.Enums.Discipline.Undefined;
        }

        /***************************************************/

        static public Discipline Discipline(this IEnumerable<IRequest> request, RevitSettings revitSettings)
        {
            if (request == null || request.Count() == 0)
                return oM.Adapters.Revit.Enums.Discipline.Undefined;

            Discipline? aDiscipline = null;

            foreach (IRequest aRequest in request)
            {
                aDiscipline = BH.Engine.Adapters.Revit.Query.Discipline(aRequest);
                if (aDiscipline != null && aDiscipline.HasValue)
                    return aDiscipline.Value;
            }

            foreach (IRequest aRequest in request)
            {
                aDiscipline = BH.Engine.Adapters.Revit.Query.DefaultDiscipline(aRequest);
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
