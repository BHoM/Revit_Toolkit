using System.Collections.Generic;
using Autodesk.Revit.DB;
using Revit.GeometryConversion;
using Revit.Elements;
using RevitServices.Transactions;
using Autodesk.DesignScript.Interfaces;


namespace RevitToolkit.Elements
{
    /// <summary>
    /// A Revit floor
    /// </summary>
    public static class Floor
    {
        /// <summary>
        /// Creates floor based on Curves
        /// </summary>
        /// <param name="Curves">Curves</param>
        /// <param name="FloorType">Floor Type</param>
        /// <param name="Level">Level</param>
        /// <param name="Structural">Is floor structural element</param>
        /// <returns name="Floor">Floor</returns>
        /// <search>
        /// Create floor, create floor
        /// </search>
        public static Revit.Elements.Floor ByCurves(List<Autodesk.DesignScript.Geometry.Curve> Curves, Revit.Elements.FloorType FloorType, Revit.Elements.Level Level, bool Structural = false)
        {
            CurveArray aCurveArray = new CurveArray();
            Curves.ForEach(x => aCurveArray.Append(x.ToRevitType(false)));
            Autodesk.Revit.DB.Document aDocument = FloorType.InternalElement.Document;
            TransactionManager.Instance.EnsureInTransaction(aDocument);
            Autodesk.Revit.DB.Floor aFloor = aDocument.Create.NewFloor(aCurveArray, FloorType.InternalElement as Autodesk.Revit.DB.FloorType, Level.InternalElement as Autodesk.Revit.DB.Level, Structural);
            TransactionManager.Instance.TransactionTaskDone();
            return aFloor.ToDSType(true) as Revit.Elements.Floor;
        }

        /// <summary>
        /// Create BHoM Panel from Revit Floor
        /// </summary>
        public static object ToBHomPanel(Autodesk.Revit.DB.Floor floor)
        {
            // Get floor Contour
            Autodesk.Revit.DB.Document document = floor.Document;
            Transaction t = new Transaction(document);
            t.Start("Get Floor Model Lines");
            ICollection<ElementId> ids = document.Delete(floor.Id);
            t.RollBack();
            t.Dispose();

            // Get floor elevation
            double scale = RevitToolkit.Global.GeometryConverter.FeetToMetre;
            ElementId levelId = floor.LookupParameter("Level").AsElementId();
            double z = scale * ((document.GetElement(levelId) as Autodesk.Revit.DB.Level).ProjectElevation + floor.LookupParameter("Height Offset From Level").AsDouble());

            // Convert contour into BHoM Geometry
            BHoM.Geometry.Group<BHoM.Geometry.Curve> edges = new BHoM.Geometry.Group<BHoM.Geometry.Curve>();
            foreach (ElementId id in ids)
            {
                Autodesk.Revit.DB.Element e = document.GetElement(id);
                if (e is Autodesk.Revit.DB.ModelCurve)
                {
                    Autodesk.Revit.DB.Curve curve = (e as Autodesk.Revit.DB.ModelCurve).GeometryCurve;
                    XYZ cStart = curve.GetEndPoint(0);
                    XYZ cEnd = curve.GetEndPoint(1);
                    edges.Add(new BHoM.Geometry.Line(scale*cStart.X, scale*cStart.Y, z, scale*cEnd.X, scale*cEnd.Y, z));
                }
            }

            // Create Panel
            BHoM.Structural.Panel panel = new BHoM.Structural.Panel(edges);
            panel.ThicknessProperty = new BHoM.Structural.ConstantThickness(floor.Name);
            panel.ThicknessProperty.Thickness = scale * floor.LookupParameter("Thickness").AsDouble();
            panel.CustomData["RevitId"] = floor.Id;

            return panel;
        }
    }
}
