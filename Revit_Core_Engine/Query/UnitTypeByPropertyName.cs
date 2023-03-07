/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.Reflection;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns Revit unit type object (enum for Revit up to 2020 or ForgeTypeId for later versions) based on SpecTypeId property name that represents it.")]
        [Input("identifier", "Name of SpecTypeId property to be queried for the correspondent unit type.")]
        [Output("unitType", "Unit type object under the input SpecTypeId property name.")]
#if (REVIT2018 || REVIT2019 || REVIT2020)
        public static UnitType UnitTypeByPropertyName(this string identifier)
#else
        public static ForgeTypeId UnitTypeByPropertyName(this string identifier)
#endif
        {
            if (m_UnitTypesWithIdentifiers == null)
                CollectUnitTypes();

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                if (m_UnitTypesWithIdentifiers.ContainsKey(identifier))
                    return m_UnitTypesWithIdentifiers[identifier];
                else
                    BH.Engine.Base.Compute.RecordWarning($"Unit type with identifier {identifier} not found.");
            }

#if (REVIT2018 || REVIT2019 || REVIT2020)
            return UnitType.UT_Undefined;
#else
            return null;
#endif
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

#if (REVIT2018 || REVIT2019 || REVIT2020)
        private static void CollectUnitTypes()
        {
            m_UnitTypesWithIdentifiers = new Dictionary<string, UnitType>();
            foreach (PropertyInfo info in typeof(SpecTypeId).GetProperties())
            {
                UnitType? unitType = info.GetValue(null) as UnitType?;
                if (unitType != null && unitType != UnitType.UT_Undefined)
                    m_UnitTypesWithIdentifiers.Add(info.Name, unitType.Value);
            }
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static Dictionary<string, UnitType> m_UnitTypesWithIdentifiers = null;

        /***************************************************/
#else
        private static void CollectUnitTypes()
        {
            m_UnitTypesWithIdentifiers = new Dictionary<string, ForgeTypeId>();
            foreach (PropertyInfo info in typeof(SpecTypeId).GetProperties())
            {
                ForgeTypeId unitType = info.GetValue(null) as ForgeTypeId;
                if (unitType != null)
                    m_UnitTypesWithIdentifiers.Add(info.Name, unitType);
            }
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static Dictionary<string, ForgeTypeId> m_UnitTypesWithIdentifiers = null;
#endif

        /***************************************************/
    }
}
