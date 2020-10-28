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
using System.ComponentModel;

namespace BH.oM.Adapters.Revit
{
    //[Description("Wrapper for Revit family file (.rfa) that stores basic information about it such as family category, familiy type names etc. Prototype, currently with limited functionality.")]
    public class RevitGeometry : IFragment, IImmutable
    {
        /***************************************************/
        /****             Public Properties             ****/
        /***************************************************/

        //[Description("Path to the Revit family file wrapped by this object.")]
        public virtual List<ICurve> Edges { get; set; } = null;

        //[Description("Path to the Revit family file wrapped by this object.")]
        public virtual List<ISurface> Surfaces { get; set; } = null;

        //[Description("Path to the Revit family file wrapped by this object.")]
        public virtual List<Mesh> Meshes { get; set; } = null;


        /***************************************************/
        /****            Public Constructors            ****/
        /***************************************************/

        public RevitGeometry(List<ICurve> edges, List<ISurface> surfaces, List<Mesh> meshes)
        {
            Edges = edges;
            Surfaces = surfaces;
            Meshes = meshes;
        }

        /***************************************************/
    }
}


