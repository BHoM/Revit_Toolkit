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

using BH.oM.Base;
using BH.oM.Adapters.Revit.Properties;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Elements
{
    [Description("A wrapper BHoM type for Revit plan view, used to create or update Revit plans (on Push) and represent them as BHoMObjects (on Pull).")]
    public class ViewPlan : BHoMObject, IView
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("An entity storing the information about Revit view type.")]
        public virtual InstanceProperties InstanceProperties { get; set; } = new InstanceProperties();

        [Description("Name of Revit level to which the view belongs (Associated Level).")]
        public virtual string LevelName { get; set; } = string.Empty;

        [Description("If true, the object represents a Revit view template, if false, the object represents an actual Revit view.")]
        public virtual bool IsTemplate { get; set; } = false;

        /***************************************************/
    }
}



