using Autodesk.Revit.DB;
using BH = BHoM.Structural;
using Geom = BHoM.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM.Global;

namespace Revit2015IO
{
    public class PanelIO
    {
        public static bool GetSlabs(out List<BH.Panel> panels, Document document, int rounding)
        {
            panels = new List<BH.Panel>();
            ICollection<Floor> floors = new FilteredElementCollector(document).OfClass(typeof(Floor)).Cast<Floor>().ToList();

            ObjectManager<string, BH.Panel> panelManager = new ObjectManager<string, BH.Panel>("Revit Number", FilterOption.UserData);
            ObjectManager<BH.ThicknessProperty> thicknessManager = new ObjectManager<BHoM.Structural.ThicknessProperty>();
            foreach (Floor floor in floors)
            {
                Geom.Group<Geom.Curve> curves = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
                GeometryElement geometry = floor.get_Geometry(new Options());
                foreach (GeometryObject obj in geometry)
                {
                    if (obj is Solid)
                    {
                        foreach (Face face in (obj as Solid).Faces)
                        if (face is PlanarFace && (face as PlanarFace).Normal.AngleTo(XYZ.BasisZ) < Math.PI / 6)
                        {
                            foreach (EdgeArray curveArray in face.EdgeLoops)
                            {
                                foreach (Edge c in curveArray)
                                {
                                    curves.Add(GeometryUtils.Convert(c.AsCurve(), rounding));
                                }
                            }
                            break;
                        }
                    }
                }
                if (thicknessManager[floor.FloorType.Name] == null)
                {
                    thicknessManager.Add(floor.FloorType.Name, SectionIO.GetThicknessProperty(floor, document));
                }
                BH.ThicknessProperty thickness = thicknessManager[floor.FloorType.Name];
                List<Geom.Curve> crvs = Geom.Curve.Join(curves);
                crvs.Sort(delegate (Geom.Curve c1, Geom.Curve c2)
                {
                    return c2.Length.CompareTo(c1.Length);
                });

                BH.Panel panel = new BHoM.Structural.Panel(new Geom.Group<Geom.Curve>(crvs));             
            }

            return true;
        }

        private static bool IsInside(Geom.Curve c, List<Geom.Curve> crvs)
        {
            List<Geom.Point> pnts = c.ControlPoints.ToList();
            for (int i = 0; i < crvs.Count; i++)
            {
                if (!crvs[i].Equals(c))
                {
                    if (crvs[i].ContainsPoints(pnts))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool GetWalls(out List<BH.Panel> panels, Document document, int rounding)
        {
            panels = new List<BH.Panel>();
            ICollection<Wall> walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();

            ObjectManager<string, BH.Panel> panelManager = new ObjectManager<string, BH.Panel>("Revit Number", FilterOption.UserData);
            ObjectManager<BH.ThicknessProperty> thicknessManager = new ObjectManager<BHoM.Structural.ThicknessProperty>();
            foreach (Wall wall in walls)
            {
                if (wall.Location is LocationCurve)
                {
                    Curve c = (wall.Location as LocationCurve).Curve;

                    ElementId baseConst = wall.LookupParameter("Base Constraint").AsElementId();
                    ElementId topConst = wall.LookupParameter("Top Constraint").AsElementId();

                    double baseOffset = wall.LookupParameter("Base Offset").AsDouble();
                    double topOffset = wall.LookupParameter("Top Offset").AsDouble();

                    Geom.Group<Geom.Curve> curves = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
                    if (baseConst.IntegerValue > 0 && topConst.IntegerValue > 0)
                    {
                        double baseLevel = (document.GetElement(baseConst) as Level).ProjectElevation + baseOffset;
                        double topLevel = (document.GetElement(topConst) as Level).ProjectElevation + topOffset;

                        XYZ p1 = c.GetEndPoint(0);
                        XYZ p2 = c.GetEndPoint(1);
                        XYZ basePoint1 = new XYZ(p1.X, p1.Y, baseLevel);
                        XYZ basePoint2 = new XYZ(p2.X, p2.Y, baseLevel);
                        XYZ topPoint1 = new XYZ(p2.X, p2.Y, topLevel);
                        XYZ topPoint2 = new XYZ(p1.X, p1.Y, topLevel);

                        curves.Add(new Geom.Line(GeometryUtils.Convert(basePoint1, rounding), GeometryUtils.Convert(basePoint2, rounding)));
                        curves.Add(new Geom.Line(GeometryUtils.Convert(basePoint2, rounding), GeometryUtils.Convert(topPoint1, rounding)));
                        curves.Add(new Geom.Line(GeometryUtils.Convert(topPoint1, rounding), GeometryUtils.Convert(topPoint2, rounding)));
                        curves.Add(new Geom.Line(GeometryUtils.Convert(topPoint2, rounding), GeometryUtils.Convert(basePoint1, rounding)));
                    }

                    if (thicknessManager[wall.WallType.Name] == null)
                    {
                        thicknessManager.Add(wall.WallType.Name, SectionIO.GetThicknessProperty(wall, document));
                    }
                    BH.ThicknessProperty thickness = thicknessManager[wall.WallType.Name];
                    BH.Panel panel = new BHoM.Structural.Panel(Geom.Curve.Join(curves)[0]);
                    panelManager.Add(wall.Id.IntegerValue.ToString(), panel);
                    panel.ThicknessProperty = thickness;
                    panels.Add(panel);
                }
            }

            return true;
        }
    }
}