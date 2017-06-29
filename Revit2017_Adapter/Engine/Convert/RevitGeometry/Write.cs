using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBG = Autodesk.Revit.DB;
using BHG = BHoM.Geometry;

namespace Engine.Convert
{
    public static partial class RevitGeometry
    {
        /**********************************************/
        /****  Units                               ****/
        /**********************************************/

        //Meter
        /// <summary>
        /// Converts Meter to Revit Internal Units (Feet).
        /// </summary>
        public static double MeterToFeet(double dub)
        {
            return DBG.UnitUtils.ConvertToInternalUnits(dub, DBG.DisplayUnitType.DUT_METERS);
        }

        //Millimeter
        /// <summary>
        /// Converts Millimeter to Revit Internal Units (Feet).
        /// </summary>
        public static double MmToFeet(double dub)
        {
            return DBG.UnitUtils.ConvertToInternalUnits(dub, DBG.DisplayUnitType.DUT_MILLIMETERS);
        }

        //Radians
        /// <summary>
        /// Converts Radians to Revit Internal Units (Degrees).
        /// </summary>
        public static double RadToDeg(double dub)
        {
            return DBG.UnitUtils.ConvertToInternalUnits(dub, DBG.DisplayUnitType.DUT_RADIANS);
        }

        /**********************************************/
        /****  Generic                             ****/
        /**********************************************/

        // Geometry
        /// <summary>
        /// Identifies BHoM Geometry to use appropriate converter.
        /// </summary>
        public static object Write(object geo)
        {
            if (geo is BHG.Line)
            {
                return Write(geo as BHG.Line);
            }
            else if (geo is BHG.Polyline)
            {
                return Write(geo as BHG.Polyline);
            }
            else if (geo is BHG.Point)
            {
                return Write(geo as BHG.Point);
            }
            else if (geo is BHG.Vector)
            {
                return Write(geo as BHG.Vector);
            }
            else if (geo is BHG.Plane)
            {
                return Write(geo as BHG.Plane);
            }
            else if (geo is BHG.Arc)
            {
                return Write(geo as BHG.Arc);
            }
            else if (geo is BHG.Circle)
            {
                return Write(geo as BHG.Circle);
            }
            else if (geo is BHG.NurbCurve)
            {
                return Write(geo as BHG.NurbCurve);
            }
            return null;
        }

        /**********************************************/
        /****  Curves                              ****/
        /**********************************************/

        // Curve
        /// <summary>
        /// Identifies BHoM Curve to use appropriate converter.
        /// </summary>
        public static DBG.Curve Write(BHG.Curve geo)
        {
            if (geo is BHG.Line)
            {
                return Write(geo as BHG.Line);
            }
            else if (geo is BHG.Arc)
            {
                return Write(geo as BHG.Arc);
            }
            else if (geo is BHG.Circle)
            {
                return Write(geo as BHG.Circle);
            }
            else if (geo is BHG.NurbCurve)
            {
                return Write(geo as BHG.NurbCurve);
            }
            return null;
        }

        // CurveGroup
        /// <summary>
        /// Converts BHoMGroup of Curves to Revit CurveArrArray.
        /// </summary>
        public static DBG.CurveArrArray Write(BHG.Group<BHG.Curve> curves)
        {
            List<BHG.Curve> loops = BHG.Curve.Join(curves);
            DBG.CurveArrArray revitCurves = new DBG.CurveArrArray();
            foreach (BHG.Curve perimeterCurve in loops)
            {
                DBG.CurveArray revitPerimeter = new DBG.CurveArray();
                foreach (BHG.Curve segment in perimeterCurve.Explode())
                {
                    revitPerimeter.Append(Write(segment));
                }
                revitCurves.Append(revitPerimeter);
            }
            return revitCurves;
        }

        // Line
        /// <summary>
        /// Converts BHoM Line to Revit Line.
        /// </summary>
        public static DBG.Line Write(BHG.Line line)
        {
            return DBG.Line.CreateBound(Write(line.StartPoint), Write(line.EndPoint));
        }

