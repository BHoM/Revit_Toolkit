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

using BH.oM.Adapters.Revit.Properties;
using BH.oM.Base;
using BH.oM.Geometry;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Elements
{
    [Description("A wrapper BHoM type for Revit viewport, used to create or update Revit viewports (on Push) and represent them as BHoMObjects (on Pull).")]
    public class Viewport : BHoMObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("An entity storing the information about Revit viewport type.")]
        public virtual InstanceProperties InstanceProperties { get; set; } = new InstanceProperties();

        [Description("Location of the viewport in Revit sheet space.")]
        public virtual Point Location { get; set; } = new Point() { X = 0, Y = 0, Z = 0 };

        /***************************************************/
    }
}


