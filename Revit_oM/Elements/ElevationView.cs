/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Geometry;

namespace BH.oM.Adapters.Revit.Elements
{
    [Description("A wrapper BHoM type for Revit elevation view, used to create or update Revit elevations (on Push) and represent them as BHoMObjects (on Pull).")]
    public class ElevationView : BHoMObject, IView
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("An entity storing the information about Revit view type.")]
        public virtual InstanceProperties InstanceProperties { get; set; } = new InstanceProperties();

        [Description("Line that is the location of the elevation view.")]
        public virtual Line BottomLine { get; set; } = null;

        [Description("Height of the elevation view.")]
        public virtual double Heigth { get; set; } = 0;

        [Description("Depth of the elevation view.")]
        public virtual double Depth { get; set; } = 0;

        [Description("Name of the view template applied to the Revit view.")]
        public virtual string TemplateName { get; set; } = string.Empty;

        [Description("True to show crop region.")]
        public virtual bool CropRegionVisible { get; set; } = false;

        [Description("True to show annotation crop.")]
        public virtual bool AnnotationCropVisible { get; set; } = false;

        /***************************************************/
    }
}






