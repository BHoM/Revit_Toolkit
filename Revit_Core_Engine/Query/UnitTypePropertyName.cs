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
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns name of SpecTypeId property that contains a given Revit unit type object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("unitType", "Unit type object to be queried for the correspondent SpecTypeId property name.")]
        [Output("identifier", "Name of SpecTypeId property that contains the input unit type object.")]
#if (REVIT2020)
        public static string UnitTypePropertyName(this UnitType unitType)
#else
        public static string UnitTypePropertyName(this ForgeTypeId unitType)
#endif
        {
            if (m_UnitTypesWithIdentifiers == null)
                CollectUnitTypes();

            if (m_UnitTypesWithIdentifiers.ContainsValue(unitType))
                return m_UnitTypesWithIdentifiers.FirstOrDefault(x => x.Value.Equals(unitType)).Key;
            else
                return null;
        }

        /***************************************************/

        [Description("Returns name of SpecTypeId property that contains given Revit parameter's unit type object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("parameter", "Parameter to be queried for the correspondent SpecTypeId property name.")]
        [Output("identifier", "Name of SpecTypeId property that contains the unit type object correspondent to the input parameter.")]
        public static string UnitTypePropertyName(this Parameter parameter)
        {
            return parameter.StorageType == StorageType.Double ? parameter.Definition.GetDataType().UnitTypePropertyName() : null;
        }

        /***************************************************/

        [Description("Returns name of SpecTypeId property that contains a given Revit unit type object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("unitType", "Unit type object to be queried for the correspondent SpecTypeId property name.")]
        [Output("identifier", "Name of SpecTypeId property that contains the input unit type object.")]
#if (REVIT2020)
        public static string UnitTypePropertyName(this UnitType unitType)
#else
        public static string UnitTypePropertyName2(this ForgeTypeId unitType)
#endif
        {
            if (m_UnitTypesWithIdentifiers2 == null)
                CollectUnitTypes2();

            if (m_UnitTypesWithIdentifiers2.ContainsValue(unitType))
                return m_UnitTypesWithIdentifiers2.FirstOrDefault(x => x.Value.Equals(unitType)).Key;
            else
                return null;
        }

        /***************************************************/

        [Description("Returns name of SpecTypeId property that contains given Revit parameter's unit type object (enum for Revit up to 2020 or ForgeTypeId for later versions).")]
        [Input("parameter", "Parameter to be queried for the correspondent SpecTypeId property name.")]
        [Output("identifier", "Name of SpecTypeId property that contains the unit type object correspondent to the input parameter.")]
        public static string UnitTypePropertyName2(this Parameter parameter)
        {
            return parameter.StorageType == StorageType.Double ? parameter.GetUnitTypeId().UnitTypePropertyName2() : null;
        }

        /***************************************************/
        private static void CollectUnitTypes2()
        {
            m_UnitTypesWithIdentifiers2 = new Dictionary<string, ForgeTypeId>();
            foreach (PropertyInfo info in typeof(UnitTypeId).GetProperties())
            {
                ForgeTypeId unitType = info.GetValue(null) as ForgeTypeId;
                if (unitType != null)
                    m_UnitTypesWithIdentifiers2.Add(info.Name, unitType);
            }
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static Dictionary<string, ForgeTypeId> m_UnitTypesWithIdentifiers2 = null;
    }
}

