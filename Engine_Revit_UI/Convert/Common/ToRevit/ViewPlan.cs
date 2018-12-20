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

        internal static ViewPlan ToRevitViewPlan(this oM.Adapters.Revit.Elements.ViewPlan floorPlan, Document document, PushSettings pushSettings = null)
        {
            if (floorPlan == null || string.IsNullOrEmpty(floorPlan.LevelName) || string.IsNullOrEmpty(floorPlan.Name))
                return null;

            ViewPlan aViewPlan = pushSettings.FindRefObject<ViewPlan>(document, floorPlan.BHoM_Guid);
            if (aViewPlan != null)
                return aViewPlan;

            pushSettings.DefaultIfNull();

            ElementId aElementId_Level = null;

            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (aLevelList == null || aLevelList.Count < 1)
                return null;

            Level aLevel = aLevelList.Find(x => x.Name == floorPlan.LevelName);
            if (aLevel == null)
                return null;

            aElementId_Level = aLevel.Id;

            ElementId aElementId_ViewFamilyType = ElementId.InvalidElementId;

            IEnumerable<ElementType> aViewFamilyTypes = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).Cast<ElementType>();

            ElementType aElementType = floorPlan.ElementType(aViewFamilyTypes, false);
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
            aViewPlan.ViewName = floorPlan.Name;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(aViewPlan, floorPlan, null, pushSettings.ConvertUnits);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(floorPlan, aViewPlan);

            return aViewPlan;
        }

        /***************************************************/
    }
}