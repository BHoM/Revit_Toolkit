using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Environmental.Elements;
using BH.oM.Environmental.Properties;
using BH.oM.Geometry;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        public static XYZ ToRevitPoint(this oM.Geometry.Point Point)
        {
            return new XYZ(Point.X, Point.Y, Point.Z);
        }

        public static XYZ ToRevitVector(this Vector Vector)
        {
            return new XYZ(Vector.X, Vector.Y, Vector.Z);
        }

        public static Curve ToRevitCurve(this ICurve ICurve)
        {
            if (ICurve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = ICurve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevitPoint(aLine.Start), ToRevitPoint(aLine.End));
            }

            if (ICurve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = ICurve as oM.Geometry.Arc;
                return Autodesk.Revit.DB.Arc.Create(ToRevitPoint(aArc.Start), ToRevitPoint(aArc.End), ToRevitPoint(aArc.Middle));
            }

            if (ICurve is NurbCurve)
            {
                NurbCurve aNurbCurve = ICurve as NurbCurve;
                return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevitPoint(x)), false));
            }

            if (ICurve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = ICurve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevitPoint(aEllipse.Centre), aEllipse.Radius1, aEllipse.Radius2, ToRevitVector(aEllipse.Axis1), ToRevitVector(aEllipse.Axis2), 0, 1);
            }

            return null;
        }

        public static WallType ToRevitWallType(this BuildingElementProperties BuildingElementProperties, Document Document)
        {
            if (BuildingElementProperties.BuildingElementType != BuidingElementType.Wall)
                return null;

            WallType aWallType = new FilteredElementCollector(Document).OfClass(typeof(WallType)).First() as WallType;
            if (aWallType == null)
                return null;

            aWallType = aWallType.Duplicate(BuildingElementProperties.Name) as WallType;

            return aWallType;

        }

        public static Wall ToRevitWall(this BuildingElement BuildingElement, Document Document)
        {
            if(BuildingElement.BuildingElementGeometry is BuildingElementCurve)
            {
                ICurve aICurve = (BuildingElement.BuildingElementGeometry as BuildingElementCurve).ICurve;

                throw new NotImplementedException();
                //Wall aWall = Wall.Create(Document, ToRevitCurve(aICurve), );

                //return aWall;
            }

            return null;
        }
    }
}
