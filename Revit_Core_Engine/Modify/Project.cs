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
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Projects a point on a given plane.")]
        [Input("point", "Point to project on the plane.")]
        [Input("plane", "Plane to project the point on.")]
        [Output("projected", "Point projected on the input plane.")]
        public static XYZ Project(this XYZ point, Plane plane)
        {
            if (point == null || plane == null)
                return null;

            XYZ toOrigin = plane.Origin - point;
            double multiplier = plane.Normal.DotProduct(toOrigin);
            return point + plane.Normal * multiplier;
        }

        /***************************************************/

        [Description("Projects a line on a given plane.")]
        [Input("line", "Line to project on the plane.")]
        [Input("plane", "Plane to project the line on.")]
        [Output("projected", "Line projected on the input plane.")]
        public static Line Project(this Line line, Plane plane)
        {
            if (line == null || plane == null)
                return null;

            XYZ newStart = line.GetEndPoint(0).Project(plane);
            XYZ newEnd = line.GetEndPoint(1).Project(plane);
            if (newStart.DistanceTo(newEnd) > Tolerance.ShortCurve / 0.3048)
                return Line.CreateBound(newStart, newEnd);
            else
            {
                BH.Engine.Base.Compute.RecordWarning("Projection of a line on plane resulted in zero length curve.");
                return null;
            }    
        }

        /***************************************************/
    }
}

