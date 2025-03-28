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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
#if REVIT2021 || REVIT2022 || REVIT2023 || REVIT2024
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets a BuiltInParameterGroup by its name in the UI.")]
        [Input("groupName", "Name in the UI of a Revit parameter group.")]
        [Output("builtInParameterGroup", "A BuiltInParameterGroup that has the input name.")]
        public static BuiltInParameterGroup GroupFromName(this string groupName)
        {
            BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.INVALID;
            foreach (BuiltInParameterGroup bpg in Enum.GetValues(typeof(BuiltInParameterGroup)))
            {
                if (LabelUtils.GetLabelFor(bpg) == groupName)
                {
                    parameterGroup = bpg;
                    break;
                }
            }

            return parameterGroup;
        }
#else
        /***************************************************/
        /****              Private fields               ****/
        /***************************************************/

        private static Dictionary<string, ForgeTypeId> m_GroupTypeByName = new Dictionary<string, ForgeTypeId>();

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the GroupTypeId of a Revit parameter group by its name in the UI.")]
        [Input("groupName", "Name in the UI of a Revit parameter group.")]
        [Output("groupTypeId", "GroupTypeId of a Revit parameter group that has the input name.")]
        public static ForgeTypeId GroupFromName(this string groupName)
        {
            if (!m_GroupTypeByName.Any())
            {
                Type type = typeof(GroupTypeId);
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

                foreach (PropertyInfo property in properties)
                {
                    ForgeTypeId value = property.GetValue(null) as ForgeTypeId;
                    string name = LabelUtils.GetLabelForGroup(value);
                    m_GroupTypeByName[name] = value;
                }
            }

            if (m_GroupTypeByName.TryGetValue(groupName, out ForgeTypeId groupTypeId))
                return groupTypeId;
            else
                return null;
        }
#endif

        /***************************************************/
    }
}
