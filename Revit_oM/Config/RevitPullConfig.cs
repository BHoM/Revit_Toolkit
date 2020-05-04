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
using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Enums;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Configuration used for adapter interaction with Revit on Pull action.")]
    public class RevitPullConfig: ActionConfig
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Discipline used on pull action. Default is Physical.")]
        public virtual Discipline Discipline { get; set; } = Discipline.Undefined;

        [Description("Elements from closed worksets will be processed if true.")]
        public virtual bool IncludeClosedWorksets { get; set; } = false;

        [Description("Configuration specifying which geometry should be pulled and passed to CustomData.")]
        public virtual PullGeometryConfig GeometryConfig { get; set; } = new PullGeometryConfig();

        [Description("Configuration specifying representation to be pulled and passed to CustomData.")]
        public virtual PullRepresentationConfig RepresentationConfig { get; set; } = new PullRepresentationConfig();

        /***************************************************/
    }
}
