using Autodesk.Revit.DB;
using BHoM.Base;
using BHoM.Global;
using BHoM.Structural;
using BHoM.Structural.Elements;
using BHoM.Structural.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH = BHoM.Geometry;

namespace RevitToolkit2015
{

    public class BarIO
    {
        public static bool GetBeams(out List<Bar> bars, Document document, int rounding)
        {
            bars = new List<Bar>();
            ICollection<FamilyInstance> framing = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralFraming).Cast<FamilyInstance>().ToList();
            BH.Curve barCentreline = null;

            ObjectManager<string, Node> nodes = new ObjectManager<string, Node>("RevitLocation", FilterOption.UserData);
            ObjectManager<string, Bar> barManager = new ObjectManager<string, Bar>(RevitUtils., FilterOption.UserData);
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
                    double rotation = beam.LookupParameter("Cross-Section Rotation").AsDouble();

                    bar.SectionProperty = sections[beam.Symbol.Name];
                    bar.OrientationAngle = rotation;
                    bars.Add(bar);
                    barManager.Add(beam.Id.ToString(), bar);
                }
            }
            return true;
        }

        public static bool GetColumns(out List<Bar> bars, Document document, int rounding)
        {
            bars = new List<Bar>();
            ICollection<FamilyInstance> framing = new FilteredElementCollector(document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_StructuralColumns).Cast<FamilyInstance>().ToList();
            BH.Curve barCentreline = null;

            ObjectManager<string, Node> nodes = new ObjectManager<string, Node>("RevitLocation", FilterOption.UserData);
            ObjectManager<string, Bar> barManager = new ObjectManager<string, Bar>("Revit Number", FilterOption.UserData);
            ObjectManager<SectionProperty> sections = new ObjectManager<SectionProperty>();
            BH.Point p1 = null;
            BH.Point p2 = null;
            double rotation = 0;
            foreach (FamilyInstance beam in framing)
            {
                try
                {
                    if (beam.Location is LocationCurve)
                    {
                        barCentreline = GeometryUtils.Convert((beam.Location as LocationCurve).Curve, rounding);
                        p1 = barCentreline.StartPoint;
                        p2 = barCentreline.EndPoint;
                        rotation = beam.LookupParameter("Cross-Section Rotation").AsDouble();
                    }
                    else if (beam.Location is LocationPoint)
                    {
                        ElementId baseConst = beam.LookupParameter("Base Level").AsElementId();
                        ElementId topConst = beam.LookupParameter("Top Level").AsElementId();

                        double baseOffset = beam.LookupParameter("Base Offset").AsDouble();
                        double topOffset = beam.LookupParameter("Top Offset").AsDouble();

                        if (baseConst.IntegerValue > 0 && topConst.IntegerValue > 0)
                        {
                            double baseLevel = (document.GetElement(baseConst) as Level).ProjectElevation + baseOffset;
                            double topLevel = (document.GetElement(topConst) as Level).ProjectElevation + topOffset;

                            XYZ rP1 = (beam.Location as LocationPoint).Point;
                            XYZ baseElevation = new XYZ(rP1.X, rP1.Y, baseLevel);
                            XYZ topElevation = new XYZ(rP1.X, rP1.Y, topLevel);
                            p1 = GeometryUtils.Convert(baseElevation, rounding);
                            p2 = GeometryUtils.Convert(topElevation, rounding);
                        }
                        int mulipler = beam.FacingOrientation.DotProduct(new XYZ(0, 1, 0)) > 0 ? 1 : -1;
                        rotation =  beam.FacingOrientation.AngleTo(new XYZ(0, 1, 0)) * mulipler;
                    }

                    Node n1 = nodes[GeometryUtils.PointLocation(p1, 3)];
                    Node n2 = nodes[GeometryUtils.PointLocation(p2, 3)];

                    if (n1 == null) n1 = nodes.Add(GeometryUtils.PointLocation(p1, 3), new Node(p1.X, p1.Y, p1.Z));

                    if (n2 == null) n2 = nodes.Add(GeometryUtils.PointLocation(p2, 3), new Node(p2.X, p2.Y, p2.Z));

                    Bar bar = new Bar(n1, n2);

                    if (sections[beam.Symbol.Name] == null)
                    {
                        sections.Add(beam.Symbol.Name, SectionIO.GetSectionProperty(beam.Symbol, true));
                    }


                    bar.SectionProperty = sections[beam.Symbol.Name];
                    bar.OrientationAngle = rotation;
                    barManager.Add(beam.Id.ToString(), bar);
                    bars.Add(bar);
                }
                catch (Exception ex)
                {

                }
            }
            return true;
        }
    }
}
