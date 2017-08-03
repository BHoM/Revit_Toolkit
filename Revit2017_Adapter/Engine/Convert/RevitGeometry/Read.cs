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
        /// Converts Revit Internal Units (Feet) to Meter.
        /// </summary>
        public static double FeetToMeter(double dub)
        {
            return DBG.UnitUtils.ConvertFromInternalUnits(dub, DBG.DisplayUnitType.DUT_METERS);
        }

        //Radians
        /// <summary>
        /// Converts Revit Internal Units (Degrees) to  Radians.
        /// </summary>
        public static double DegToRad(double dub)
        {
            return DBG.UnitUtils.ConvertFromInternalUnits(dub, DBG.DisplayUnitType.DUT_RADIANS);
        }

        /**********************************************/
        /****  Generic                             ****/
        /**********************************************/

        // Geometry
        /// <summary>
        /// Identifies Revit Geometry to use appropriate converter.
        /// </summary>
        public static object Read(object geo)
        {
            if (geo is BHG.Line)
            {
                return Read(geo as BHG.Line);
            }
            else if (geo is BHG.Polyline)
            {
                return Read(geo as BHG.Polyline);
            }
            else if (geo is BHG.Point)
            {
                return Read(geo as BHG.Point);
            }
            else if (geo is BHG.Vector)
            {
                return Read(geo as BHG.Vector);
            }

            return null;
        }

        /**********************************************/
        /****  Curves                              ****/
        /**********************************************/

        // Line
        /// <summary>
        /// Converts Revit Line to BHoM Line.
        /// </summary>
        public static BHG.Line Read(DBG.Line line)
        {
            return new BHG.Line(Read(line.GetEndPoint(0)), Read(line.GetEndPoint(1)));
        }

        // Polyline
        /// <summary>
        /// Converts Revit Polyline to BHoM Polyline.
        /// </summary>
        public static BHG.Polyline Read(DBG.PolyLine polyline)
        {
            List<BHG.Point> points = new List<BHG.Point>();
            foreach (DBG.XYZ point in polyline.GetCoordinates())
            {
                points.Add(Read(point));
            }
            return new BHG.Polyline(points);
        }

        /**********************************************/
        /****  Points & Vectors                    ****/
        /**********************************************/

        // Point
        /// <summary>
        /// Converts Revit XYZ to BHoM Point.
        /// </summary>
        public static BHG.Point Read(DBG.XYZ pt)
        {
            return new BHG.Point(FeetToMeter(pt.X), FeetToMeter(pt.Y), FeetToMeter(pt.Z));
        }

        // Vector
        /// <summary>
        /// Converts Revit XYZ to BHoM Vector. Either in Meter or in original units.
        /// </summary>
        public static BHG.Vector Read(DBG.XYZ Vector, bool toMeter)
        {
            if (toMeter)
            {
                return new BHG.Vector(FeetToMeter(Vector.X), FeetToMeter(Vector.Y), FeetToMeter(Vector.Z));
            }
            else
            {
                return new BHG.Vector(Vector.X, Vector.Y, Vector.Z);
            }
        }

        // Plane
        /// <summary>
        /// Converts Revit Plane to BHoM Plane.
        /// </summary>
        public static BHG.Plane Read(DBG.Plane pln)
        {
            return new BHG.Plane(Read(pln.Origin),Read(pln.Normal, false));
        }
    }
}