        // Polyline
        /// <summary>
        /// Converts BHoM Polyline to Revit Polyline.
        /// </summary>
        public static DBG.PolyLine Write(BHG.Polyline polyline)
        {
            IList<DBG.XYZ> points = new List<DBG.XYZ>();
            foreach (BHG.Point point in polyline.ControlPoints)
            {
                points.Add(Write(point));
            }
            return DBG.PolyLine.Create(points);
        }

        // Arc
        /// <summary>
        /// Converts BHoM Arc to Revit Arc.
        /// </summary>
        public static DBG.Arc Write(BHG.Arc arc)
        {
            return DBG.Arc.Create(Write(arc.StartPoint), Write(arc.EndPoint), Write(arc.MiddlePoint));
        }

        // Circle
        /// <summary>
        /// Converts BHoM Circle to Revit Arc.
        /// </summary>
        public static DBG.Arc Write(BHG.Circle cir)
        {
            BHG.Vector Xaxis = cir.Centre - cir.StartPoint;
            BHG.Vector Yaxis = BHG.Vector.CrossProduct(cir.Plane.Normal, Xaxis);
            Xaxis.Unitize();
            Yaxis.Unitize();
            return DBG.Arc.Create(Write(cir.Centre), cir.Radius, 0.0, 2.0 * Math.PI, Write(Xaxis, false),Write(Yaxis,false));
        }

        // NurbCurve
        /// <summary>
        /// Converts BHoM NurbCurve to Revit NurbSpline.
        /// </summary>
        public static DBG.Curve Write(BHG.NurbCurve spline)
        {
                return DBG.NurbSpline.CreateCurve(spline.Degree, spline.Knots, Write(spline.ControlPoints).ToList());
        }

        /**********************************************/
        /****  Point, Vector, Plane                ****/
        /**********************************************/

        // Point
        /// <summary>
        /// Converts BHoM Point to Revit XYZ.
        /// </summary>
        public static DBG.XYZ Write(BHG.Point pt)
        {
            return new DBG.XYZ(MeterToFeet(pt.X), MeterToFeet(pt.Y), MeterToFeet(pt.Z));
        }

        /// <summary>
        /// Converts BHoM Point to Revit XYZ with rounding.
        /// </summary>
        public static DBG.XYZ Write(BHG.Point point, int rounding = 9)
        {
            return new DBG.XYZ(MeterToFeet(Math.Round(point.X,rounding)), MeterToFeet(Math.Round(point.Y, rounding)), MeterToFeet(Math.Round(point.Z, rounding)));
        }

        // Point List
        /// <summary>
        /// Converts List of BHoM Point to List of Revit XYZ.
        /// </summary>
        public static IEnumerable<DBG.XYZ> Write(List<BHG.Point> points)
        {
            List<DBG.XYZ> bhPoints = new List<DBG.XYZ>();
            foreach (BHG.Point point in points)
            {
                bhPoints.Add(Write(point));
            }
            return bhPoints;
        }

        /// <summary>
        /// Converts List of BHoM Point to List of Revit XYZ with rounding.
        /// </summary>
        public static IEnumerable<DBG.XYZ> Write(List<BHG.Point> points, int rounding = 9)
        {
            List<DBG.XYZ> bhPoints = new List<DBG.XYZ>();
            foreach (BHG.Point point in points)
            {
                bhPoints.Add(Write(point,rounding));
            }
            return bhPoints;
        }

        // Vector
        /// <summary>
        /// Converts BHoM Vector to Revit XYZ. Either in feet or in original units.
        /// </summary>
        public static DBG.XYZ Write(BHG.Vector Vector, bool toFeet)
        {
            if (toFeet)
            {
                return new DBG.XYZ(MeterToFeet(Vector.X), MeterToFeet(Vector.Y), MeterToFeet(Vector.Z));
            }
            else
            {
                return new DBG.XYZ(Vector.X, Vector.Y, Vector.Z);
            }
        }

        // Plane
        /// <summary>
        /// Converts BHoM Plane to Revit Plane.
        /// </summary>
        public static DBG.Plane Write(BHG.Plane pln)
        {
            return new DBG.Plane(Write(pln.Normal, false), Write(pln.Origin));
        }

