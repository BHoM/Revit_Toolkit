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

using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;


namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /**** Internal Methods                          ****/
        /***************************************************/

        internal static ViewPlan ToRevitViewPlan(this oM.Adapters.Revit.Elements.ViewPlan viewPlan, Document document, PushSettings pushSettings = null)
        {
            if (viewPlan == null || string.IsNullOrEmpty(viewPlan.LevelName) || string.IsNullOrEmpty(viewPlan.Name))
                return null;

            ViewPlan aViewPlan = pushSettings.FindRefObject<ViewPlan>(document, viewPlan.BHoM_Guid);
            if (aViewPlan != null)
                return aViewPlan;

            pushSettings.DefaultIfNull();

            ElementId aElementId_Level = null;

            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (aLevelList == null || aLevelList.Count < 1)
                return null;

            Level aLevel = aLevelList.Find(x => x.Name == viewPlan.LevelName);
            if (aLevel == null)
                return null;

            aElementId_Level = aLevel.Id;

            ElementId aElementId_ViewFamilyType = ElementId.InvalidElementId;

            IEnumerable<ElementType> aViewFamilyTypes = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).Cast<ElementType>();

            ElementType aElementType = viewPlan.ElementType(aViewFamilyTypes, false);
            if (aElementType == null)
                return null;

            foreach (ViewFamilyType aViewFamilyType in aViewFamilyTypes)
            {
                if(aViewFamilyType.FamilyName == "Floor Plan")
                {
                    aElementId_ViewFamilyType = aViewFamilyType.Id;
                    break;
                }
            }

            if (aElementId_ViewFamilyType == ElementId.InvalidElementId)
                return null;

            aViewPlan = ViewPlan.Create(document, aElementId_ViewFamilyType, aElementId_Level);
#if REVIT2020
            aViewPlan.Name = viewPlan.Name;
#else
            aViewPlan.ViewName = viewPlan.Name;
#endif


            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aViewPlan, viewPlan, null, pushSettings.ConvertUnits);

            object aValue = null;
            if(viewPlan.CustomData.TryGetValue(BH.Engine.Adapters.Revit.Convert.ViewTemplate, out aValue))
            {
                if(aValue is string)
                {
                    List<ViewPlan> aViewPlanList = new FilteredElementCollector(document).OfClass(typeof(ViewPlan)).Cast<ViewPlan>().ToList();
                    ViewPlan aViewPlan_Template = aViewPlanList.Find(x => x.IsTemplate && aValue.Equals(x.Name));
                    if (aViewPlan_Template == null)
                        Compute.ViewTemplateNotExistsWarning(viewPlan);
                    else
                        aViewPlan.ViewTemplateId = aViewPlan_Template.Id;
                }
            }

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(viewPlan, aViewPlan);

            return aViewPlan;
        }

        /***************************************************/
    }
}