using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revit.GeometryConversion;
using Revit.Elements;
using RevitToolkit.Global;
using RevitToolkit.Materials;
using RevitServices.Transactions;
using Autodesk.DesignScript.Interfaces;
using Autodesk.Revit.DB;

namespace RevitToolkit.Elements
{
    /// <summary>
    /// A Revit column
    /// </summary>
    public static class Column
    {
        /// <summary>
        /// Create BHoM Bar from Revit Column
        /// </summary>
        public static BHoM.Structural.Bar ToBHomBar(Autodesk.Revit.DB.FamilyInstance column)
        {
            if (column.StructuralType != Autodesk.Revit.DB.Structure.StructuralType.Column)
                return null;

            Autodesk.Revit.DB.Document document = column.Document;
            Autodesk.Revit.DB.Curve c = Utils.GetLocationCurve(column);
            Autodesk.Revit.DB.XYZ cStart = new Autodesk.Revit.DB.XYZ();
            Autodesk.Revit.DB.XYZ cEnd = new Autodesk.Revit.DB.XYZ();
            double scale = RevitToolkit.Global.GeometryConverter.FeetToMetre;

            /*if (c != null)
            {
                cStart = c.GetEndPoint(0);
                cEnd = c.GetEndPoint(1);
            }
            else if (column.Location is Autodesk.Revit.DB.LocationPoint)
            {
                Autodesk.Revit.DB.ElementId baseConst = column.LookupParameter("Base Level").AsElementId();
                Autodesk.Revit.DB.ElementId topConst = column.LookupParameter("Top Level").AsElementId();

                double baseOffset = column.LookupParameter("Base Offset").AsDouble();
                double topOffset = column.LookupParameter("Top Offset").AsDouble();

                if (baseConst.IntegerValue > 0 && topConst.IntegerValue > 0)
                {
                    double baseLevel = (document.GetElement(baseConst) as Autodesk.Revit.DB.Level).ProjectElevation + baseOffset;
                    double topLevel = (document.GetElement(topConst) as Autodesk.Revit.DB.Level).ProjectElevation + topOffset;
                    Autodesk.Revit.DB.XYZ loc = (column.Location as Autodesk.Revit.DB.LocationPoint).Point;

                    cStart = new Autodesk.Revit.DB.XYZ(loc.X, loc.Y, loc.Z + baseLevel);
                    cEnd = new Autodesk.Revit.DB.XYZ(loc.X, loc.Y, loc.Z + topLevel);
                }
            }

            BHoM.Geometry.Point startPoint = new BHoM.Geometry.Point(scale * cStart.X, scale * cStart.Y, scale * cStart.Z); // TODO - Need to check for existing nodes
            BHoM.Geometry.Point endPoint = new BHoM.Geometry.Point(scale * cEnd.X, scale * cEnd.Y, scale * cEnd.Z);
            */

            
            GeometryElement geometry = column.get_Geometry(new Options());
            BoundingBoxXYZ box = geometry.GetBoundingBox();
            XYZ center = (box.Max + box.Min) / 2;
            BHoM.Geometry.Point startPoint = new BHoM.Geometry.Point(scale * center.X, scale * center.Y, scale * box.Min.Z);
            BHoM.Geometry.Point endPoint = new BHoM.Geometry.Point(scale * center.X, scale * center.Y, scale * box.Max.Z);


            /*foreach (GeometryObject obj in geometry)
            {
                Line line = obj as Line;
                if (line != null)
                {
                    cStart = line.GetEndPoint(0);
                    cEnd = line.GetEndPoint(1);
                    startPoint = new BHoM.Geometry.Point(cStart.X, cStart.Y, cStart.Z);
                    endPoint = new BHoM.Geometry.Point(cEnd.X, cEnd.Y, cEnd.Z);
                }
            }*/

            BHoM.Structural.Bar bar = new BHoM.Structural.Bar(startPoint, endPoint);
            bar.CustomData["RevitId"] = column.Id;
            bar.CustomData["RevitType"] = "Column";

            Autodesk.Revit.DB.Material material = document.GetElement(column.StructuralMaterialId) as Autodesk.Revit.DB.Material;
            //bar.Material = RevitToolkit.Materials.Material.ToBHoMMaterial(material);
            bar.OrientationAngle = 0; // TODO - Not sure what this is
            bar.SectionProperty = RevitToolkit.Section_Properties.SectionProperty.ToBHoMSectionProperty(null); //TODO - found proper way to extract section from revit element
            return bar;
        }
    }
}
