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

        public static oM.Geometry.Line ToBHoM(this Line curve)
        {
            if (curve == null)
                return null;

            return new BH.oM.Geometry.Line { Start = curve.GetEndPoint(0).ToBHoM(), End = curve.GetEndPoint(1).ToBHoM(), Infinite = !curve.IsBound };
        }

        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this Arc curve)
        {
            if (curve == null)
                return null;

            if (!curve.IsBound)
            {
                return new oM.Geometry.Circle { Centre = curve.Center.ToBHoM(), Normal = curve.Normal.ToBHoMVector().Normalise(), Radius = curve.Radius.ToSI(UnitType.UT_Length) };
            }

            BH.oM.Geometry.CoordinateSystem.Cartesian cs = BH.Engine.Geometry.Create.CartesianCoordinateSystem(curve.Center.ToBHoM(), curve.XDirection.ToBHoMVector(), curve.YDirection.ToBHoMVector());
            
            double startAngle = curve.XDirection.AngleOnPlaneTo(curve.GetEndPoint(0) - curve.Center, curve.Normal);
            double endAngle = curve.XDirection.AngleOnPlaneTo(curve.GetEndPoint(1) - curve.Center, curve.Normal);
            if (startAngle > endAngle)
                startAngle -= 2 * Math.PI;

            return new oM.Geometry.Arc { CoordinateSystem = cs, Radius = curve.Radius.ToSI(UnitType.UT_Length), StartAngle = startAngle, EndAngle = endAngle };
        }

        /***************************************************/
        
        public static oM.Geometry.Ellipse ToBHoM(this Ellipse curve)
        {
            if (curve == null)
                return null;

            if (!curve.IsBound)
                return new oM.Geometry.Ellipse { Centre = curve.Center.ToBHoM(), Axis1 = curve.XDirection.ToBHoMVector().Normalise(), Radius1 = curve.RadiusX.ToSI(UnitType.UT_Length), Axis2 = curve.YDirection.ToBHoMVector().Normalise(), Radius2 = curve.RadiusY.ToSI(UnitType.UT_Length) };
            else
            {
                BH.Engine.Reflection.Compute.RecordError("Conversion of open ellipses to BHoM is currently not supported.");
                return null;
            }
        }

        /***************************************************/
        
        public static oM.Geometry.NurbsCurve ToBHoM(this NurbSpline curve)
        {
            if (curve == null)
                return null;

            List<double> knots = curve.Knots.Cast<double>().ToList();
            knots.RemoveAt(knots.Count - 1);
            knots.RemoveAt(0);

            return new BH.oM.Geometry.NurbsCurve
            {
                ControlPoints = curve.CtrlPoints.Select(x => x.ToBHoM()).ToList(),
                Knots = knots,
                Weights = curve.Weights.Cast<double>().ToList()
            };
        }

        /***************************************************/
        
        public static oM.Geometry.NurbsCurve ToBHoM(this HermiteSpline curve)
        {
            if (curve == null)
                return null;
            
            return NurbSpline.Create(curve).ToBHoM();
        }

        /***************************************************/
        
        [NotImplemented]
        public static oM.Geometry.NurbsCurve ToBHoM(this CylindricalHelix curve)
        {
            if (curve == null)
                return null;

            curve.CurveToBHoMNotImplemented();
            return null;
        }

        /***************************************************/

        public static oM.Geometry.Polyline ToBHoM(this Polyloop polyloop)
        {
            IList<XYZ> xyzList = polyloop.GetPoints();
            if (xyzList == null || xyzList.Count < 2)
                return null;

            List<oM.Geometry.Point> points = xyzList.Select(x => x.ToBHoM()).ToList();
            points.Add(points[0]);
            return new oM.Geometry.Polyline { ControlPoints = points };
        }

        /***************************************************/

        public static oM.Geometry.Polyline ToBHoM(this PolyLine polyline)
        {
            IList<XYZ> xyzList = polyline.GetCoordinates();
            if (xyzList == null || xyzList.Count < 2)
                return null;

            List<oM.Geometry.Point> points = xyzList.Select(x => x.ToBHoM()).ToList();
            points.Add(points[0]);
            return new oM.Geometry.Polyline { ControlPoints = points };
        }

        /***************************************************/

        public static oM.Geometry.PolyCurve ToBHoM(this CurveLoop curveLoop)
        {
            if (curveLoop == null)
                return null;

            return new oM.Geometry.PolyCurve { Curves = curveLoop.Select(x => x.IToBHoM()).ToList() };
        }

        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve)
        {
            if (locationCurve == null)
                return null;

            return locationCurve.Curve.IToBHoM();
        }

        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this Edge edge)
        {
            if (edge == null)
                return null;

            return edge.AsCurve().IToBHoM();
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static oM.Geometry.ICurve IToBHoM(this Curve curve)
        {
            oM.Geometry.ICurve result = ToBHoM(curve as dynamic);

            if (result == null)
            {
                IList<XYZ> xyzList = curve.Tessellate();
                if (xyzList == null || xyzList.Count < 2)
                    result = null;
                else
                {
                    BH.Engine.Reflection.Compute.RecordWarning("The curve types without conversion support have been tesellated and converted into Polylines.");
                    result = new BH.oM.Geometry.Polyline { ControlPoints = xyzList.Select(x => x.ToBHoM()).ToList() };
                }
            }

            return result;
        }

        /***************************************************/
    }
}

