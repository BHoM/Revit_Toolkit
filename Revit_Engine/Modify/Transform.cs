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

using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;

namespace BH.Engine.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Interface Methods - IElements             ****/
        /***************************************************/

        [Description("Transforms the IInstance's location and orientation by the transform matrix. Only rigid body transformations are supported.")]
        [Input("instance", "IInstance to transform.")]
        [Input("transform", "Transform matrix.")]
        [Input("tolerance", "Tolerance used in the check whether the input matrix is equivalent to the rigid body transformation.")]
        [Output("transformed", "Modified IInstance with unchanged properties, but transformed location and orientation.")]
        public static IInstance Transform(this IInstance instance, TransformMatrix transform, double tolerance = Tolerance.Distance)
        {
            if (!transform.IsRigidTransformation(tolerance))
            {
                BH.Engine.Reflection.Compute.RecordError("Transformation failed: only rigid body transformations are currently supported.");
                return null;
            }

            IInstance result = instance.GetShallowClone() as IInstance;
            result.Location = result.Location?.ITransform(transform);
            result.Orientation = result.Orientation?.Transform(transform);
            return result;
        }

        /***************************************************/
    }
}

