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
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    [Description("Fragment containing the geometry extracted from Revit element represented by the BHoM object.")]
    public class RevitGeometry : IFragment, IImmutable
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        [Description("Edge curves of Revit element represented by the BHoM object carrying this fragment.")]
        public virtual IList<ICurve> Edges { get; } = null;

        [Description("Surface geometry of Revit element represented by the BHoM object carrying this fragment.")]
        public virtual IList<ISurface> Surfaces { get; } = null;

        [Description("Meshed surfaces of Revit element represented by the BHoM object carrying this fragment.")]
        public virtual IList<Mesh> Meshes { get; } = null;


        /***************************************************/
        /****            Public Constructors            ****/
        /***************************************************/

        public RevitGeometry(List<ICurve> edges, List<ISurface> surfaces, List<Mesh> meshes)
        {
            Edges = edges == null ? null : new ReadOnlyCollection<ICurve>(edges);
            Surfaces = surfaces == null ? null : new ReadOnlyCollection<ISurface>(surfaces);
            Meshes = meshes == null ? null : new ReadOnlyCollection<Mesh>(meshes);
        }

        /***************************************************/
    }
}


