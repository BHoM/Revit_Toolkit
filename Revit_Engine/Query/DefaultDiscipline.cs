/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Enums;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets Default Discipline for given RevitSettings.")]
        [Input("revitSettings", "RevitSettings")]
        [Output("Discipline")]
        public static Discipline DefaultDiscipline(this RevitSettings revitSettings)
        {
            if (revitSettings == null || revitSettings.GeneralSettings == null)
                return oM.Adapters.Revit.Enums.Discipline.Undefined;
            else
                return revitSettings.GeneralSettings.DefaultDiscipline;
        }

        /***************************************************/

        [Description("Gets Default Discipline for given FilterRequest.")]
        [Input("filterRequest", "FilterRequest")]
        [Output("Discipline")]
        public static Discipline DefaultDiscipline(this FilterRequest filterRequest)
        {
            if (filterRequest == null || !filterRequest.Equalities.ContainsKey(Convert.FilterRequest.DefaultDiscipline))
                return oM.Adapters.Revit.Enums.Discipline.Undefined;
            else
                return (Discipline)filterRequest.Equalities[Convert.FilterRequest.DefaultDiscipline];
        }

        /***************************************************/
    }
}
