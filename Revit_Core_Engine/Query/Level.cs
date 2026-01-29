/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

        [Description("Returns the level that the element is assigned to. Checks LevelId property first, then level parameters, and finally work plane-based host level for family instances.")]
        [Input("element", "Element to get the level from.")]
        [Output("level", "Level that the element is assigned to, or null if not found.")]
        public static Level Level(this Element element)
        {
            // Null check
            if (element == null)
                return null;

            Document doc = element.Document;
            ElementId levelId = element.LevelId;

            // First priority: Check LevelId property (most direct way to get level)
            if (levelId.Value() != -1)
                return doc.GetElement(levelId) as Level;

            // Second priority: Check level parameters (LevelParameter or BaseLevelParameter)
            Parameter levelParameter = element.LevelParameter() ?? element.BaseLevelParameter();
            if (levelParameter != null)
                return doc.GetElement(levelParameter.AsElementId()) as Level;

            // Third priority: Check if element is work plane-based and hosted on a level
            if (element.IsWorkPlaneLevelBased())
                return (element as FamilyInstance)?.Host as Level;

            // No level found
            return null;
        }

        /***************************************************/

        [Description("Returns the element parameter that stores the level of the element. Checks multiple built-in level parameters in order of preference.")]
        [Input("element", "Element to get the level parameter from.")]
        [Output("parameter", "Parameter that stores the level, or null if not found or parameter is read-only.")]
        public static Parameter LevelParameter(this Element element)
        {
            if (element == null)
                return null;

            return m_LevelParameters
                .Select(x => element.get_Parameter(x))
                .Where(x => x != null)
                .OrderByDescending(x => x.AsElementId().Value() != -1) // Valid ElementIds first
                .FirstOrDefault(x => !x.IsReadOnly);
        }

        /***************************************************/

        [Description("Returns the element parameter that stores the base level of the element. Checks multiple built-in base level parameters in order of preference.")]
        [Input("element", "Element to get the base level parameter from.")]
        [Output("parameter", "Parameter that stores the base level, or null if not found or parameter is read-only.")]
        public static Parameter BaseLevelParameter(this Element element)
        {
            if (element == null)
                return null;

            return m_BaseLevelParameters
                .Select(x => element.get_Parameter(x))
                .Where(x => x != null)
                .OrderByDescending(x => x.AsElementId().Value() != -1) // Valid ElementIds first
                .FirstOrDefault(x => !x.IsReadOnly);
        }

        /***************************************************/

        [Description("Returns the element parameter that stores the top level of the element. Checks multiple built-in top level parameters in order of preference.")]
        [Input("element", "Element to get the top level parameter from.")]
        [Output("parameter", "Parameter that stores the top level, or null if not found or parameter is read-only.")]
        public static Parameter TopLevelParameter(this Element element)
        {
            if (element == null)
                return null;

            return m_TopLevelParameters
                .Select(x => element.get_Parameter(x))
                .Where(x => x != null)
                .OrderByDescending(x => x.AsElementId().Value() != -1) // Valid ElementIds first
                .FirstOrDefault(x => !x.IsReadOnly);
        }

        /***************************************************/
        /****              Private methods               ****/
        /***************************************************/

        [Description("True if element level is work plane based on the level and cannot be changed.")]
        [Input("element", "Element to check if it's work plane level based.")]
        [Output("isWorkPlaneLevelBased", "True if element is work plane based on a level.")]
        private static bool IsWorkPlaneLevelBased(this Element element)
        {
            FamilyInstance familyInstance = element as FamilyInstance;

            if (familyInstance == null || familyInstance.Symbol.Family.FamilyPlacementType != FamilyPlacementType.WorkPlaneBased)
                return false;

            return (familyInstance.Host as Level) != null;
        }

        /***************************************************/
        /****              Private fields                ****/
        /***************************************************/

        private static BuiltInParameter[] m_LevelParameters = new BuiltInParameter[]
        {
            BuiltInParameter.FAMILY_LEVEL_PARAM,
            BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM,
            BuiltInParameter.INSTANCE_SCHEDULE_ONLY_LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_LEVEL_PARAM,
            BuiltInParameter.RBS_START_LEVEL_PARAM,
            BuiltInParameter.GROUP_LEVEL,
            BuiltInParameter.ROOM_LEVEL_ID,
            BuiltInParameter.LEVEL_PARAM,
            BuiltInParameter.FACEROOF_LEVEL_PARAM
        };

        /***************************************************/

        private static BuiltInParameter[] m_BaseLevelParameters = new BuiltInParameter[]
        {
            BuiltInParameter.WALL_BASE_CONSTRAINT,
            BuiltInParameter.FAMILY_BASE_LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
        };

        /***************************************************/

        private static BuiltInParameter[] m_TopLevelParameters = new BuiltInParameter[]
        {
            BuiltInParameter.WALL_HEIGHT_TYPE,
            BuiltInParameter.FAMILY_TOP_LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM,
        };

        /***************************************************/
    }
}
