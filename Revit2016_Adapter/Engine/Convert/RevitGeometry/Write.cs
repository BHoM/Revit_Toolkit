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

        public static double MeterToFeet(double dub)
        {
            return DBG.UnitUtils.ConvertToInternalUnits(dub, DBG.DisplayUnitType.DUT_METERS);
        }

        public static double MmToFeet(double dub)
        {
            return DBG.UnitUtils.ConvertToInternalUnits(dub, DBG.DisplayUnitType.DUT_MILLIMETERS);
        }
        /**********************************************/
        /****  Generic                             ****/
        /**********************************************/

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
            else if (geo is BHG.Arc)
            {
                return Write(geo as BHG.Arc);
            }
            else if (geo is BHG.Circle)
            {
                return Write(geo as BHG.Circle);
            }
            return null;
        }

        /**********************************************/
        /****  Curves                              ****/
        /**********************************************/
        // Line
        /// <summary>
        /// Converts BHoM Line to Revit Line.
        /// </summary>
        public static DBG.Line Write(BHG.Line line)
        {
            return DBG.Line.CreateBound(Write(line.StartPoint), Write(line.EndPoint));
        }

        /**********************************************/
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

        /**********************************************/
        // Arc
        /// <summary>
        /// Converts BHoM Arc to Revit Arc.
        /// </summary>
        public static DBG.Arc Write(BHG.Arc arc)
        {
            return DBG.Arc.Create(Write(arc.StartPoint),Write(arc.EndPoint),Write(arc.PointAt(0.5)));
        }

        /**********************************************/
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

        /**********************************************/
        // CurveGroup
        /// <summary>
        /// Converts BHoMGroup of Curves to Revit CurveArray.
        /// </summary>
        public static DBG.CurveArray Write(BHG.Group<BHG.Curve> crvg)
        {
            DBG.CurveArray crvA = new DBG.CurveArray();
            foreach (BHG.Curve crv in crvg)
            {
                Autodesk.Revit.DB.Curve crvR = (Autodesk.Revit.DB.Curve)RevitGeometry.Write(crv);
                if (crvR != null)
                {
                    crvA.Append(crvR);
                }
            }
            return crvA;
        }

        /**********************************************/
        /****  Points & Vectors                    ****/
        /**********************************************/
        // Point
        /// <summary>
        /// Converts BHoM Point to Revit XYZ.
        /// </summary>
        public static DBG.XYZ Write(BHG.Point pt)
        {
            return new DBG.XYZ(MeterToFeet(pt.X), MeterToFeet(pt.Y), MeterToFeet(pt.Z));
        }

        /**********************************************/
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
    }
}
