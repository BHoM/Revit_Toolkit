using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH = BHoM.Geometry;

namespace RevitToolkit.Global
{
    /// <summary>
    /// Convert between revit geometry and BHoM Geometry
    /// </summary>
    public static class GeometryConverter
    {
        public const double FeetToMetre = 0.3048;
        public const double MetreToFeet = 3.28084;

        /// <summary></summary>
        public static string RoundPoint(BH.Point point, int decimals)
        {
            return Math.Round(point.X, decimals) + "," + Math.Round(point.Y, decimals) + "," + Math.Round(point.Z, decimals);
        }

        /// <summary></summary>
        public static BH.Point Convert(XYZ point)
        {
            return new BHoM.Geometry.Point(point.X * FeetToMetre, point.Y * FeetToMetre, point.Z * FeetToMetre);
        }

        /// <summary></summary>
        public static XYZ Convert(BH.Point point)
        {
            return new XYZ(point.X * MetreToFeet, point.Y * MetreToFeet, point.Z * MetreToFeet);
        }

        /// <summary></summary>
        public static List<BH.Point> Convert(IEnumerable<XYZ> points)
        {
            List<BH.Point> bhPoints = new List<BHoM.Geometry.Point>();
            foreach (XYZ point in points)
            {
                bhPoints.Add(Convert(point));
            }
            return bhPoints;
        }

        /// <summary></summary>
        public static BH.Curve Convert(Curve curve)
        {
            if (curve is Line)
            {
                return new BH.Line(Convert(curve.GetEndPoint(0)), Convert(curve.GetEndPoint(1)));
            }
            else if (curve is Arc)
            {
                return new BH.Arc(Convert(curve.GetEndPoint(0)), Convert(curve.GetEndPoint(1)), Convert(curve.Evaluate(0.5, true)));
            }
            else if (curve is NurbSpline)
            {
                NurbSpline spline = curve as NurbSpline;
                return BH.NurbCurve.Create(Convert(spline.CtrlPoints), spline.Degree, spline.Knots.Cast<double>().ToArray(), spline.Weights.Cast<double>().ToArray());
            }
            return null;
        }


    }
}
