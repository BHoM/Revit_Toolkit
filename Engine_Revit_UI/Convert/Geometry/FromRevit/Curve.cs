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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Geometry;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Geometry.Line FromRevit(this Line curve)
        {
            if (curve == null)
                return null;

            return new BH.oM.Geometry.Line { Start = curve.GetEndPoint(0).PointFromRevit(), End = curve.GetEndPoint(1).PointFromRevit(), Infinite = !curve.IsBound };
        }

        /***************************************************/

        public static oM.Geometry.ICurve FromRevit(this Arc curve)
        {
            if (curve == null)
                return null;

            if (!curve.IsBound)
            {
                return new oM.Geometry.Circle { Centre = curve.Center.PointFromRevit(), Normal = curve.Normal.VectorFromRevit().Normalise(), Radius = curve.Radius.ToSI(UnitType.UT_Length) };
            }

            BH.oM.Geometry.CoordinateSystem.Cartesian cs = BH.Engine.Geometry.Create.CartesianCoordinateSystem(curve.Center.PointFromRevit(), curve.XDirection.VectorFromRevit(), curve.YDirection.VectorFromRevit());
            
            double startAngle = curve.XDirection.AngleOnPlaneTo(curve.GetEndPoint(0) - curve.Center, curve.Normal);
            double endAngle = curve.XDirection.AngleOnPlaneTo(curve.GetEndPoint(1) - curve.Center, curve.Normal);
            if (startAngle > endAngle)
                startAngle -= 2 * Math.PI;

            return new oM.Geometry.Arc { CoordinateSystem = cs, Radius = curve.Radius.ToSI(UnitType.UT_Length), StartAngle = startAngle, EndAngle = endAngle };
        }

        /***************************************************/
        
        public static oM.Geometry.Ellipse FromRevit(this Ellipse curve)
        {
            if (curve == null)
                return null;

            if (!curve.IsBound)
                return new oM.Geometry.Ellipse { Centre = curve.Center.PointFromRevit(), Axis1 = curve.XDirection.VectorFromRevit().Normalise(), Radius1 = curve.RadiusX.ToSI(UnitType.UT_Length), Axis2 = curve.YDirection.VectorFromRevit().Normalise(), Radius2 = curve.RadiusY.ToSI(UnitType.UT_Length) };
            else
            {
                BH.Engine.Reflection.Compute.RecordWarning("Conversion of open ellipses is currently not supported because of lack of support for such type in BHoM.");
                return null;
            }
        }

        /***************************************************/
        
        public static oM.Geometry.NurbsCurve FromRevit(this NurbSpline curve)
        {
            if (curve == null)
                return null;

            List<double> knots = curve.Knots.Cast<double>().ToList();
            knots.RemoveAt(knots.Count - 1);
            knots.RemoveAt(0);

            return new BH.oM.Geometry.NurbsCurve
            {
                ControlPoints = curve.CtrlPoints.Select(x => x.PointFromRevit()).ToList(),
                Knots = knots,
                Weights = curve.Weights.Cast<double>().ToList()
            };
        }

        /***************************************************/
        
        public static oM.Geometry.NurbsCurve FromRevit(this HermiteSpline curve)
        {
            if (curve == null)
                return null;
            
            return NurbSpline.Create(curve).FromRevit();
        }

        /***************************************************/
        
        [NotImplemented]
        public static oM.Geometry.NurbsCurve FromRevit(this CylindricalHelix curve)
        {
            if (curve == null)
                return null;

            curve.CurveToBHoMNotImplemented();
            return null;
        }

        /***************************************************/

        public static oM.Geometry.Polyline FromRevit(this Polyloop polyloop)
        {
            IList<XYZ> xyzList = polyloop.GetPoints();
            if (xyzList == null || xyzList.Count < 2)
                return null;

            List<oM.Geometry.Point> points = xyzList.Select(x => x.PointFromRevit()).ToList();
            points.Add(points[0]);
            return new oM.Geometry.Polyline { ControlPoints = points };
        }

        /***************************************************/

        public static oM.Geometry.Polyline FromRevit(this PolyLine polyline)
        {
            IList<XYZ> xyzList = polyline.GetCoordinates();
            if (xyzList == null || xyzList.Count < 2)
                return null;

            List<oM.Geometry.Point> points = xyzList.Select(x => x.PointFromRevit()).ToList();
            points.Add(points[0]);
            return new oM.Geometry.Polyline { ControlPoints = points };
        }

        /***************************************************/

        public static oM.Geometry.PolyCurve FromRevit(this CurveLoop curveLoop)
        {
            if (curveLoop == null)
                return null;

            return new oM.Geometry.PolyCurve { Curves = curveLoop.Select(x => x.IFromRevit()).ToList() };
        }

        /***************************************************/

        public static oM.Geometry.ICurve FromRevit(this LocationCurve locationCurve)
        {
            if (locationCurve == null)
                return null;

            return locationCurve.Curve.IFromRevit();
        }

        /***************************************************/

        public static oM.Geometry.ICurve FromRevit(this Edge edge)
        {
            if (edge == null)
                return null;

            return edge.AsCurve().IFromRevit();
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static oM.Geometry.ICurve IFromRevit(this Curve curve)
        {
            oM.Geometry.ICurve result = FromRevit(curve as dynamic);

            if (result == null)
            {
                BH.Engine.Reflection.Compute.RecordWarning("Curve types without conversion support have been tesellated and converted into Polylines.");

                IList<XYZ> xyzList = curve.Tessellate();
                if (xyzList == null || xyzList.Count < 2)
                    result = null;
                else
                    result = new BH.oM.Geometry.Polyline { ControlPoints = xyzList.Select(x => x.PointFromRevit()).ToList() };
            }

            return result;
        }

        /***************************************************/
    }
}

