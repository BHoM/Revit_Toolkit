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
        public static bool GetSlabs(out List<BHoME.Panel> panels, Document document,List<string> ids = null, int rounding = 9)
        {
            ICollection<Floor> floors = null;
            if (ids == null)
                floors = new FilteredElementCollector(document).OfClass(typeof(Floor)).Cast<Floor>().ToList();
            else
            {
                floors = new List<Floor>();
                for (int i = 0; i < ids.Count; i++)
                {
                    Floor instance = document.GetElement(new ElementId(int.Parse(ids[i]))) as Floor;
                    if (instance != null)
                    {
                        floors.Add(instance);
                    }
                }
            }

            panels = RevitSlabsToBHoMPanels(floors, rounding);
            return true;
        }


        public static List<BHoME.Panel> RevitSlabsToBHoMPanels(ICollection<Floor> floors, int rounding = 9)
        {
            List<BHoME.Panel> panels = new List<BHoME.Panel>();
            BHoM.Materials.Material material = BHoM.Materials.Material.Default(BHoM.Materials.MaterialType.Concrete);

            BHoMB.ObjectManager<string, BHoME.Panel> panelManager = new BHoMB.ObjectManager<string, BHoME.Panel>(Base.RevitUtils.REVIT_ID_KEY, BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.PanelProperty> thicknessManager = new BHoMB.ObjectManager<BHoMP.PanelProperty>();
            foreach (Floor floor in floors)
            {
                try
                {
                    BHoMG.Group<BHoMG.Curve> curves = new BHoMG.Group<BHoMG.Curve>();
                    GeometryElement geometry = floor.get_Geometry(new Options());
                    foreach (GeometryObject obj in geometry)
                    {
                        if (obj is Solid)
                        {
                            foreach (Face face in (obj as Solid).Faces)
                                if (face is PlanarFace && (face as PlanarFace).FaceNormal.AngleTo(XYZ.BasisZ) < Math.PI / 6)
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
                    BHoME.Panel panel = new BHoME.Panel(curves);
                    panelManager.Add(floor.Id.IntegerValue.ToString(), panel);
                    panel.PanelProperty = thickness;
                    if (panel.PanelProperty != null) panel.PanelProperty.Material = material;
                    panels.Add(panel);
                }
                catch
                {

                }
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

        public static bool GetWalls(out List<BHoME.Panel> panels, Document document, List<string> ids = null, int rounding = 9)
        {
            ICollection<Wall> walls = null;
            if (ids == null)
                walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            else
            {
                walls = new List<Wall>();
                for (int i = 0; i < ids.Count; i++)
                {
                    Wall instance = document.GetElement(new ElementId(int.Parse(ids[i]))) as Wall;
                    if (instance != null)
                    {
                        walls.Add(instance);
                    }
                }
            }
            
            panels = RevitWallsToBHoMPanels(walls, rounding);
            return true;
        }

        public static bool GetFoundations(out List<BHoME.Panel> panels, Document document, List<string> ids = null, int rounding = 9)
        {
            List<FamilyInstance> foundations = null;
            if (ids == null)
            {
                foundations = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFoundation).Cast<FamilyInstance>().ToList();
            }
            else
            {
                foundations = new List<FamilyInstance>();
                for (int i = 0; i < ids.Count; i++)
                {
                    FamilyInstance instance = document.GetElement(new ElementId(int.Parse(ids[i]))) as FamilyInstance;
                    if (instance != null && instance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Footing)
                    {
                        foundations.Add(instance);
                    }
                }
            }
            panels = RevitFoundationsToBHoMPanels(foundations, rounding);
            return true;
        }

        public static List<BHoME.Panel> RevitWallsToBHoMPanels(ICollection<Wall> walls, int rounding = 9)
        {
            List<BHoME.Panel> panels = new List<BHoME.Panel>();
            BHoM.Materials.Material material = BHoM.Materials.Material.Default(BHoM.Materials.MaterialType.Concrete);

            BHoMB.ObjectManager<string, BHoME.Panel> panelManager = new BHoMB.ObjectManager<string, BHoME.Panel>(Base.RevitUtils.REVIT_ID_KEY, BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.PanelProperty> thicknessManager = new BHoMB.ObjectManager<BHoMP.PanelProperty>();

            if (walls.Count > 0)
            {
                IEnumerable<FamilyInstance> doors = new FilteredElementCollector(walls.First().Document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_Doors).Cast<FamilyInstance>();

                Dictionary<ElementId, List<FamilyInstance>> doorMap = new Dictionary<ElementId, List<FamilyInstance>>();

                foreach (FamilyInstance door in doors)
                {
                    if (door.Host != null)
                    {
                        List<FamilyInstance> list = null;
                        if (doorMap.TryGetValue(door.Host.Id, out list))
                        {
                            list.Add(door);
                        }
                        else
                        {
                            doorMap.Add(door.Host.Id, new List<FamilyInstance>() { door });
                        }
                    }
                }

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

                        BHoMG.Curve perimeter = null;
                        BHoMG.Plane wallPlane = null;

                        if (baseConst.IntegerValue > 0 && topConst.IntegerValue > 0)
                        {
                            double baseLevel = (document.GetElement(baseConst) as Level).ProjectElevation + baseOffset;
                            double topLevel = (document.GetElement(topConst) as Level).ProjectElevation + topOffset;

                            XYZ p1 = c.GetEndPoint(0);
                            XYZ p2 = c.GetEndPoint(1);
                            List<BHoMG.Point> pts = new List<BHoMG.Point>();
                            pts.Add(GeometryUtils.Convert(new XYZ(p1.X, p1.Y, baseLevel), rounding));
                            pts.Add(GeometryUtils.Convert(new XYZ(p2.X, p2.Y, baseLevel), rounding));
                            pts.Add(GeometryUtils.Convert(new XYZ(p2.X, p2.Y, topLevel), rounding));
                            pts.Add(GeometryUtils.Convert(new XYZ(p1.X, p1.Y, topLevel), rounding));
                            pts.Add(pts[0]);
                            perimeter = new BHoMG.Polyline(pts);
                            wallPlane = new BHoMG.Plane(pts[0], pts[1], pts[2]);
                        }

                        if (thicknessManager[wall.WallType.Name] == null)
                        {
                            thicknessManager.Add(wall.WallType.Name, SectionIO.GetThicknessProperty(wall, document));
                        }

                        BHoMP.PanelProperty thickness = thicknessManager[wall.WallType.Name];
                        BHoME.Panel panel = new BHoME.Panel(new List<BHoMG.Curve>() { perimeter });

                        List<FamilyInstance> openings = null;

                        if (doorMap.TryGetValue(wall.Id, out openings))
                        {
                            BHoMG.Vector direction = BHoMG.Vector.CrossProduct(wallPlane.Normal, BHoMG.Vector.ZAxis());
                            foreach (FamilyInstance opening in openings)
                            {
                                double width = opening.Symbol.LookupParameter("Width").AsDouble() * GeometryUtils.FeetToMetre;
                                double height = opening.Symbol.LookupParameter("Height").AsDouble() * GeometryUtils.FeetToMetre;
                                if (opening.Location is LocationPoint)
                                {
                                    BHoMG.Point location = GeometryUtils.Convert((opening.Location as LocationPoint).Point);
                                    List<BHoMG.Point> pts = new List<BHoMG.Point>();
                                    pts.Add(location - direction * width / 2);
                                    pts.Add(location + direction * width / 2);
                                    pts.Add(pts[1] + new BHoMG.Vector(0, 0, height));
                                    pts.Add(pts[0] + new BHoMG.Vector(0, 0, height));
                                    pts.Add(pts[0]);
                                    perimeter = new BHoMG.Polyline(pts);
                                    panel.Internal_Contours.Add(perimeter);
                                }
                            }
                        }

                        panelManager.Add(wall.Id.IntegerValue.ToString(), panel);
                        panel.PanelProperty = thickness;
                        if (panel.PanelProperty != null) panel.PanelProperty.Material = material;
                        panels.Add(panel);
                    }
                }
            }
            return panels;
        }

        public static List<BHoME.Panel> RevitFoundationsToBHoMPanels(ICollection<FamilyInstance> foundations, int rounding = 9)
        {
            List<BHoME.Panel> panels = new List<BHoME.Panel>();
            BHoM.Materials.Material material = BHoM.Materials.Material.Default(BHoM.Materials.MaterialType.Concrete);

            BHoMB.ObjectManager<string, BHoME.Panel> panelManager = new BHoMB.ObjectManager<string, BHoME.Panel>(Base.RevitUtils.REVIT_ID_KEY, BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.PanelProperty> thicknessManager = new BHoMB.ObjectManager<BHoMP.PanelProperty>();
            foreach (FamilyInstance foundation in foundations)
            {
                try
                {
                    BHoMG.Group<BHoMG.Curve> curves = new BHoMG.Group<BHoMG.Curve>();
                    GeometryElement geometry = foundation.get_Geometry(new Options());
                    Transform transform = null;

                    foreach (GeometryObject obj in geometry)
                    {
                        if (obj is Solid)
                        {
                            foreach (Face face in (obj as Solid).Faces)
                            {
                                if (face is PlanarFace && (face as PlanarFace).FaceNormal.AngleTo(XYZ.BasisZ) < Math.PI / 6)
                                {
                                    foreach (EdgeArray curveArray in face.EdgeLoops)
                                    {
                                        foreach (Edge c in curveArray)
                                        {
                                            curves.Add(GeometryUtils.Convert(c.AsCurve(), rounding));
                                        }
                                    }
                                }
                            }
                        }
                        else if (obj is GeometryInstance)
                        {
                            transform = (obj as GeometryInstance).Transform;
                        }
                    }

                    Parameter param = foundation.LookupParameter("Host");
                    if (param == null)
                    {
                        //Not accurate
                        double elevation = foundation.LookupParameter("Elevation at Top").AsDouble() * GeometryUtils.FeetToMetre;
                        BHoMG.Plane plane = new BHoM.Geometry.Plane(new BHoM.Geometry.Point(0, 0, elevation), BHoMG.Vector.ZAxis());
                        curves.Project(plane);

                        if (thicknessManager[foundation.Symbol.Name] == null)
                        {
                            thicknessManager.Add(foundation.Symbol.Name, SectionIO.GetFoundationProperty(foundation, foundation.Document));
                        }

                        BHoMP.PanelProperty thickness = thicknessManager[foundation.Symbol.Name];

                        BHoME.Panel panel = new BHoME.Panel(curves);
                        panelManager.Add(foundation.Id.IntegerValue.ToString(), panel);
                        panel.PanelProperty = thickness;
                        if (panel.PanelProperty != null) panel.PanelProperty.Material = material;
                        panels.Add(panel);
                    }
                }
                catch (Exception ex)
                {

                }
            }
                

            return panels;
        }
    }
}