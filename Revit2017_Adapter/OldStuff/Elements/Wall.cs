
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitToolkit.Elements
{
    /// <summary>
    /// A Revit Wal
    /// </summary>
    public static class Wall
    {
        /// <summary>
        /// Create BHoM Panel from Revit Wall
        /// </summary>
        public static object ToBHomPanel(Autodesk.Revit.DB.Wall wall)
        {
            double scale = RevitToolkit.Global.GeometryConverter.FeetToMetre;
            GeometryElement geometry = wall.get_Geometry(new Options());
            BoundingBoxXYZ box = geometry.GetBoundingBox();
            XYZ center = (box.Max + box.Min) / 2;
            XYZ normal = wall.Orientation;
            normal.Normalize();

            /*BHoM.Geometry.Group<BHoM.Geometry.Curve> edges = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
            foreach (GeometryObject obj in geometry)
            {
                Autodesk.Revit.DB.Solid solid = obj as Autodesk.Revit.DB.Solid;
                if (solid != null)
                {
                    foreach (Autodesk.Revit.DB.Edge edge in solid.Edges)
                    {
                        Autodesk.Revit.DB.Curve curve = edge.AsCurve();
                        XYZ cStart = curve.GetEndPoint(0);
                        XYZ cEnd = curve.GetEndPoint(1);
                        double dist  = normal.DotProduct(cStart - center);
                        if ( dist > 0 && normal.DotProduct(cEnd - center) > 0)
                        {
                            XYZ pt0 = scale * (cStart - dist * normal);
                            XYZ pt1 = scale * (cEnd - dist * normal);
                            edges.Add(new BHoM.Geometry.Line(new BHoM.Geometry.Point(pt0.X, pt0.Y, pt0.Z), new BHoM.Geometry.Point(pt1.X, pt1.Y, pt1.Z)));
                        }
                    }
                }
            }*/

            double maxArea = 0;
            Face maxFace = null;
            BHoM.Geometry.Group<BHoM.Geometry.Curve> edges = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
            foreach (GeometryObject obj in geometry)
            {
                Solid solid = obj as Solid;
                if (solid != null)
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (face.Area > maxArea)
                        {
                            maxArea = face.Area;
                            maxFace = face;
                        }
                    }
                }
            }

            if (maxFace != null)
            {
                foreach (EdgeArray array in maxFace.EdgeLoops)
                {
                    foreach (Edge edge in array)
                    {
                        Curve curve = edge.AsCurve();
                        XYZ cStart = curve.GetEndPoint(0);
                        XYZ cEnd = curve.GetEndPoint(1);
                        double dist = normal.DotProduct(cStart - center);
                        XYZ pt0 = scale * (cStart - dist * normal);
                        XYZ pt1 = scale * (cEnd - dist * normal);
                        edges.Add(new BHoM.Geometry.Line(new BHoM.Geometry.Point(pt0.X, pt0.Y, pt0.Z), new BHoM.Geometry.Point(pt1.X, pt1.Y, pt1.Z)));
                    }
                }
            }

            /*BHoM.Geometry.Group<BHoM.Geometry.Curve> edges = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
            Autodesk.Revit.DB.Structure.AnalyticalModel model = wall.GetAnalyticalModel();

            IList<Autodesk.Revit.DB.Curve> c1 = model.GetCurves(Autodesk.Revit.DB.Structure.AnalyticalCurveType.ActiveCurves);
            IList<Autodesk.Revit.DB.Curve> c2 = model.GetCurves(Autodesk.Revit.DB.Structure.AnalyticalCurveType.ApproximatedCurves);
            IList<Autodesk.Revit.DB.Curve> c3 = model.GetCurves(Autodesk.Revit.DB.Structure.AnalyticalCurveType.BaseCurve);
            IList<Autodesk.Revit.DB.Curve> c4 = model.GetCurves(Autodesk.Revit.DB.Structure.AnalyticalCurveType.RawCurves);

            foreach (Autodesk.Revit.DB.Curve curve in model.GetCurves(Autodesk.Revit.DB.Structure.AnalyticalCurveType.RawCurves))
            {
                XYZ cStart = curve.GetEndPoint(0);
                XYZ cEnd = curve.GetEndPoint(1);
                double dist = normal.DotProduct(cStart - center);
                if (dist > 0 && normal.DotProduct(cStart - center) > 0)
                {
                    XYZ pt0 = scale * (cStart - dist * normal);
                    XYZ pt1 = scale * (cEnd - dist * normal);
                    edges.Add(new BHoM.Geometry.Line(new BHoM.Geometry.Point(pt0.X, pt0.Y, pt0.Z), new BHoM.Geometry.Point(pt1.X, pt1.Y, pt1.Z)));
                }
            }*/

            /*List<BHoM.Geometry.Point> contourPts = new List<BHoM.Geometry.Point>();

            if (wall.Location is Autodesk.Revit.DB.LocationCurve)
            {
                Autodesk.Revit.DB.Curve c = (wall.Location as LocationCurve).Curve;

                ElementId baseConst = wall.LookupParameter("Base Constraint").AsElementId();
                ElementId topConst = wall.LookupParameter("Top Constraint").AsElementId();

                double baseOffset = wall.LookupParameter("Base Offset").AsDouble();
                double topOffset = wall.LookupParameter("Top Offset").AsDouble();

                if (baseConst.IntegerValue > 0 && topConst.IntegerValue > 0)
                {
                    Autodesk.Revit.DB.Document document = wall.Document;
                    double baseLevel = scale * (document.GetElement(baseConst) as Level).ProjectElevation + baseOffset;
                    double topLevel = scale * (document.GetElement(topConst) as Level).ProjectElevation + topOffset;

                    XYZ p1 = scale * c.GetEndPoint(0);
                    XYZ p2 = scale * c.GetEndPoint(1);

                    contourPts.Add(new BHoM.Geometry.Point(p1.X, p1.Y, baseLevel));
                    contourPts.Add(new BHoM.Geometry.Point(p2.X, p2.Y, baseLevel));
                    contourPts.Add(new BHoM.Geometry.Point(p2.X, p2.Y, topLevel));
                    contourPts.Add(new BHoM.Geometry.Point(p1.X, p1.Y, topLevel));
                    contourPts.Add(new BHoM.Geometry.Point(p1.X, p1.Y, baseLevel));
                }
            }*/

            /*BHoM.Geometry.Polyline contour = new BHoM.Geometry.Polyline(contourPts);
            BHoM.Geometry.Group<BHoM.Geometry.Curve> group = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
            group.Add(contour);*/

            BHoM.Structural.Panel panel = new BHoM.Structural.Panel(edges);
            panel.ThicknessProperty = new BHoM.Structural.ConstantThickness(wall.Name);
            panel.ThicknessProperty.Thickness = scale * wall.WallType.LookupParameter("Width").AsDouble();
            panel.CustomData["RevitId"] = wall.Id;
            panel.CustomData["RevitType"] = "Wall";

            return panel;
        }
    }
}
