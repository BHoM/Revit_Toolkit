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

using System.ComponentModel;

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates ViewPlan object by given name and Level Name.")]
        [Input("name", "View plan Name")]
        [Input("levelName", "Level Name")]
        [Output("ViewPlan")]
        public static ViewPlan ViewPlan(string name, string levelName)
        {
            ViewPlan aViewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = levelName,
                IsTemplate = false
            };

            aViewPlan.CustomData.Add("View Name", name);

            aViewPlan.CustomData.Add(Convert.CategoryName, "Views");
            aViewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return aViewPlan;
        }

        /***************************************************/

        [Description("Creates ViewPlan object by given name.")]
        [Input("name", "View plan Name")]
        [Output("ViewPlan")]
        public static ViewPlan ViewPlan(string name)
        {
            ViewPlan aViewPlan = new ViewPlan()
            {
                Name = name,
                LevelName = null,
                IsTemplate = true
            };

            aViewPlan.CustomData.Add("View Name", name);

            aViewPlan.CustomData.Add(Convert.CategoryName, "Views");
            aViewPlan.CustomData.Add(Convert.FamilyName, "Floor Plan");

            return aViewPlan;
        }

        /***************************************************/
    }
}
