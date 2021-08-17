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
 
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Base;
using BH.oM.Geometry;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Elements
{
    [Description("A generic wrapper BHoM type corresponding to any view-independent Revit element (model elements, e.g. duct or beam). On Push it can be used to generate or update Revit model elements that do not have a correspondent BHoM type, on Pull all Revit model elements that do not have explicit Convert method for given discipline will be converted to ModelInstance.")]
    public class ModelInstance : BHoMObject, IInstance
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Information about Revit family type of the instance.")]
        public virtual InstanceProperties Properties { get; set; } = new InstanceProperties();

        [Description("Location of the instance in in three dimensional space.")]
        public virtual IGeometry Location { get; set; } = new Point();

        [Description("Orientation of the instance in 3 dimensional space. Applicable only to point-based ModelInstances. If null, a default orientation will be applied.")]
        public virtual Basis Orientation { get; set; } = null;

        /***************************************************/
    }
}



