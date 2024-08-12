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
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [PreviousVersion("7.3", "BH.Revit.Engine.Core.Query.UnitTypeByPropertyName(System.String)")]
        [Description("Returns Revit spec object (enum for Revit up to 2020 or ForgeTypeId for later versions) based on SpecTypeId property name that represents it.")]
        [Input("name", "Name of SpecTypeId property to be queried for the correspondent spec.")]
        [Output("spec", "Spec object under the input SpecTypeId property name.")]
#if (REVIT2020)
        public static UnitType SpecFromName(this string name)
#else
        public static ForgeTypeId SpecFromName(this string name)
#endif
        {
            if (m_SpecsWithNames == null)
                CollectSpecs();

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (m_SpecsWithNames.ContainsKey(name))
                    return m_SpecsWithNames[name];
                else
                    BH.Engine.Base.Compute.RecordWarning($"Spec with identifier {name} not found.");
            }

#if (REVIT2020)
            return UnitType.UT_Undefined;
#else
            return null;
#endif
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

#if (REVIT2020)
        private static void CollectSpecs()
        {
            m_SpecsWithNames = new Dictionary<string, UnitType>();
            foreach (PropertyInfo info in typeof(SpecTypeId).GetProperties())
            {
                if (info.GetGetMethod().GetCustomAttribute<NotImplementedAttribute>() == null)
                {
                    UnitType? unitType = info.GetValue(null) as UnitType?;
                    if (unitType != null && unitType != UnitType.UT_Undefined)
                        m_SpecsWithNames.Add(info.Name, unitType.Value);
                }
            }
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static Dictionary<string, UnitType> m_SpecsWithNames = null;

        /***************************************************/
#else
        private static void CollectSpecs()
        {
            m_SpecsWithNames = new Dictionary<string, ForgeTypeId>();
            foreach (PropertyInfo info in typeof(SpecTypeId).GetProperties())
            {
                ForgeTypeId unitType = info.GetValue(null) as ForgeTypeId;
                if (unitType != null)
                    m_SpecsWithNames.Add(info.Name, unitType);
            }
        }


        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static Dictionary<string, ForgeTypeId> m_SpecsWithNames = null;
#endif

        /***************************************************/
    }
}

