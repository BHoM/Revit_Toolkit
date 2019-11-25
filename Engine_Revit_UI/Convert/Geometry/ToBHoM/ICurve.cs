/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this Curve curve)
        {
            if (curve == null)
                return null;

            if (curve is Line)
                return BH.Engine.Geometry.Create.Line(curve.GetEndPoint(0).ToBHoM(), curve.GetEndPoint(1).ToBHoM());

            if (curve is Arc)
            {
                Arc arc = curve as Arc;
                double radius = arc.Radius.ToSI(UnitType.UT_Length);
                Plane plane = Plane.CreateByOriginAndBasis(arc.Center, arc.XDirection, arc.YDirection);
                double startAngle = arc.XDirection.AngleOnPlaneTo(arc.GetEndPoint(0) - arc.Center, arc.Normal);
                double endAngle = arc.XDirection.AngleOnPlaneTo(arc.GetEndPoint(1) - arc.Center, arc.Normal);
                if (startAngle > endAngle)
                {
                    startAngle -= 2 * Math.PI;
                }
                return BH.Engine.Geometry.Create.Arc(plane.ToBHoM(), radius, startAngle, endAngle);
            }
            if (curve is NurbSpline)
            {
                NurbSpline nurbs = curve as NurbSpline;

                List<double> knots = nurbs.Knots.Cast<double>().ToList();
                knots.RemoveAt(knots.Count - 1);
                knots.RemoveAt(0);

                return new BH.oM.Geometry.NurbsCurve
                {
                    ControlPoints = nurbs.CtrlPoints.Select(x => x.ToBHoM()).ToList(),
                    Knots = knots,
                    Weights = nurbs.Weights.Cast<double>().ToList()
                };
            }

            IList<XYZ> xyzList = curve.Tessellate();
            if (xyzList == null || xyzList.Count < 2)
                return null;

            return BH.Engine.Geometry.Create.Polyline(xyzList.ToList().ConvertAll(x => ToBHoM(x)));
        }

        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve)
        {
            if (locationCurve == null)
                return null;

            return locationCurve.Curve.ToBHoM();
        }

        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this Edge edge)
        {
            return edge.AsCurve().ToBHoM();
        }

        /***************************************************/
    }
}