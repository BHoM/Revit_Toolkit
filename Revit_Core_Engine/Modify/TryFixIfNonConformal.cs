/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Attempts to make a transform conformal in case it is not.")]
        [Input("transform", "Transform to fix.")]
        [Input("tolerance", "Tolerance used in geometrical processing.")]
        [Output("fixed", "Transform after fixing, input value in case it was conformal or fixing failed.")]
        public static Transform TryFixIfNonConformal(this Transform transform, double tolerance = 1e-6)
        {
            Transform result = transform;
            if (!result.IsConformal)
                result = result.AlignWithGlobalCoordinates(tolerance);

            if (!result.IsConformal)
                result = result.Orthonormalise();

            if (!result.IsConformal || !result.BasisX.IsAlmostEqualTo(transform.BasisX, tolerance) || !result.BasisY.IsAlmostEqualTo(transform.BasisY, tolerance) || !result.BasisZ.IsAlmostEqualTo(transform.BasisZ, tolerance))
            {
                BH.Engine.Base.Compute.RecordWarning("Transform is not conformal, attempt was made to fix it, but without success.");
                return transform;
            }

            return result;
        }

        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static Transform AlignWithGlobalCoordinates(this Transform transform, double tolerance)
        {
            Transform result = new Transform(transform);
            result.BasisX = result.BasisX.AlignWithGlobalCoordinates(tolerance);
            result.BasisY = result.BasisY.AlignWithGlobalCoordinates(tolerance);
            result.BasisZ = result.BasisZ.AlignWithGlobalCoordinates(tolerance);
            return result;
        }

        /***************************************************/

        private static XYZ AlignWithGlobalCoordinates(this XYZ vector, double tolerance)
        {
            if (vector.IsAlmostEqualTo(XYZ.BasisX, tolerance))
                return XYZ.BasisX;
            else if (vector.IsAlmostEqualTo(XYZ.BasisY, tolerance))
                return XYZ.BasisY;
            else if (vector.IsAlmostEqualTo(XYZ.BasisZ, tolerance))
                return XYZ.BasisZ;
            else
                return vector;
        }

        /***************************************************/

        private static Transform Orthonormalise(this Transform transform)
        {
            XYZ x = transform.BasisX;
            XYZ y = transform.BasisY;

            // Normalize X
            x = x.Normalize();

            // Make Y orthogonal to X
            y = y - (y.DotProduct(x)) * x;
            y = y.Normalize();

            // Compute Z as cross product
            XYZ z = x.CrossProduct(y);
            z = z.Normalize();

            // Build new transform
            Transform result = Transform.Identity;
            result.Origin = transform.Origin;
            result.BasisX = x;
            result.BasisY = y;
            result.BasisZ = z;

            return result;
        }

        /***************************************************/
    }
}
