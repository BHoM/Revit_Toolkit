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

using Autodesk.Revit.DB;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.TransformMatrix to a Revit Transform.")]
        [Input("transformMatrix", "BH.oM.Geometry.TransformMatrix to be converted.")]
        [Output("transform", "Revit Transform resulting from converting the input BH.oM.Geometry.TransformMatrix.")]
        public static Transform ToRevit(this oM.Geometry.TransformMatrix transformMatrix)
        {
            if (transformMatrix == null)
                return null;
            
            XYZ basisX = new XYZ(transformMatrix.Matrix[0, 0], transformMatrix.Matrix[1, 0], transformMatrix.Matrix[2, 0]);
            XYZ basisY = new XYZ(transformMatrix.Matrix[0, 1], transformMatrix.Matrix[1, 1], transformMatrix.Matrix[2, 1]);
            XYZ basisZ = new XYZ(transformMatrix.Matrix[0, 2], transformMatrix.Matrix[1, 2], transformMatrix.Matrix[2, 2]);
            XYZ translation = new XYZ(transformMatrix.Matrix[0, 3].FromSI(SpecTypeId.Length), transformMatrix.Matrix[1, 3].FromSI(SpecTypeId.Length), transformMatrix.Matrix[2, 3].FromSI(SpecTypeId.Length));
            Transform transform = Transform.CreateTranslation(translation);
            transform.set_Basis(0, basisX);
            transform.set_Basis(1, basisY);
            transform.set_Basis(2, basisZ);
            return transform;
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Basis to a Revit Transform.")]
        [Input("basis", "BH.oM.Geometry.Basis to be converted.")]
        [Output("transform", "Revit Transform resulting from converting the input BH.oM.Geometry.Basis.")]
        public static Transform ToRevit(this oM.Geometry.Basis basis)
        {
            if (basis == null)
                return null;

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

