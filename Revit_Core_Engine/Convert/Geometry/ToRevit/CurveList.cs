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
using BH.Engine.Base;
using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.Line to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.Line to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.Line.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Line curve)
        {
            return new List<Curve> { curve.ToRevit() };
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Arc to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.Arc to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.Arc.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Arc curve)
        {
            //Split the curve in half when it is closed.
            if (Math.Abs(2 * Math.PI) - curve.EndAngle + curve.StartAngle < BH.oM.Adapters.Revit.Tolerance.Angle)
            {
                double r = curve.Radius.FromSI(SpecTypeId.Length);
                XYZ centre = curve.CoordinateSystem.Origin.ToRevit();
                XYZ xAxis = curve.CoordinateSystem.X.ToRevit().Normalize();
                XYZ yAxis = curve.CoordinateSystem.Y.ToRevit().Normalize();

                Arc arc1 = Arc.Create(centre, r, 0, Math.PI, xAxis, yAxis);
                Arc arc2 = Arc.Create(centre, r, 0, Math.PI, -xAxis, -yAxis);
                return new List<Curve> { arc1, arc2 };
            }
            else
                return new List<Curve> { curve.ToRevit() };
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Circle to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.Circle to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.Circle.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Circle curve)
        {
            double r = curve.Radius.FromSI(SpecTypeId.Length);

            XYZ centre = curve.Centre.ToRevit();
            XYZ normal = curve.Normal.ToRevit().Normalize();
            Plane p = Plane.CreateByNormalAndOrigin(normal, centre);

            Arc arc1 = Arc.Create(p, r, 0, Math.PI);
            Arc arc2 = Arc.Create(centre, r, 0, Math.PI, -arc1.XDirection, -arc1.YDirection);
            return new List<Curve> { arc1, arc2 };
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Ellipse to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.Ellipse to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.Ellipse.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Ellipse curve)
        {
            XYZ centre = curve.Centre.ToRevit();
            double radius1 = curve.Radius1.FromSI(SpecTypeId.Length);
            double radius2 = curve.Radius2.FromSI(SpecTypeId.Length);
            XYZ axis1 = curve.Axis1.ToRevit().Normalize();
            XYZ axis2 = curve.Axis2.ToRevit().Normalize();
            return new List<Curve> { Ellipse.CreateCurve(centre, radius1, radius2, axis1, axis2, 0, Math.PI), Ellipse.CreateCurve(centre, radius1, radius2, axis1, axis2, Math.PI, Math.PI * 2) };
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.NurbsCurve to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.NurbsCurve to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.NurbsCurve.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.NurbsCurve curve)
        {
            if (curve.ControlPoints.Count == 2)
                return new List<Curve> { new oM.Geometry.Line { Start = curve.ControlPoints[0], End = curve.ControlPoints[1] }.ToRevit() };
            else
            {
                List<double> knots = curve.Knots.ToList();
                knots.Insert(0, knots[0]);
                knots.Add(knots[knots.Count - 1]);
                List<XYZ> controlPoints = curve.ControlPoints.Select(x => x.ToRevit()).ToList();

                try
                {
                    Curve nc = NurbSpline.CreateCurve(curve.Degree(), knots, controlPoints, curve.Weights);

                    //Split the curve in half when it is closed.
                    if (nc.GetEndPoint(0).DistanceTo(nc.GetEndPoint(1)) <= BH.oM.Adapters.Revit.Tolerance.Vertex)
                    {
                        double param1 = nc.GetEndParameter(0);
                        double param2 = nc.GetEndParameter(1);
                        Curve c1 = nc.DeepClone();
                        Curve c2 = nc.DeepClone();
                        c1.MakeBound(param1, (param1 + param2) * 0.5);
                        c2.MakeBound((param1 + param2) * 0.5, param2);
                        return new List<Curve> { c1, c2 };
                    }
                    else
                        return new List<Curve> { nc };
                }
                catch
                {
                    BH.Engine.Base.Compute.RecordWarning("Conversion of a nurbs curve from BHoM to Revit failed. An approximate, 100-segment polyline has been created instead.");
                    
                    List<XYZ> pts = new List<XYZ>();
                    int k = 100;
                    for (int i = 0; i <= k; i++)
                    {
                        double t = i / (double)k;
                        pts.Add(curve.PointAtParameter(t).ToRevit());
                    }

                    List<Curve> result = new List<Curve>();
                    for (int i = 1; i < pts.Count; i++)
                    {
                        result.Add(Line.CreateBound(pts[i - 1], pts[i]));
                    }

                    return result;
                }
            }
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.PolyCurve to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.PolyCurve to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.PolyCurve.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.PolyCurve curve)
        {
            List<Curve> result = new List<Curve>();
            foreach (BH.oM.Geometry.ICurve cc in curve.SubParts())
            {
                result.AddRange(cc.IToRevitCurves());
            }

            return result;
        }

        /***************************************************/

        [Description("Converts BH.oM.Geometry.Polyline to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.Polyline to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.Polyline.")]
        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Polyline curve)
        {
            return curve.SubParts().Select(x => x.ToRevit() as Curve).ToList();
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Converts BH.oM.Geometry.ICurve to a collection of Revit Curves.")]
        [Input("curve", "BH.oM.Geometry.ICurve to be converted.")]
        [Output("curves", "Collection of Revit Curves resulting from converting the input BH.oM.Geometry.ICurve.")]
        public static List<Curve> IToRevitCurves(this BH.oM.Geometry.ICurve curve)
        {
            return ToRevitCurves(curve as dynamic);
        }

        /***************************************************/
    }
}


