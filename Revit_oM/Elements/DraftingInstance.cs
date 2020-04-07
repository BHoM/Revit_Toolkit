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

using BH.oM.Adapters.Revit.Interface;
using BH.oM.Adapters.Revit.Properties;
using BH.oM.Base;
using BH.oM.Geometry;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit.Elements
{
    [Description("A generic wrapper BHoM type corresponding to any view-specific Revit element (drafting elements e.g. lines and hatches, tags, text notes etc.).")]
    public class DraftingInstance : BHoMObject, IInstance 
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Information about Revit family type or graphic type of the instance.")]
        public virtual InstanceProperties Properties { get; set; } = new InstanceProperties();

        [Description("Location of the instance in three dimensional space.")]
        public virtual IGeometry Location { get; set; } = new Point();

        [Description("Name of Revit view to which the instance belongs.")]
        public virtual string ViewName { get; set; } = string.Empty;

        /***************************************************/
    }
}



