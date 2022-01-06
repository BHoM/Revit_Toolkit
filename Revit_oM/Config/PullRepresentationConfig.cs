/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Configuration used to specify representation to be pulled and passed to RevitRepresentation fragment.")]
    public class PullRepresentationConfig : IObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("If true, representation of elements will be pulled and stored under RenderMesh in RevitRepresentation fragment.")]
        public virtual bool PullRenderMesh { get; set; } = false;

        [Description("Detail level of representation, correspondent to level of detail in Revit.")]
        public virtual DetailLevel DetailLevel { get; set; } = DetailLevel.Medium;

        [Description("Invisible element parts will be pulled and passed to RevitRepresentation fragment if true. PullRepresentation switched to true needed for this to activate.")]
        public virtual bool IncludeNonVisible { get; set; } = false;

        /***************************************************/
    }
}


