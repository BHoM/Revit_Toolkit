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

using Revit2017_Adapter.Geometry;
using Revit2017_Adapter.Structural.Properties;

namespace Revit2017_Adapter.Structural.Elements
{
    public class BarIO
    {
        public static bool GetBeams(out List<BHoME.Bar> bars, Document document, List<string> ids = null, int rounding = 9)
        {
            ICollection<FamilyInstance> framing = null;
            if (ids == null)
                framing = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilyInstance>().ToList();
            else
            {
                framing = new List<FamilyInstance>();
                for (int i = 0; i < ids.Count; i++)
                {
                    FamilyInstance instance = document.GetElement(new ElementId(int.Parse(ids[i]))) as FamilyInstance;
                    if (instance != null && instance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam || instance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Brace)
                    {
                        framing.Add(instance);
                    }
                }
            }
            bars = RevitBeamsToBHomBars(framing, rounding);
            return true;
        }

        public static List<BHoME.Bar> RevitBeamsToBHomBars(ICollection<FamilyInstance> framing, int rounding = 9)
        {
            List<BHoME.Bar> bars = new List<BHoME.Bar>();
            BHoMG.Curve barCentreline = null;

            BHoMB.ObjectManager<string, BHoME.Node> nodes = new BHoMB.ObjectManager<string, BHoME.Node>("RevitLocation", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<string, BHoME.Bar> barManager = new BHoMB.ObjectManager<string, BHoME.Bar>(Base.RevitUtils.REVIT_ID_KEY, BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.SectionProperty> sections = new BHoMB.ObjectManager<BHoMP.SectionProperty>();
            Dictionary<BHoM.Materials.MaterialType, BHoM.Materials.Material> materials = new Dictionary<BHoM.Materials.MaterialType, BHoM.Materials.Material>();
            BHoMG.Point p1;
            BHoMG.Point p2;

            foreach (FamilyInstance beam in framing)
            {
                if (beam.Location is LocationCurve)
                {
                    barCentreline = GeometryUtils.Convert((beam.Location as LocationCurve).Curve, rounding);
                    p1 = barCentreline.StartPoint;
                    p2 = barCentreline.EndPoint;
                    BHoME.Node n1 = nodes[GeometryUtils.PointLocation(p1, 3)];
                    BHoME.Node n2 = nodes[GeometryUtils.PointLocation(p2, 3)];

                    if (n1 == null) n1 = nodes.Add(GeometryUtils.PointLocation(barCentreline.StartPoint, 3), new BHoME.Node(p1.X, p1.Y, p1.Z));

                    if (n2 == null) n2 = nodes.Add(GeometryUtils.PointLocation(barCentreline.EndPoint, 3), new BHoME.Node(p2.X, p2.Y, p2.Z));

                    BHoME.Bar bar = new BHoME.Bar(n1, n2);
                    if (sections[beam.Symbol.Name] == null)
                    {
                        sections.Add(beam.Symbol.Name, SectionIO.GetSectionProperty(beam.Symbol, false));
                    }
                   
                    bar.SectionProperty = sections[beam.Symbol.Name];
                    double rotation = -beam.LookupParameter("Cross-Section Rotation").AsDouble();

                    BHoM.Materials.Material material = null;
                    BHoM.Materials.MaterialType matKey = Base.RevitUtils.GetMaterialType(beam.StructuralMaterialType);

                    if (!materials.TryGetValue(matKey, out material))
                    {
                        material = BHoM.Materials.Material.Default(matKey);
                        materials.Add(matKey, material);
                    }

                    if (bar.SectionProperty != null) bar.SectionProperty.Material = material;
                    bar.SectionProperty = sections[beam.Symbol.Name];
                    bar.OrientationAngle = rotation;
                    bars.Add(bar);
                    barManager.Add(beam.Id.ToString(), bar);
                }
            }

            return bars;
        }

        public static bool GetColumns(out List<BHoME.Bar> bars, Document document, List<string> ids = null, int rounding = 9)
        {
           ICollection<FamilyInstance> columns = null;
            if (ids == null)
                columns = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilyInstance>().ToList();
            else
            {
                columns = new List<FamilyInstance>();
                for (int i = 0; i < ids.Count; i++)
                {
                    FamilyInstance instance = document.GetElement(new ElementId(int.Parse(ids[i]))) as FamilyInstance;
                    if (instance != null && instance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column)
                    {
                        columns.Add(instance);
                    }
                }
            }
            bars = RevitColumnsToBHomBars(columns, rounding);
            return true;
        }

        public static List<BHoME.Panel> BHomColumnsToBHoMPanels(List<BHoME.Bar> columns, string key = "", BHoMB.FilterOption option = BHoMB.FilterOption.Guid)
        {
            List<BHoME.Panel> panels = new List<BHoM.Structural.Elements.Panel>();
            BHoMB.ObjectManager<string, BHoME.Panel> panelManager = new BHoMB.ObjectManager<string, BHoME.Panel>(key, option);
            BHoMB.ObjectManager<BHoMP.PanelProperty> thicknessManager = new BHoMB.ObjectManager<BHoMP.PanelProperty>();

            foreach (BHoME.Bar col in columns)
            {
                if (col.SectionProperty != null && col.Line.Direction.IsParallel(BHoMG.Vector.ZAxis(), Math.PI/24))
                {
                    double length = 0;
                    double thickness = 0;
                    double angleIncrement = 0;
                    if (col.SectionProperty.TotalDepth > col.SectionProperty.TotalWidth)
                    {
                        length = col.SectionProperty.TotalDepth;
                        thickness = col.SectionProperty.TotalWidth;
                    }
                    else
                    {
                        length = col.SectionProperty.TotalWidth;
                        thickness = col.SectionProperty.TotalDepth;
                        angleIncrement = Math.PI / 2;
                    }
                    double rad = col.OrientationAngle * Math.PI / 180 + angleIncrement;

                    BHoMG.Vector normal = new BHoMG.Vector(Math.Cos(rad), Math.Sin(rad), 0);
                    BHoMG.Vector up = new BHoMG.Vector(0, 0, 1);
                    BHoMG.Vector direction = BHoMG.Vector.CrossProduct(up, normal);

                    direction.Unitize();
                    direction = direction * length / 2;
                    
                    BHoMG.Line line1 = col.Line.DuplicateCurve() as BHoMG.Line;
                    BHoMG.Line line2 = col.Line.DuplicateCurve() as BHoMG.Line;

                    line1.Translate(direction);
                    line2.Translate(BHoMG.Vector.Zero - direction);

                    BHoMG.Line line3 = new BHoMG.Line(line1.StartPoint, line2.StartPoint);
                    BHoMG.Line line4 = new BHoMG.Line(line1.EndPoint, line2.EndPoint);

                    BHoMG.Group<BHoMG.Curve> perimeter = new BHoMG.Group<BHoMG.Curve>(new List<BHoMG.Curve>() { line1, line2, line3, line4 });

                    string propertyName = "Wall " + Math.Round(thickness * 1000, 2) + " mm";

                    if (thicknessManager[propertyName] == null)
                    {
                        thicknessManager.Add(propertyName, new BHoMP.ConstantThickness(propertyName, thickness, BHoM.Structural.Properties.PanelType.Wall));
                    }

                    BHoME.Panel panel = new BHoME.Panel(perimeter);

                    switch (option)
                    {
                        case BHoMB.FilterOption.Guid:
                            panelManager.Add(col.BHoM_Guid.ToString(), panel);
                            break;
                        case BHoM.Base.FilterOption.Name:
                            panelManager.Add(col.Name, panel);
                            break;
                        case BHoM.Base.FilterOption.Property:
                            panelManager.Add(key, panel);
                            break;
                        case BHoM.Base.FilterOption.UserData:
                            panelManager.Add(col[key].ToString(), panel);
                            break;
                    }



                    BHoMP.PanelProperty property = thicknessManager[propertyName];
                    panel.PanelProperty = property;
                    if (panel.PanelProperty != null) panel.PanelProperty.Material = col.Material;
                    panels.Add(panel);

                    BHoM.Global.Project.ActiveProject.RemoveObject(col.BHoM_Guid);
                }
            }
            return panels;
        }

        public static List<BHoME.Panel> RevitColumnsToBHomPanels(ICollection<FamilyInstance> columns, int rounding = 9)
        {
            List<BHoME.Bar> bhColumns = RevitColumnsToBHomBars(columns, rounding);

            return BHomColumnsToBHoMPanels(bhColumns);
        }


        public static List<BHoME.Bar> RevitColumnsToBHomBars(ICollection<FamilyInstance> columns, int rounding = 9)
        {
            List<BHoME.Bar> bars = new List<BHoME.Bar>();
            BHoMG.Curve barCentreline = null;

            BHoMB.ObjectManager<string, BHoME.Node> nodes = new BHoMB.ObjectManager<string, BHoME.Node>("RevitLocation", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<string, BHoME.Bar> barManager = new BHoMB.ObjectManager<string, BHoME.Bar>(Base.RevitUtils.REVIT_ID_KEY, BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.SectionProperty> sections = new BHoMB.ObjectManager<BHoMP.SectionProperty>();
            Dictionary<BHoM.Materials.MaterialType, BHoM.Materials.Material> materials = new Dictionary<BHoM.Materials.MaterialType, BHoM.Materials.Material>();
            BHoMG.Point p1 = null;
            BHoMG.Point p2 = null;
            double rotation = 0;
            foreach (FamilyInstance column in columns)
            {
                try
                {
                    Document document = column.Document;

                    if (column.Location is LocationCurve)
                    {
                        barCentreline = GeometryUtils.Convert((column.Location as LocationCurve).Curve, rounding);
                        p1 = barCentreline.StartPoint;
                        p2 = barCentreline.EndPoint;
                        rotation = -column.LookupParameter("Cross-Section Rotation").AsDouble();
                    }
                    else if (column.Location is LocationPoint)
                    {
                        ElementId baseConst = column.LookupParameter("Base Level").AsElementId();
                        ElementId topConst = column.LookupParameter("Top Level").AsElementId();

                        double baseOffset = column.LookupParameter("Base Offset").AsDouble();
                        double topOffset = column.LookupParameter("Top Offset").AsDouble();

                        if (baseConst.IntegerValue > 0 && topConst.IntegerValue > 0)
                        {
                            double baseLevel = (document.GetElement(baseConst) as Level).ProjectElevation + baseOffset;
                            double topLevel = (document.GetElement(topConst) as Level).ProjectElevation + topOffset;

                            XYZ rP1 = (column.Location as LocationPoint).Point;
                            XYZ baseElevation = new XYZ(rP1.X, rP1.Y, baseLevel);
                            XYZ topElevation = new XYZ(rP1.X, rP1.Y, topLevel);
                            p1 = GeometryUtils.Convert(baseElevation, rounding);
                            p2 = GeometryUtils.Convert(topElevation, rounding);
                        }
                        int multiplier = column.FacingOrientation.DotProduct(new XYZ(1, 0, 0)) < 0 ? 1 : -1;
                        rotation = column.FacingOrientation.AngleTo(new XYZ(0, 1, 0)) * multiplier;
                    }

                    BHoME.Node n1 = nodes[GeometryUtils.PointLocation(p1, 3)];
                    BHoME.Node n2 = nodes[GeometryUtils.PointLocation(p2, 3)];

                    if (n1 == null) n1 = nodes.Add(GeometryUtils.PointLocation(p1, 3), new BHoME.Node(p1.X, p1.Y, p1.Z));

                    if (n2 == null) n2 = nodes.Add(GeometryUtils.PointLocation(p2, 3), new BHoME.Node(p2.X, p2.Y, p2.Z));

                    BHoME.Bar bar = new BHoME.Bar(n1, n2);

                    if (sections[column.Symbol.Name] == null)
                    {
                        sections.Add(column.Symbol.Name, SectionIO.GetSectionProperty(column.Symbol, true));
                    }                  

                    bar.SectionProperty = sections[column.Symbol.Name];
                    bar.OrientationAngle = rotation;
                    barManager.Add(column.Id.ToString(), bar);
                    bars.Add(bar);
                }
                catch (Exception ex)
                {

                }
            }
            return bars;
        }       

        public static bool GetPiles(out List<BHoME.Bar> bars, Document document, List<string> ids = null, int rounding = 9)
        {
            ICollection<FamilyInstance> foundations = null;
            if (ids == null)
                foundations = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFoundation).Cast<FamilyInstance>().ToList();
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
            bars = RevitPilesToBHoMBars(foundations, rounding);
            return true;
        }

        public static List<BHoME.Bar> RevitPilesToBHoMBars(ICollection<FamilyInstance> foundations, int rounding = 9)
        {
            List<BHoME.Bar> bars = new List<BHoME.Bar>();
            BHoMB.ObjectManager<string, BHoME.Bar> barManager = new BHoMB.ObjectManager<string, BHoME.Bar>(Base.RevitUtils.REVIT_ID_KEY, BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.SectionProperty> sections = new BHoMB.ObjectManager<BHoMP.SectionProperty>();
            BHoMB.ObjectManager<string, BHoME.Node> nodes = new BHoMB.ObjectManager<string, BHoME.Node>("RevitLocation", BHoMB.FilterOption.UserData);
            Dictionary<BHoM.Materials.MaterialType, BHoM.Materials.Material> materials = new Dictionary<BHoM.Materials.MaterialType, BHoM.Materials.Material>();
            BHoMG.Point p1 = null;
            BHoMG.Point p2 = null;

            foreach (FamilyInstance foundation in foundations)
            { 
                Parameter param = foundation.LookupParameter("Host");
                if (param != null)
                {
                    if (foundation.Location is LocationPoint)
                    {
                        try
                        {
                            double depth = foundation.LookupParameter("Pile Depth").AsDouble();
                            double diameter = foundation.Symbol.LookupParameter("Pile Diameter").AsDouble() * GeometryUtils.FeetToMetre;
                            double offset = foundation.LookupParameter("Offset").AsDouble();

                            XYZ rP1 = (foundation.Location as LocationPoint).Point;

                            double topElevation = 0;// rP1.Z - offset;
                            ElementId levelId = null;

                            if (foundation.HostFace != null)
                            {
                                ElementId reference = foundation.HostFace.ElementId;
                                Element e = foundation.Document.GetElement(reference);
                                if (e != null)
                                {
                                    topElevation = e.LookupParameter("Elevation at Top").AsDouble();
                                }
                            }
                            else if (foundation.SuperComponent != null)
                            {
                                Element e = foundation.SuperComponent as FamilyInstance;
                                if (e != null)
                                {
                                    topElevation = e.LookupParameter("Elevation at Top").AsDouble();
                                }
                            }
                            else
                            { 
                                topElevation = (foundation.Location as LocationPoint).Point.Z - offset;
                            }


                            XYZ basePoint = new XYZ(rP1.X, rP1.Y, topElevation - depth);
                            XYZ topPoint = new XYZ(rP1.X, rP1.Y, topElevation);

                            p1 = GeometryUtils.Convert(basePoint, rounding);
                            p2 = GeometryUtils.Convert(topPoint, rounding);

                          

                            BHoME.Node n1 = nodes[GeometryUtils.PointLocation(p1, 3)];
                            BHoME.Node n2 = nodes[GeometryUtils.PointLocation(p2, 3)];

                            if (n1 == null) n1 = nodes.Add(GeometryUtils.PointLocation(p1, 3), new BHoME.Node(p1.X, p1.Y, p1.Z));

                            if (n2 == null) n2 = nodes.Add(GeometryUtils.PointLocation(p2, 3), new BHoME.Node(p2.X, p2.Y, p2.Z));

                            BHoME.Bar bar = new BHoME.Bar(n1, n2);
                            if (sections[foundation.Symbol.Name] == null)
                            {
                                BHoM.Materials.Material material = null;
                                BHoM.Materials.MaterialType matKey = Base.RevitUtils.GetMaterialType(foundation.StructuralMaterialType);
                                if (!materials.TryGetValue(matKey, out material))
                                {
                                    material = BHoM.Materials.Material.Default(matKey);
                                    materials.Add(matKey, material);
                                }
                                sections.Add(foundation.Symbol.Name, BHoMP.SectionProperty.CreateCircularSection(matKey, diameter));
                                
                                if (bar.SectionProperty != null) bar.SectionProperty.Material = material;
                            }
                            bar.SectionProperty = sections[foundation.Symbol.Name];
                            bar.StructuralUsage = BHoM.Structural.Elements.BarStructuralUsage.Pile;
                            barManager.Add(foundation.Id.ToString(), bar);
                            bars.Add(bar);
                            Parameter Mark = null;
                            if ((Mark = foundation.LookupParameter("Mark")) != null)
                            {
                                bar.CustomData.Add("Mark", Mark.AsString());
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            return bars;
        }
    }
}
