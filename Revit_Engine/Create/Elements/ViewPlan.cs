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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates BHoM ViewPlan object linked to a specific Revit level.")]
        [Input("name", "Name of the created ViewPlan correspondent with Revit view name.")]
        [InputFromProperty("levelName")]
        [Output("viewPlan")]
        public static ViewPlan ViewPlan(string name, string levelName)
        {
            ViewPlan viewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = levelName,
                IsTemplate = false
            };

            viewPlan.CustomData.Add("View Name", name);

            viewPlan.CustomData.Add(Convert.CategoryName, "Views");
            viewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return viewPlan;
        }

        /***************************************************/

        [Description("Creates BHoM ViewPlan object not linked to any specific Revit level.")]
        [Input("name", "Name of the created ViewPlan correspondent with Revit view name.")]
        [Output("viewPlan")]
        public static ViewPlan ViewPlan(string name)
        {
            ViewPlan viewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = null,
                IsTemplate = true
            };

            viewPlan.CustomData.Add("View Name", name);

            viewPlan.CustomData.Add(Convert.CategoryName, "Views");
            viewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return viewPlan;
        }

        /***************************************************/

        [Description("Creates BHoM ViewPlan object linked to a specific level, based on specific Revit view template.")]
        [Input("name", "Name of the created ViewPlan correspondent with Revit view name.")]
        [InputFromProperty("levelName")]
        [Input("viewTemplateName", "Name of Revit view template to be assigned to the created ViewPlan.")]
        [Output("viewPlan")]
        public static ViewPlan ViewPlan(string name, string levelName, string viewTemplateName)
        {
            ViewPlan viewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = levelName,
                IsTemplate = false
            };

            viewPlan.CustomData.Add("View Name", name);

            viewPlan.CustomData.Add(Convert.CategoryName, "Views");
            viewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");
            viewPlan.CustomData.Add(Convert.ViewTemplate, viewTemplateName);

            return viewPlan;
        }

        /***************************************************/
    }
}

