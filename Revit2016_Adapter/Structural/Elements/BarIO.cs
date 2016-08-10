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
    public class BarIO
    {
        public static bool GetBeams(out List<BHoME.Bar> bars, Document document, int rounding = 9)
        {
            ICollection<FamilyInstance> framing = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilyInstance>().ToList();
            bars = RevitBeamsToBHomBars(framing, rounding);
            return true;
        }

        public static List<BHoME.Bar> RevitBeamsToBHomBars(ICollection<FamilyInstance> framing, int rounding = 9)
        {
            List<BHoME.Bar> bars = new List<BHoME.Bar>();
            BHoMG.Curve barCentreline = null;

            BHoMB.ObjectManager<string, BHoME.Node> nodes = new BHoMB.ObjectManager<string, BHoME.Node>("RevitLocation", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<string, BHoME.Bar> barManager = new BHoMB.ObjectManager<string, BHoME.Bar>("Revit Number", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.SectionProperty> sections = new BHoMB.ObjectManager<BHoMP.SectionProperty>();
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
                    double rotation = beam.LookupParameter("Cross-Section Rotation").AsDouble() * 180 / Math.PI;

                    bar.SectionProperty = sections[beam.Symbol.Name];
                    bar.OrientationAngle = rotation;
                    bars.Add(bar);
                    barManager.Add(beam.Id.ToString(), bar);
                }
            }

            return bars;
        }


        public static bool GetColumns(out List<BHoME.Bar> bars, Document document, int rounding = 9)
        {
            ICollection<FamilyInstance> columns = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilyInstance>().ToList();
            bars = RevitColumnsToBHomBars(columns, rounding);
            return true;
        }

        public static List<BHoME.Bar> RevitColumnsToBHomBars(ICollection<FamilyInstance> columns, int rounding = 9)
        {
            List<BHoME.Bar> bars = new List<BHoME.Bar>();
            BHoMG.Curve barCentreline = null;

            BHoMB.ObjectManager<string, BHoME.Node> nodes = new BHoMB.ObjectManager<string, BHoME.Node>("RevitLocation", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<string, BHoME.Bar> barManager = new BHoMB.ObjectManager<string, BHoME.Bar>("Revit Number", BHoMB.FilterOption.UserData);
            BHoMB.ObjectManager<BHoMP.SectionProperty> sections = new BHoMB.ObjectManager<BHoMP.SectionProperty>();
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

    }
}
