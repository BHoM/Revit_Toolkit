using Autodesk.Revit.DB;
using BHoM.Global;
using BHoM.Structural;
using BHoM.Structural.SectionProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH = BHoM.Geometry;

namespace Revit2016IO
{

    public class BarIO
    {
        public static bool GetBeams(out List<Bar> bars, Document document, int rounding = 9)
        {
            ICollection<FamilyInstance> framing = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilyInstance>().ToList();
            bars = RevitBeamsToBHomBars(framing, rounding);
            return true;
        }

        public static List<Bar> RevitBeamsToBHomBars(ICollection<FamilyInstance> framing, int rounding = 9)
        {
            List<Bar> bars = new List<Bar>();
            BH.Curve barCentreline = null;

            ObjectManager<string, Node> nodes = new ObjectManager<string, Node>("RevitLocation", FilterOption.UserData);
            ObjectManager<string, Bar> barManager = new ObjectManager<string, Bar>("Revit Number", FilterOption.UserData);
            ObjectManager<SectionProperty> sections = new ObjectManager<SectionProperty>();
            BH.Point p1;
            BH.Point p2;

            foreach (FamilyInstance beam in framing)
            {
                if (beam.Location is LocationCurve)
                {
                    barCentreline = GeometryUtils.Convert((beam.Location as LocationCurve).Curve, rounding);
                    p1 = barCentreline.StartPoint;
                    p2 = barCentreline.EndPoint;
                    Node n1 = nodes[GeometryUtils.PointLocation(p1, 3)];
                    Node n2 = nodes[GeometryUtils.PointLocation(p2, 3)];

                    if (n1 == null) n1 = nodes.Add(GeometryUtils.PointLocation(barCentreline.StartPoint, 3), new Node(p1.X, p1.Y, p1.Z));

                    if (n2 == null) n2 = nodes.Add(GeometryUtils.PointLocation(barCentreline.EndPoint, 3), new Node(p2.X, p2.Y, p2.Z));

                    Bar bar = new Bar(n1, n2);
                    if (sections[beam.Symbol.Name] == null)
                    {
                        sections.Add(beam.Symbol.Name, SectionIO.GetSectionProperty(beam.Symbol, false));
                    }
                    bar.SectionProperty = sections[beam.Symbol.Name];
                    double rotation = beam.LookupParameter("Cross-Section Rotation").AsDouble() * 180 / Math.PI;

                    bar.SectionProperty = sections[beam.Symbol.Name];
                    bar.OrientationAngle = rotation;
                    bars.Add(bar);
                    barManager.Add(beam.Id.ToString(), bar);
                }
            }

            return bars;
        }


        public static bool GetColumns(out List<Bar> bars, Document document, int rounding = 9)
        {
            ICollection<FamilyInstance> columns = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilyInstance>().ToList();
            bars = RevitColumnsToBHomBars(columns, rounding);
            return true;
        }

        public static List<Bar> RevitColumnsToBHomBars(ICollection<FamilyInstance> columns, int rounding = 9)
        {
            List<Bar> bars = new List<Bar>();
            BH.Curve barCentreline = null;

            ObjectManager<string, Node> nodes = new ObjectManager<string, Node>("RevitLocation", FilterOption.UserData);
            ObjectManager<string, Bar> barManager = new ObjectManager<string, Bar>("Revit Number", FilterOption.UserData);
            ObjectManager<SectionProperty> sections = new ObjectManager<SectionProperty>();
            BH.Point p1 = null;
            BH.Point p2 = null;
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
                        rotation = column.LookupParameter("Cross-Section Rotation").AsDouble();
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
                        int mulipler = column.FacingOrientation.DotProduct(new XYZ(0, 1, 0)) > 0 ? 1 : -1;
                        rotation = column.FacingOrientation.AngleTo(new XYZ(0, 1, 0)) * mulipler;
                    }

                    Node n1 = nodes[GeometryUtils.PointLocation(p1, 3)];
                    Node n2 = nodes[GeometryUtils.PointLocation(p2, 3)];

                    if (n1 == null) n1 = nodes.Add(GeometryUtils.PointLocation(p1, 3), new Node(p1.X, p1.Y, p1.Z));

                    if (n2 == null) n2 = nodes.Add(GeometryUtils.PointLocation(p2, 3), new Node(p2.X, p2.Y, p2.Z));

                    Bar bar = new Bar(n1, n2);

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

    }
}
