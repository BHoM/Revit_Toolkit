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

using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
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

        public static Curve ToRevitCurve(this ICurve curve)
        {
            if (curve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = curve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(aLine.Start.ToRevit(), aLine.End.ToRevit());
            }

            if (curve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = curve as oM.Geometry.Arc;
                double radius = aArc.Radius.FromSI(UnitType.UT_Length);
                return Autodesk.Revit.DB.Arc.Create(aArc.CoordinateSystem.ToRevit(), radius, aArc.StartAngle, aArc.EndAngle);
                //return Autodesk.Revit.DB.Arc.Create(aArc.CoordinateSystem.Origin.ToRevitXYZ(pushSettings), radius, aArc.StartAngle, aArc.EndAngle, aArc.CoordinateSystem.X.ToRevitXYZ(pushSettings).Normalize(), aArc.CoordinateSystem.Y.ToRevitXYZ(pushSettings).Normalize());
            }

            if (curve is NurbsCurve)
            {
                NurbsCurve aNurbCurve = curve as NurbsCurve;
                List<double> knots = aNurbCurve.Knots.ToList();
                knots.Insert(0, knots[0]);
                knots.Add(knots[knots.Count - 1]);
                List<XYZ> controlPoints = aNurbCurve.ControlPoints.Select(x => x.ToRevit()).ToList();
                try
                {
                    return NurbSpline.CreateCurve(aNurbCurve.Degree(), knots, controlPoints, aNurbCurve.Weights);
                }
                catch
                {
                    BH.Engine.Reflection.Compute.RecordWarning("Conversion of BHoM nurbs curve to Revit curve based on degree and knot vector failed. A simplified (possibly different) spline has been created.");
                    return NurbSpline.CreateCurve(controlPoints, aNurbCurve.Weights);
                }
            }

            if (curve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = curve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(aEllipse.Centre.ToRevit(), aEllipse.Radius1, aEllipse.Radius2, aEllipse.Axis1.ToRevit(), aEllipse.Axis2.ToRevit(), 0, 1);
            }

            if (curve is Polyline)
            {
                Polyline aPolyline = curve as Polyline;
                if (aPolyline.ControlPoints.Count == 2)
                    return Autodesk.Revit.DB.Line.CreateBound(aPolyline.ControlPoints[0].ToRevit(), aPolyline.ControlPoints[1].ToRevit());
            }

            return null;
        }

        /***************************************************/

        public static List<Curve> ToRevitCurves(this ICurve curve)
        {
            if (curve is Polyline || curve is PolyCurve)
            {
                List<Curve> result = new List<Curve>();
                foreach (ICurve cc in curve.ISubParts())
                {
                    result.AddRange(cc.ToRevitCurves());
                }

                return result;
            }
            else if (curve is BH.oM.Geometry.Arc)
            {
                BH.oM.Geometry.Arc arc = curve as BH.oM.Geometry.Arc;
                if (arc.Radius < Tolerance.Distance)
                    return new List<Curve>();
                
                if (Math.Abs(2 * Math.PI) - arc.EndAngle + arc.StartAngle < BH.oM.Geometry.Tolerance.Angle)
                {
                    double r = arc.Radius.FromSI(UnitType.UT_Length);
                    XYZ centre = arc.CoordinateSystem.Origin.ToRevitXYZ();
                    XYZ xAxis = arc.CoordinateSystem.X.ToRevitXYZ().Normalize();
                    XYZ yAxis = arc.CoordinateSystem.Y.ToRevitXYZ().Normalize();

                    Autodesk.Revit.DB.Arc arc1 = Autodesk.Revit.DB.Arc.Create(centre, r, 0, Math.PI, xAxis, yAxis);
                    Autodesk.Revit.DB.Arc arc2 = Autodesk.Revit.DB.Arc.Create(centre, r, 0, Math.PI, -xAxis, -yAxis);
                    return new List<Curve> { arc1, arc2 };
                }
            }
            else if (curve is Circle)
            {
                Circle circle = curve as Circle;
                double r = circle.Radius.FromSI(UnitType.UT_Length);
                
                XYZ centre = circle.Centre.ToRevitXYZ();
                XYZ normal = circle.Normal.ToRevitXYZ().Normalize();
                Autodesk.Revit.DB.Plane p = Autodesk.Revit.DB.Plane.CreateByNormalAndOrigin(normal, centre);

                Autodesk.Revit.DB.Arc arc1 = Autodesk.Revit.DB.Arc.Create(p, r, 0, Math.PI);
                Autodesk.Revit.DB.Arc arc2 = Autodesk.Revit.DB.Arc.Create(centre, r, 0, Math.PI, -arc1.XDirection, -arc1.YDirection);
                return new List<Curve> { arc1, arc2 };
            }
            else if (curve is NurbsCurve)
            {
                Curve nc = curve.ToRevitCurve();
                if (nc.GetEndPoint(0).DistanceTo(nc.GetEndPoint(1)) <= BH.oM.Geometry.Tolerance.Distance)
                {
                    double param1 = nc.GetEndParameter(0);
                    double param2 = nc.GetEndParameter(1);
                    Curve c1 = nc.Clone();
                    Curve c2 = nc.Clone();
                    c1.MakeBound(param1, (param1 + param2) * 0.5);
                    c2.MakeBound((param1 + param2) * 0.5, param2);
                    return new List<Curve> { c1, c2 };
                }
                else
                    return new List<Curve> { nc };
            }

            return new List<Curve> { curve.ToRevitCurve() };
        }

        /***************************************************/
    }
}