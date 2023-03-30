/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public Methods               ****/
        /***************************************************/

        [Description("Gets the distance between a 2 points along an input vector's direction.")]
        [Input("pnt1", "A point to compute distance from.")]
        [Input("pnt2", "A point to compute distance to.")]
        [Input("vector", "A vector to measure point distance along.")]
        [Output("distance", "The distance between a 2 points along an input vector's direction.")]
        public static double DistanceAlongVector(this XYZ pnt1, XYZ pnt2, XYZ vector)
        {
            XYZ pointVector = pnt2 - pnt1;
            return Math.Abs(pointVector.DotProduct(vector.Normalize()));
        }

        /***************************************************/
    }
}


