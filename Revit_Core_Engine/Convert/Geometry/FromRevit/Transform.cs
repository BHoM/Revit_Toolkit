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

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Geometry.TransformMatrix FromRevit(this Transform transform)
        {
            if (transform == null)
                return null;

            oM.Geometry.TransformMatrix transformMatrix = new oM.Geometry.TransformMatrix();
            transformMatrix.Matrix[0, 0] = transform.BasisX.X;
            transformMatrix.Matrix[1, 0] = transform.BasisX.Y;
            transformMatrix.Matrix[2, 0] = transform.BasisX.Z;
            transformMatrix.Matrix[0, 1] = transform.BasisY.X;
            transformMatrix.Matrix[1, 1] = transform.BasisY.Y;
            transformMatrix.Matrix[2, 1] = transform.BasisY.Z;
            transformMatrix.Matrix[0, 2] = transform.BasisZ.X;
            transformMatrix.Matrix[1, 2] = transform.BasisZ.Y;
            transformMatrix.Matrix[2, 2] = transform.BasisZ.Z;
            transformMatrix.Matrix[0, 3] = transform.Origin.X;
            transformMatrix.Matrix[1, 3] = transform.Origin.Y;
            transformMatrix.Matrix[2, 3] = transform.Origin.Z;
            transformMatrix.Matrix[3, 3] = 1;
            return transformMatrix;
        }

        /***************************************************/
    }
}