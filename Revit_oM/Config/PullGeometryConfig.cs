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

using BH.oM.Adapter;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Configuration used to specify which geometry should be pulled and passed to CustomData.")]
    public class PullGeometryConfig
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("If true, edges of elements will be pulled and stored under Revit_edges in CustomData.")]
        public virtual bool PullEdges { get; set; } = false;

        [Description("If true, surfaces of elements will be pulled and stored under Revit_surfaces in CustomData.")]
        public virtual bool PullSurfaces { get; set; } = false;

        [Description("Invisible element parts will be pulled and passed to CustomData if true. PullEdges or PullSurfaces switched to true needed for this to activate.")]
        public virtual bool IncludeNonVisible { get; set; } = false;

        /***************************************************/
    }
}
