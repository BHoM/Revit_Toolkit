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

        public static Transform ToRevit(this oM.Geometry.TransformMatrix transformMatrix)
        {
            XYZ translation = new XYZ(transformMatrix.Matrix[3, 0].FromSI(UnitType.UT_Length), transformMatrix.Matrix[3, 1].FromSI(UnitType.UT_Length), transformMatrix.Matrix[3, 2].FromSI(UnitType.UT_Length));
            XYZ basisX = new XYZ(transformMatrix.Matrix[0, 0], transformMatrix.Matrix[1, 0], transformMatrix.Matrix[2, 0]);
            XYZ basisY = new XYZ(transformMatrix.Matrix[0, 1], transformMatrix.Matrix[1, 1], transformMatrix.Matrix[2, 1]);
            XYZ basisZ = new XYZ(transformMatrix.Matrix[0, 2], transformMatrix.Matrix[1, 2], transformMatrix.Matrix[2, 2]);
            Transform transform = Transform.CreateTranslation(translation);
            transform.set_Basis(0, basisX);
            transform.set_Basis(1, basisY);
            transform.set_Basis(2, basisZ);
            return transform;
        }

        /***************************************************/

        public static Transform ToRevit(this oM.Geometry.Basis basis)
        {
            XYZ basisX = new XYZ(basis.X.X, basis.X.Y, basis.X.Z);
            XYZ basisY = new XYZ(basis.Y.X, basis.Y.Y, basis.Y.Z);
            XYZ basisZ = new XYZ(basis.Z.X, basis.Z.Y, basis.Z.Z);
            Transform transform = Transform.Identity;
            transform.set_Basis(0, basisX);
            transform.set_Basis(1, basisY);
            transform.set_Basis(2, basisZ);
            return transform;
        }

        /***************************************************/
    }
}