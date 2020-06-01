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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Adapters.Revit.Elements.ViewPlan ViewPlanFromRevit(this ViewPlan revitViewPlan, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Adapters.Revit.Elements.ViewPlan viewPlan = refObjects.GetValue<oM.Adapters.Revit.Elements.ViewPlan>(revitViewPlan.Id.IntegerValue);
            if (viewPlan != null)
                return viewPlan;

            if(!revitViewPlan.IsTemplate && revitViewPlan.GenLevel != null)
                viewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(revitViewPlan.Name, revitViewPlan.GenLevel.Name);
            else
                viewPlan = BH.Engine.Adapters.Revit.Create.ViewPlan(revitViewPlan.Name);

            ElementType elementType = revitViewPlan.Document.GetElement(revitViewPlan.GetTypeId()) as ElementType;
            if (elementType != null)
                viewPlan.InstanceProperties = elementType.InstancePropertiesFromRevit(settings, refObjects);

            viewPlan.Name = revitViewPlan.Name;

            //Set identifiers, parameters & custom data
            viewPlan.SetIdentifiers(revitViewPlan);
            viewPlan.CopyParameters(revitViewPlan, settings.ParameterSettings);
            viewPlan.SetProperties(revitViewPlan, settings.ParameterSettings);

            refObjects.AddOrReplace(revitViewPlan.Id, viewPlan);
            return viewPlan;
        }

        /***************************************************/
    }
}

