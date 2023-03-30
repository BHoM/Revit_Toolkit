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
using BH.oM.Adapters.Revit;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Compute
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the intersection point of 2 vectors given their start/end points and directions.")]
        [Input("startPoint", "A point on the first vector.")]
        [Input("endPoint", "A point on the second vector.")]
        [Input("startDir", "The direction of the first vector.")]
        [Input("endDir", "The direction of the second vector.")]
        [Output("xyz", "The intersection point of 2 vectors given their start/end points and directions.")]
        public static XYZ VectorIntersection(this XYZ startPoint, XYZ endPoint, XYZ startDir, XYZ endDir)
        {
            double x;
            double y;

            if (Math.Abs(startDir.X) < Tolerance.Angle)
            {
                x = startPoint.X;
                y = endPoint.Y + (x - endPoint.X) * endDir.Y / endDir.X;
            }
            else if (Math.Abs(endDir.X) < Tolerance.Angle)
            {
                x = endPoint.X;
                y = startPoint.Y + (x - startPoint.X) * startDir.Y / startDir.X;
            }
            else
            {
                double m1 = startDir.Y / startDir.X;
                double A1 = -m1;
                double C1 = m1 * startPoint.X - startPoint.Y;
                double m2 = endDir.Y / endDir.X;
                double A2 = -m2;
                double C2 = m2 * endPoint.X - endPoint.Y;
                double delta = A2 - A1;
                x = (C1 - C2) / delta;
                y = (A1 * C2 - A2 * C1) / delta;
            }

            return new XYZ(x, y, startPoint.Z);
        }

        /***************************************************/
    }
}




