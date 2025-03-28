/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

        [Description("Returns ForgeTypeId based on UnitTypeId property name that represents it.")]
        [Input("name", "Name of UnitTypeId property to be queried for the correspondent unit.")]
        [Output("unit", "Unit object under the input UnitTypeId property name.")]
        public static ForgeTypeId UnitFromName(this string name)
        {
            if (m_UnitsWithNames == null)
                CollectUnits();

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (m_UnitsWithNames.ContainsKey(name))
                    return m_UnitsWithNames[name];
                else
                    BH.Engine.Base.Compute.RecordWarning($"Unit with identifier {name} not found.");
            }

            return null;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static void CollectUnits()
        {
            BH.Engine.Base.Compute.StartSuppressRecordingEvents(false, true, true);

            m_UnitsWithNames = new Dictionary<string, ForgeTypeId>();
            foreach (PropertyInfo info in typeof(UnitTypeId).GetProperties())
            {
                ForgeTypeId unitType = info.GetValue(null) as ForgeTypeId;
                if (unitType != null)
                    m_UnitsWithNames.Add(info.Name, unitType);
            }

            BH.Engine.Base.Compute.StopSuppressRecordingEvents();
        }

        /***************************************************/
        /****               Private fields              ****/
        /***************************************************/

        private static Dictionary<string, ForgeTypeId> m_UnitsWithNames = null;

        /***************************************************/
    }
}


