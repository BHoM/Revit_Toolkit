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
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Plane ArbitraryPlane(this Curve curve)
        {
            XYZ origin = curve.GetEndPoint(0);
            XYZ x = (curve.GetEndPoint(1) - origin).Normalize();
            XYZ helper = 1 - Math.Abs(x.DotProduct(XYZ.BasisZ)) > BH.oM.Geometry.Tolerance.Distance ? XYZ.BasisZ : XYZ.BasisX;
            XYZ y = x.CrossProduct(helper).Normalize();
            return Plane.CreateByOriginAndBasis(origin, x, y);
        }

        /***************************************************/
    }
}

