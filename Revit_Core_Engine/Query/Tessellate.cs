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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Get control points of CurveLoop with tessellation.")]
        [Input("curveLoop", "CurveLoop to get the points from.")]
        [Output("curveLoopPoints", "CurveLoop points.")]
        public static List<XYZ> Tessellate(this CurveLoop curveLoop)
        {
            if (curveLoop == null || curveLoop.Count() == 0)
            {
                BH.Engine.Base.Compute.RecordError($"CurveLoop is null or has 0 curves.");
                return null;
            }

            List<XYZ> curveLoopPoints = new List<XYZ>();
            foreach (Curve curve in curveLoop)
            {
                IList<XYZ> points = curve.Tessellate();
                if (points.Count != 0)
                    curveLoopPoints.AddRange(points.Take(points.Count - 1));
            }

            if (curveLoop.IsOpen())
                curveLoopPoints.Add(curveLoop.Last().GetEndPoint(1));

            return curveLoopPoints;
        }

        /***************************************************/
    }
}
