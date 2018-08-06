using Autodesk.Revit.DB;
using System;
using System.Linq;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this Curve curve, bool convertUnits = true)
        {
            if (curve is Line)
                return Geometry.Create.Line(ToBHoM(curve.GetEndPoint(0), convertUnits), ToBHoM(curve.GetEndPoint(1), convertUnits));

            if (curve is Arc)
            {
                Arc aArc = curve as Arc;
                double radius = convertUnits ? UnitUtils.ConvertFromInternalUnits(aArc.Radius, DisplayUnitType.DUT_METERS) : aArc.Radius;
                Plane plane = Plane.CreateByOriginAndBasis(aArc.Center, aArc.XDirection, aArc.YDirection);
                double startAngle = aArc.XDirection.AngleOnPlaneTo(aArc.GetEndPoint(0) - aArc.Center, aArc.Normal);
                double endAngle = aArc.XDirection.AngleOnPlaneTo(aArc.GetEndPoint(1) - aArc.Center, aArc.Normal);
                if (startAngle > endAngle)
                {
                    startAngle -= 2 * Math.PI;
                }
                return Geometry.Create.Arc(ToBHoM(plane, convertUnits), radius, startAngle, endAngle);
            }

            //if (curve is NurbSpline)
            //{
            //    NurbSpline aNurbSpline = curve as NurbSpline;
            //    return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => ToBHoM(x, convertUnits)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            //}

            if (curve is Ellipse)
            {
                Ellipse aEllipse = curve as Ellipse;
                return Geometry.Create.Ellipse(ToBHoM(aEllipse.Center, convertUnits), aEllipse.RadiusX, aEllipse.RadiusY);
            }
            
            return null;
        }

        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this List<Curve> curves, bool convertUnits = true)
        {
            return curves.Select(c => c.ToBHoM(convertUnits)).ToList();
        }

        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve, bool convertUnits = true)
        {
            return ToBHoM(locationCurve.Curve, convertUnits);
        }

        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this CurveArray curves, bool convertUnits = true)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            for (int i = 0; i < curves.Size; i++)
            {
                result.Add(curves.get_Item(i).ToBHoM(convertUnits));
            }
            return result;
        }

        /***************************************************/

        internal static oM.Geometry.PolyCurve ToBHoM(this CurveLoop curveLoop, bool convertUnits = true)
        {
            if (curveLoop == null)
                return null;

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in curveLoop)
                aICurveList.Add(aCurve.ToBHoM(convertUnits));

            return Geometry.Create.PolyCurve(aICurveList);
        }

        /***************************************************/
    }
}