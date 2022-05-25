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

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns control points of the CurveLoop.")]
        [Input("curveLoop", "CurveLoop to get the points from.")]
        [Output("points", "List of uniqe curve loop points")]
        public static List<XYZ> Points(this CurveLoop curveLoop)
        {
            if (curveLoop == null)
                return null;

            List<XYZ> curveLoopPoints = new List<XYZ>();
            foreach (Curve curve in curveLoop)
            {
                IList<XYZ> points = curve.Tessellate();
                if (curveLoopPoints.Count == 0)
                {
                    curveLoopPoints.AddRange(points);
                    continue;
                }
                foreach (XYZ point in points)
                {
                    bool pointInList = false;
                    foreach (XYZ curveLoopPoint in curveLoopPoints)
                    {
                        if (point.IsAlmostEqualTo(curveLoopPoint))
                        {
                            pointInList = true;
                            break;
                        }
                    }
                    if (pointInList == false)
                        curveLoopPoints.Add(point);
                }
            }

            return curveLoopPoints;
        }

        /***************************************************/
    }
}
