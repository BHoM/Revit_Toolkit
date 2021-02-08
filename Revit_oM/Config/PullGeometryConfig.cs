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

using BH.oM.Adapter;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Configuration used to specify which geometry should be pulled and passed to RevitGeometry fragment.")]
    public class PullGeometryConfig : IObject
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("If true, edges of elements will be pulled and stored under Revit_edges in RevitGeometry fragment.")]
        public virtual bool PullEdges { get; set; } = false;

        [Description("If true, surfaces of elements will be pulled and stored under Revit_surfaces in RevitGeometry fragment.")]
        public virtual bool PullSurfaces { get; set; } = false;

        [Description("If true, meshed surfaces of elements will be pulled and stored under Revit_meshes in RevitGeometry fragment.")]
        public virtual bool PullMeshes { get; set; } = false;

        [Description("Detail level of mesh to be pulled, correspondent to level of detail in Revit.")]
        public virtual DetailLevel MeshDetailLevel { get; set; } = DetailLevel.Medium;

        [Description("Invisible element parts will be pulled and passed to RevitGeometry fragment if true. PullEdges or PullSurfaces switched to true needed for this to activate.")]
        public virtual bool IncludeNonVisible { get; set; } = false;

        /***************************************************/
    }
}