        /**********************************************/
        /****  Misc                                ****/
        /**********************************************/

        // CurveGroup
        /// <summary>
        /// Converts BHoMGroup of Curves to Revit CurveArrArray.
        /// </summary>
        public static DBG.CurveArrArray OrientSectionToYZPlane(BHG.Group<BHG.Curve> curves,BHG.Plane pln)
        {
            List<BHG.Curve> loops = BHG.Curve.Join(curves);

            foreach (BHoM.Geometry.Curve crv in loops)
            {
                crv.Transform(BHoM.Geometry.Transform.Rotation(new BHoM.Geometry.Point(0, 0, 0), new BHoM.Geometry.Vector(0, 0, 1), Math.PI / 2));
                crv.Transform(BHoM.Geometry.Transform.Rotation(new BHoM.Geometry.Point(0, 0, 0), new BHoM.Geometry.Vector(0, 1, 0), Math.PI / 2));
                crv.Translate(new BHG.Vector(pln.Origin));
            }

            DBG.CurveArrArray revitCurves = new DBG.CurveArrArray();
            foreach (BHG.Curve perimeterCurve in loops)
            {
                DBG.CurveArray revitPerimeter = new DBG.CurveArray();
                foreach (BHG.Curve segment in perimeterCurve.Explode())
                {
                    revitPerimeter.Append(Write(segment));
                }
                revitCurves.Append(revitPerimeter);
            }
            return revitCurves;
        }
        public static bool CheckClosed(DBG.CurveArrArray arr)
        {
            foreach (DBG.CurveArray crvarr in arr)
            {
                int counter = 0;
                int i = 0;
                while (i < crvarr.Size)
                {
                    if (i != crvarr.Size-1)
                    {
                        counter = i + 1;
                    }
                    else
                    {
                        counter = 0;
                    }
                    DBG.Curve crv1 = crvarr.get_Item(i);
                    DBG.Curve crv2 = crvarr.get_Item(counter);
                    if (crv1.GetEndPoint(1).DistanceTo(crv2.GetEndPoint(0))> 0.001)
                    {
                        return false;
                    }
                    i = i + 1;
                }
            }
            return true;
        }
        public static bool CheckPlanar(DBG.CurveArrArray arr, DBG.Plane pln)
        {
            foreach (DBG.CurveArray crvarr in arr)
            {
                foreach (DBG.Curve crv in crvarr)
                {
                    if ((crv.GetEndPoint(0).X - pln.Origin.X > 0.001) || (crv.GetEndPoint(1).X - pln.Origin.X > 0.001))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static DBG.CurveArrArray EdgesToCurves(DBG.EdgeArrayArray edgarrarr)
        {
            DBG.CurveArrArray crvarrarr = new DBG.CurveArrArray();
            foreach (DBG.EdgeArray edgarr in edgarrarr)
            {
                DBG.CurveArray crvarr = new DBG.CurveArray();
                foreach (DBG.Edge edg in edgarr)
                {
                    crvarr.Append(edg.AsCurve());
                }
                crvarrarr.Append(crvarr);
            }
            bool bo = CheckClosed(crvarrarr);
            return crvarrarr;
        }

        public static bool IsHorizontal(BHG.Plane p)
        {
            return BHG.Vector.VectorAngle(p.Normal, BHG.Vector.ZAxis()) < Math.PI / 48;
        }
        public static bool IsHorizontal(BHG.Group<BHG.Curve> curves)
        {
            double z = curves[0].ControlPoints[0].Z;
            foreach (BHG.Curve crv in curves)
            {
                foreach (BHG.Point pt in crv.ControlPoints)
                {
                    if (Math.Abs(pt.Z - z) > 0.001)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsVertical(BHG.Plane p)
        {
            double angle = BHG.Vector.VectorAngle(p.Normal, BHG.Vector.ZAxis());
            return angle > Math.PI / 2 - Math.PI / 48 && angle < Math.PI / 2 + Math.PI / 48;
        }
    }
}
