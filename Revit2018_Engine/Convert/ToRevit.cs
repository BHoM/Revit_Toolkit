using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Environmental.Elements;
using BH.oM.Environmental.Properties;
using BH.oM.Geometry;

using Autodesk.Revit.DB;
using BH.oM.Structural.Elements;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        public static XYZ ToRevit(this oM.Geometry.Point Point)
        {
            return new XYZ(Point.X, Point.Y, Point.Z);
        }

        public static XYZ ToRevit(this Vector Vector)
        {
            return new XYZ(Vector.X, Vector.Y, Vector.Z);
        }

        public static Curve ToRevit(this ICurve ICurve)
        {
            if (ICurve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = ICurve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aLine.Start), ToRevit(aLine.End));
            }

            if (ICurve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = ICurve as oM.Geometry.Arc;
                return Autodesk.Revit.DB.Arc.Create(ToRevit(aArc.Start), ToRevit(aArc.End), ToRevit(aArc.Middle));
            }

            if (ICurve is NurbCurve)
            {
                NurbCurve aNurbCurve = ICurve as NurbCurve;
                return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevit(x)), false));
            }

            if (ICurve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = ICurve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevit(aEllipse.Centre), aEllipse.Radius1, aEllipse.Radius2, ToRevit(aEllipse.Axis1), ToRevit(aEllipse.Axis2), 0, 1);
            }

            return null;
        }

        public static CurveArray ToRevit(this PolyCurve PolyCurve)
        {
            if (PolyCurve == null)
                return null;

            CurveArray aCurveArray = new CurveArray();
            foreach (ICurve aICurve in PolyCurve.Curves)
                aCurveArray.Append(aICurve.ToRevit());

            return aCurveArray;
        }

        public static ElementType ToRevit(this BuildingElementProperties BuildingElementProperties, Document Document)
        {
            if (BuildingElementProperties == null || Document == null)
                return null;

            ElementType aElementType = null;
            switch (BuildingElementProperties.BuildingElementType)
            {
                case BuidingElementType.Ceiling:
                    aElementType = new FilteredElementCollector(Document).OfClass(typeof(CeilingType)).First() as ElementType;
                    break;
                case BuidingElementType.Floor:
                    aElementType = new FilteredElementCollector(Document).OfClass(typeof(FloorType)).First() as ElementType;
                    break;
                case BuidingElementType.Roof:
                    aElementType = new FilteredElementCollector(Document).OfClass(typeof(RoofType)).First() as ElementType;
                    break;
                case BuidingElementType.Wall:
                    aElementType = new FilteredElementCollector(Document).OfClass(typeof(WallType)).First() as ElementType;
                    break;
            }

            aElementType = aElementType.Duplicate(BuildingElementProperties.Name) as WallType;

            return aElementType;
        }

        public static Element ToRevit(this BuildingElement BuildingElement, Document Document)
        {
            if (BuildingElement == null || BuildingElement.BuildingElementProperties == null || Document == null)
                return null;

            Storey aStorey = BuildingElement.Storey;
            if (aStorey == null)
                return null;

            List<Level> aLevelList = new FilteredElementCollector(Document).OfClass(typeof(Level)).Cast<Level>().ToList();

            Level aLevel = aLevelList.Find(x => x.Elevation == aStorey.Elevation);
            if (aLevel == null)
                aLevel = aStorey.ToRevit(Document);

            ICurve aICurve = BuildingElement.BuildingElementGeometry.Curve;

            Element aElement = null;
            switch (BuildingElement.BuildingElementProperties.BuildingElementType)
            {
                case BuidingElementType.Ceiling:
                    break;
                case BuidingElementType.Floor:
                    break;
                case BuidingElementType.Roof:
                    break;
                case BuidingElementType.Wall:
                    aElement = Wall.Create(Document, ToRevit(aICurve), aLevel.Id, false);
                    break;
            }

            return aElement;
        }

        public static Level ToRevit(this Storey Storey, Document Document)
        {
            Level aLevel = Level.Create(Document, Storey.Elevation);
            aLevel.Name = Storey.Name;
            return aLevel;
        }
    }
}
