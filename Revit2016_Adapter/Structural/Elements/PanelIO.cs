using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoMB = BHoM.Base;
using BHoMG = BHoM.Geometry;
using BHoME = BHoM.Structural.Elements;
using BHoMP = BHoM.Structural.Properties;

using Revit2016_Adapter.Geometry;
using Revit2016_Adapter.Structural.Properties;

namespace Revit2016_Adapter.Structural.Elements
{
    public class PanelIO
    {
        public static bool GetSlabs(out List<BHoME.Panel> panels, Document document, int rounding = 9)
        {
            ICollection<Floor> floors = new FilteredElementCollector(document).OfClass(typeof(Floor)).Cast<Floor>().ToList();
            panels = RevitSlabsToBHoMPanels(floors, rounding);
            return true;
        }


        public static List<BHoME.Panel> RevitSlabsToBHoMPanels(ICollection<Floor> floors, int rounding = 9)
        {
            List<BHoME.Panel> panels = new List<BHoME.Panel>();

            BHoMB.ObjectManager<string, BHoME.Panel> panelManager = new BHoMB.ObjectManager<string, BHoME.Panel>("Revit Number", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.PanelProperty> thicknessManager = new BHoMB.ObjectManager<BHoMP.PanelProperty>();
            foreach (Floor floor in floors)
            {
                BHoMG.Group<BHoMG.Curve> curves = new BHoMG.Group<BHoMG.Curve>();
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
                    thicknessManager.Add(floor.FloorType.Name, SectionIO.GetThicknessProperty(floor, floor.Document));
                }
                BHoMP.PanelProperty thickness = thicknessManager[floor.FloorType.Name];
                List<BHoMG.Curve> crvs = BHoMG.Curve.Join(curves);
                crvs.Sort(delegate (BHoMG.Curve c1, BHoMG.Curve c2)
                {
                    return c2.Length.CompareTo(c1.Length);
                });

                BHoME.Panel panel = new BHoME.Panel(new BHoMG.Group<BHoMG.Curve>(crvs));
                panelManager.Add(floor.Id.IntegerValue.ToString(), panel);
                panel.PanelProperty = thickness;
                panels.Add(panel);
            }

            return panels;
        }

        private static bool IsInside(BHoMG.Curve c, List<BHoMG.Curve> crvs)
        {
            List<BHoMG.Point> pnts = c.ControlPoints.ToList();
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

        public static bool GetWalls(out List<BHoME.Panel> panels, Document document, int rounding)
        {
            ICollection<Wall> walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            panels = RevitWallsToBHoMPanels(walls, rounding);
            return true;
        }

        public static List<BHoME.Panel> RevitWallsToBHoMPanels(ICollection<Wall> walls, int rounding = 9)
        {
            List<BHoME.Panel> panels = new List<BHoME.Panel>();

            BHoMB.ObjectManager<string, BHoME.Panel> panelManager = new BHoMB.ObjectManager<string, BHoME.Panel>("Revit Number", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.PanelProperty> thicknessManager = new BHoMB.ObjectManager<BHoMP.PanelProperty>();
            foreach (Wall wall in walls)
            {
                if (wall.Location is LocationCurve)
                {
                    Document document = wall.Document;
                    Curve c = (wall.Location as LocationCurve).Curve;

                    ElementId baseConst = wall.LookupParameter("Base Constraint").AsElementId();
                    ElementId topConst = wall.LookupParameter("Top Constraint").AsElementId();

                    double baseOffset = wall.LookupParameter("Base Offset").AsDouble();
                    double topOffset = wall.LookupParameter("Top Offset").AsDouble();

                    BHoMG.Group<BHoMG.Curve> curves = new BHoMG.Group<BHoM.Geometry.Curve>();
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

                        curves.Add(new BHoMG.Line(GeometryUtils.Convert(basePoint1, rounding), GeometryUtils.Convert(basePoint2, rounding)));
                        curves.Add(new BHoMG.Line(GeometryUtils.Convert(basePoint2, rounding), GeometryUtils.Convert(topPoint1, rounding)));
                        curves.Add(new BHoMG.Line(GeometryUtils.Convert(topPoint1, rounding), GeometryUtils.Convert(topPoint2, rounding)));
                        curves.Add(new BHoMG.Line(GeometryUtils.Convert(topPoint2, rounding), GeometryUtils.Convert(basePoint1, rounding)));
                    }

                    if (thicknessManager[wall.WallType.Name] == null)
                    {
                        thicknessManager.Add(wall.WallType.Name, SectionIO.GetThicknessProperty(wall, document));
                    }
                    BHoMP.PanelProperty thickness = thicknessManager[wall.WallType.Name];
                    BHoME.Panel panel = new BHoME.Panel(BHoMG.Curve.Join(curves));
                    panelManager.Add(wall.Id.IntegerValue.ToString(), panel);
                    panel.PanelProperty = thickness;
                    panels.Add(panel);
                }
            }

            return panels;
        }
    }
}