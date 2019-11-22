/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Adapters.Revit.Elements.ViewPlan ToBHoMViewPlan(this ViewPlan viewPlan, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Adapters.Revit.Elements.ViewPlan aViewPlan = pullSettings.FindRefObject<oM.Adapters.Revit.Elements.ViewPlan>(viewPlan.Id.IntegerValue);
            if (aViewPlan != null)
                return aViewPlan;

            if(!viewPlan.IsTemplate && viewPlan.GenLevel != null)
                aViewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(viewPlan.Name, viewPlan.GenLevel.Name);
            else
                aViewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(viewPlan.Name);

            ElementType aElementType = viewPlan.Document.GetElement(viewPlan.GetTypeId()) as ElementType;
            if(aElementType != null)
                aViewPlan.InstanceProperties = ToBHoMInstanceProperties(aElementType, pullSettings);

            aViewPlan.Name = viewPlan.Name;
            aViewPlan = Modify.SetIdentifiers(aViewPlan, viewPlan) as oM.Adapters.Revit.Elements.ViewPlan;
            if (pullSettings.CopyCustomData)
                aViewPlan = Modify.SetCustomData(aViewPlan, viewPlan) as oM.Adapters.Revit.Elements.ViewPlan;

            aViewPlan = aViewPlan.UpdateValues(pullSettings, viewPlan) as oM.Adapters.Revit.Elements.ViewPlan;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aViewPlan);

            return aViewPlan;
        }

        /***************************************************/
    }
}
