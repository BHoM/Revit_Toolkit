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
using BH.Engine.Geometry;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Line ToRevit(this BH.oM.Geometry.Line curve)
        {
            return Line.CreateBound(curve.Start.ToRevit(), curve.End.ToRevit());
        }

        /***************************************************/

        public static Arc ToRevit(this BH.oM.Geometry.Arc curve)
        {
            double radius = curve.Radius.FromSI(UnitType.UT_Length);
            return Arc.Create(curve.CoordinateSystem.ToRevit(), radius, curve.StartAngle, curve.EndAngle);
        }

        /***************************************************/

        public static Curve ToRevit(this oM.Geometry.Circle curve)
        {
            double radius = curve.Radius.FromSI(UnitType.UT_Length);
            Plane plane = Plane.CreateByNormalAndOrigin(curve.Normal.ToRevit().Normalize(), curve.Centre.ToRevit());
            return Arc.Create(plane, radius, 0, Math.PI * 2);
        }

        /***************************************************/

        public static Ellipse ToRevit(this oM.Geometry.Ellipse curve)
        {
            return Ellipse.CreateCurve(curve.Centre.ToRevit(), curve.Radius1.FromSI(UnitType.UT_Length), curve.Radius2.FromSI(UnitType.UT_Length), curve.Axis1.ToRevit().Normalize(), curve.Axis2.ToRevit().Normalize(), 0, Math.PI * 2) as Ellipse;
        }

        /***************************************************/

        public static Curve ToRevit(this oM.Geometry.NurbsCurve curve)
        {
            if (curve.IsPeriodic())
            {
                BH.Engine.Reflection.Compute.RecordError("Conversion of BHoM nurbs curve to Revit failed as Revit does not support periodic nurbs curves.");
                return null;
            }

            List<double> knots = curve.Knots.ToList();
            knots.Insert(0, knots[0]);
            knots.Add(knots[knots.Count - 1]);
            List<XYZ> controlPoints = curve.ControlPoints.Select(x => x.ToRevit()).ToList();
            
            try
            {
                return NurbSpline.CreateCurve(curve.Degree(), knots, controlPoints, curve.Weights);
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordWarning("Conversion of BHoM nurbs curve to Revit based on degree and knot vector failed. A simplified (possibly different) spline has been created.");
                return NurbSpline.CreateCurve(controlPoints, curve.Weights);
            }
        }

        /***************************************************/

        public static Curve ToRevit(this oM.Geometry.Polyline curve)
        {
            Compute.MultiSegmentCurveError();
            return null;
        }

        /***************************************************/

        public static Curve ToRevit(this oM.Geometry.PolyCurve curve)
        {
            Compute.MultiSegmentCurveError();
            return null;
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static Curve IToRevit(this oM.Geometry.ICurve curve)
        {
            return ToRevit(curve as dynamic);
        }
        
        /***************************************************/
    }
}
