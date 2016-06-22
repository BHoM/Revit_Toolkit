using Autodesk.DesignScript.Geometry;
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
            List<BHoM.Geometry.Point> contourPts = new List<BHoM.Geometry.Point>();

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
            }

            BHoM.Geometry.Polyline contour = new BHoM.Geometry.Polyline(contourPts);
            BHoM.Geometry.Group<BHoM.Geometry.Curve> group = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
            group.Add(contour);

            BHoM.Structural.Panel panel = new BHoM.Structural.Panel(group);
            panel.ThicknessProperty = new BHoM.Structural.ConstantThickness(wall.Name);
            panel.ThicknessProperty.Thickness = scale * wall.WallType.LookupParameter("Width").AsDouble();
            panel.CustomData["RevitId"] = wall.Id;

            return panel;
        }
    }
}
