/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.Adapter.Revit;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static ViewPlan ToRevitViewPlan(this oM.Adapters.Revit.Elements.ViewPlan viewPlan, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (viewPlan == null || string.IsNullOrEmpty(viewPlan.LevelName) || string.IsNullOrEmpty(viewPlan.ViewName))
                return null;

            ViewPlan revitViewPlan = refObjects.GetValue<ViewPlan>(document, viewPlan.BHoM_Guid);
            if (revitViewPlan != null)
                return revitViewPlan;

            settings = settings.DefaultIfNull();

            ElementId levelElementID = null;

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (levels == null || levels.Count < 1)
                return null;

            Level level = levels.Find(x => x.Name == viewPlan.LevelName);
            if (level == null)
                return null;

            levelElementID = level.Id;
            
            ElementId viewTemplateId = ElementId.InvalidElementId;
            
            if (!string.IsNullOrWhiteSpace(viewPlan.TemplateName))
            {
                IEnumerable<ViewPlan> viewPlans = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).Cast<ViewPlan>();
                ViewPlan viewPlanTemplate = viewPlans.FirstOrDefault(x => x.IsTemplate && viewPlan.TemplateName == x.Name);
                if (viewPlanTemplate == null)
                    Compute.ViewTemplateNotExistsWarning(viewPlan);
                else
                    viewTemplateId = viewPlanTemplate.Id;
            }

            revitViewPlan = Create.ViewPlan(document, level, viewPlan.ViewName, null, viewTemplateId) as ViewPlan;


            // Copy parameters from BHoM object to Revit element
            revitViewPlan.CopyParameters(viewPlan, settings);

            refObjects.AddOrReplace(viewPlan, revitViewPlan);
            return revitViewPlan;
        }

        /***************************************************/
    }
}

