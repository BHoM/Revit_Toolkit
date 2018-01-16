using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Environmental.Properties;
using BH.oM.Structural.Elements;
using BH.oM.Environmental.Elements;
using BH.Engine.Environment;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        public static oM.Geometry.Point FromRevitPoint(this XYZ XYZ)
        {
            return BH.Engine.Geometry.Create.Point(XYZ.X, XYZ.Y, XYZ.Z);
        }

        public static oM.Geometry.Point FromRevitPoint(this LocationPoint LocationPoint)
        {
            return FromRevitPoint(LocationPoint.Point);
        }

        public static oM.Geometry.ICurve FromRevitCurve(this Curve Curve)
        {
            if (Curve is Line)
                return Geometry.Create.Line(FromRevitPoint(Curve.GetEndPoint(0)), FromRevitPoint(Curve.GetEndPoint(1)));

            if (Curve is Arc)
                return Geometry.Create.Arc(FromRevitPoint(Curve.GetEndPoint(0)), FromRevitPoint(Curve.Evaluate(0.5, true)), FromRevitPoint(Curve.GetEndPoint(1)));

            if (Curve is NurbSpline)
            {
                NurbSpline aNurbSpline = Curve as NurbSpline;
                return Geometry.Create.NurbCurve(aNurbSpline.CtrlPoints.Cast<XYZ>().ToList().ConvertAll(x => FromRevitPoint(x)), aNurbSpline.Weights.Cast<double>(), aNurbSpline.Degree);
            }

            if(Curve is Ellipse)
            {
                Ellipse aEllipse = Curve as Ellipse;
                return Geometry.Create.Ellipse(FromRevitPoint(aEllipse.Center), aEllipse.RadiusX, aEllipse.RadiusY);
            }

            return null;
        }

        public static oM.Geometry.ICurve FromRevitCurve(this LocationCurve LocationCurve)
        {
            return FromRevitCurve(LocationCurve.Curve);
        }

        public static BuildingElementCurve FromRevitBuildingElementCurve(this Wall Wall)
        {
            LocationCurve aLocationCurve = Wall.Location as LocationCurve;
            BuildingElementCurve aBuildingElementCurve = new BuildingElementCurve()
            {
                ICurve = FromRevitCurve(aLocationCurve)
            };
            return aBuildingElementCurve;
        }

        public static BuildingElement FromRevitBuildingElement(this Wall Wall)
        {
            BuildingElementProperties aBuildingElementProperties = FromRevitBuildingElementProperties(Wall.WallType);

            return Create.BuildingElement(aBuildingElementProperties, FromRevitBuildingElementCurve(Wall), FromRevitStorey(Wall.Document.GetElement(Wall.LevelId) as Level));
        }

        public static BuildingElementProperties FromRevitBuildingElementProperties(this WallType WallType)
        {
            BuildingElementProperties aBuildingElementProperties = Create.BuildingElementProperties(BuidingElementType.Wall, WallType.Name);
            return aBuildingElementProperties;
        }

        public static Storey FromRevitStorey(this Level Level)
        {
            return BH.Engine.Structure.Create.Storey(Level.Name, Level.Elevation, 0);
        }

        public static Space FromRevitSpace(this SpatialElement SpatialElement)
        {
            SpatialElementBoundaryOptions aSpatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            aSpatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            aSpatialElementBoundaryOptions.StoreFreeBoundaryFaces = false;

            return FromRevitSpace(SpatialElement, aSpatialElementBoundaryOptions);
        }

        public static Space FromRevitSpace(this SpatialElement SpatialElement, SpatialElementBoundaryOptions SpatialElementBoundaryOptions)
        {
            List<BuildingElement> aBuildingElmementList = new List<BuildingElement>();
            IList<IList<BoundarySegment>> aBoundarySegmentListList = SpatialElement.GetBoundarySegments(SpatialElementBoundaryOptions);
            if (aBoundarySegmentListList != null)
                foreach (IList<BoundarySegment> aBoundarySegmentList in aBoundarySegmentListList)
                    foreach (BoundarySegment aBoundarySegment in aBoundarySegmentList)
                    {
                        //BoundaryCurves.Add(aBoundarySegment.GetCurve().ToProtoType(false, null));
                        //BoundaryElements.Add(aDocument.GetElement(aBoundarySegment.ElementId).ToDSType(true));
                    }

            return null;
        }
    }
}
