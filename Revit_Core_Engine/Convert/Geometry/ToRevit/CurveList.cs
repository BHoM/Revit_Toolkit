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
using System.Linq;
using System.Collections.Generic;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Line curve)
        {
            return new List<Curve> { curve.ToRevit() };
        }

        /***************************************************/

        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Arc curve)
        {
            //Split the curve in half when it is closed - tolerance value taken from Autodesk.Revit.ApplicationServices.Application.AngleTolerance constant.
            if (Math.Abs(2 * Math.PI) - curve.EndAngle + curve.StartAngle < 0.00174532925199433)
            {
                double r = curve.Radius.FromSI(UnitType.UT_Length);
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

        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Circle curve)
        {
            double r = curve.Radius.FromSI(UnitType.UT_Length);

            XYZ centre = curve.Centre.ToRevit();
            XYZ normal = curve.Normal.ToRevit().Normalize();
            Plane p = Plane.CreateByNormalAndOrigin(normal, centre);

            Arc arc1 = Arc.Create(p, r, 0, Math.PI);
            Arc arc2 = Arc.Create(centre, r, 0, Math.PI, -arc1.XDirection, -arc1.YDirection);
            return new List<Curve> { arc1, arc2 };
        }

        /***************************************************/

        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Ellipse curve)
        {
            XYZ centre = curve.Centre.ToRevit();
            double radius1 = curve.Radius1.FromSI(UnitType.UT_Length);
            double radius2 = curve.Radius2.FromSI(UnitType.UT_Length);
            XYZ axis1 = curve.Axis1.ToRevit().Normalize();
            XYZ axis2 = curve.Axis2.ToRevit().Normalize();
            return new List<Curve> { Ellipse.CreateCurve(centre, radius1, radius2, axis1, axis2, 0, Math.PI), Ellipse.CreateCurve(centre, radius1, radius2, axis1, axis2, Math.PI, Math.PI * 2) };
        }

        /***************************************************/

        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.NurbsCurve curve)
        {
            Curve nc = curve.ToRevit();
            if (nc == null)
                return null;

            //Split the curve in half when it is closed - tolerance value taken from Autodesk.Revit.ApplicationServices.Application.VertexTolerance constant.
            if (nc.GetEndPoint(0).DistanceTo(nc.GetEndPoint(1)) <= 0.0005233832795)
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

        /***************************************************/

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

        public static List<Curve> ToRevitCurves(this BH.oM.Geometry.Polyline curve)
        {
            return curve.SubParts().Select(x => x.ToRevit() as Curve).ToList();
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static List<Curve> IToRevitCurves(this BH.oM.Geometry.ICurve curve)
        {
            return ToRevitCurves(curve as dynamic);
        }

        /***************************************************/
    }
}
