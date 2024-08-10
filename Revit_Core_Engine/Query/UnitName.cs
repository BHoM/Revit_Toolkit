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
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns name of UnitTypeId property that contains a given Revit unit object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("unit", "Unit object to be queried for the correspondent UnitTypeId property name.")]
        [Output("name", "Name of UnitTypeId property that contains the input unit object.")]
#if (REVIT2020)
        public static string UnitName(this DisplayUnitType unit)
#else
        public static string UnitName(this ForgeTypeId unit)
#endif
        {
            if (m_UnitsWithNames == null)
                CollectUnits();

            if (m_UnitsWithNames.ContainsValue(unit))
                return m_UnitsWithNames.FirstOrDefault(x => x.Value.Equals(unit)).Key;
            else
                return null;
        }

        /***************************************************/

        [Description("Returns name of UnitTypeId property that contains given Revit parameter's unit object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("parameter", "Parameter to be queried for the correspondent UnitTypeId property name.")]
        [Output("name", "Name of UnitTypeId property that contains the unit object correspondent to the input parameter.")]
        public static string UnitName(this Parameter parameter)
        {
            return parameter.StorageType == StorageType.Double ? parameter.GetUnitTypeId().UnitName() : null;
        }

        /***************************************************/
    }
}

