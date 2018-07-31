﻿using Autodesk.Revit.DB;
using BH.oM.Geometry;
using System.Linq;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Curve ToRevit(this ICurve curve, bool convertUnits = true)
        {
            if (curve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = curve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aLine.Start, convertUnits), ToRevit(aLine.End, convertUnits));
            }

            if (curve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = curve as oM.Geometry.Arc;
                double radius = convertUnits ? UnitUtils.ConvertToInternalUnits(aArc.Radius, DisplayUnitType.DUT_METERS) : aArc.Radius;
                return Autodesk.Revit.DB.Arc.Create(ToRevit(aArc.CoordinateSystem, convertUnits), radius, aArc.StartAngle, aArc.EndAngle);
            }

            if (curve is NurbCurve)
            {
                NurbCurve aNurbCurve = curve as NurbCurve;
                return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevit(x, convertUnits)), false));
            }

            if (curve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = curve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevit(aEllipse.Centre, convertUnits), aEllipse.Radius1, aEllipse.Radius2, ToRevit(aEllipse.Axis1, convertUnits), ToRevit(aEllipse.Axis2, convertUnits), 0, 1);
            }

            return null;
        }

        /***************************************************/

        public static CurveArray ToRevit(this PolyCurve polyCurve, bool convertUnits = true)
        {
            if (polyCurve == null)
                return null;

            CurveArray aCurveArray = new CurveArray();
            foreach (ICurve aICurve in polyCurve.Curves)
                aCurveArray.Append(aICurve.ToRevit(convertUnits));

            return aCurveArray;
        }

        /***************************************************/
    }
}