/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates plane based on a point and a parallel vector (perpendicular to the normal of created plane).")]
        [Input("point", "Point on created plane.")]
        [Input("vectorInPlane", "Vector parallel to the created plane.")]
        [Output("plane", "Arbitrary plane created based on the provided inputs.")]
        public static Plane ArbitraryPlane(XYZ point, XYZ vectorInPlane)
        {
            XYZ helper = 1 - Math.Abs(vectorInPlane.DotProduct(XYZ.BasisZ)) > Tolerance.Angle ? XYZ.BasisZ : XYZ.BasisX;
            XYZ y = vectorInPlane.CrossProduct(helper).Normalize();
            return Plane.CreateByOriginAndBasis(point, vectorInPlane, y);
        }

        /***************************************************/
    }
}
