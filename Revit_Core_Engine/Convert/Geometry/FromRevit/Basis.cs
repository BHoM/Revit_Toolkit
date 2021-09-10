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

using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Transform to BH.oM.Geometry.Basis.")]
        [Input("transform", "Revit Transform to be converted.")]
        [Output("basis", "BH.oM.Geometry.Basis resulting from converting the input Revit Transform.")]
        public static oM.Geometry.Basis BasisFromRevit(this Transform transform)
        {
            if (transform == null)
                return null;

            BH.oM.Geometry.Vector x = new oM.Geometry.Vector { X = transform.BasisX.X, Y = transform.BasisX.Y, Z = transform.BasisX.Z };
            BH.oM.Geometry.Vector y = new oM.Geometry.Vector { X = transform.BasisY.X, Y = transform.BasisY.Y, Z = transform.BasisY.Z };
            BH.oM.Geometry.Vector z = new oM.Geometry.Vector { X = transform.BasisZ.X, Y = transform.BasisZ.Y, Z = transform.BasisZ.Z };
            return new oM.Geometry.Basis(x, y, z);
        }

        /***************************************************/
    }
}
