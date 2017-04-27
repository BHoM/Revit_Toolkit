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
using System.IO;

namespace Revit2016_Adapter.Structural.Properties
{
    public class SectionIO
    {
        public static string GetTemplate(BHoME.Bar member)
        {
            switch (member.StructuralUsage)
            {
                case BHoM.Structural.Elements.BarStructuralUsage.Column:
                    return Revit2016_Adapter.Properties.Resources.ColumnTemplate;
                default:
                    return Revit2016_Adapter.Properties.Resources.BeamTemplate;
            }
        }

        public static string GetFamilyName(BHoME.Bar property )
        {
            return property.StructuralUsage + " - " + property.SectionProperty.Name;
        }
        public static FamilySymbol GetFamilySymbol(Document revitDoc, BHoME.Bar property)
        {
            Element family = new FilteredElementCollector(revitDoc).OfClass(typeof(Family)).FirstOrDefault(e => e.Name == GetFamilyName(property));
            if (family != null && family.Name == GetFamilyName(property))
            {
                foreach (ElementId symbolId in (family as Family).GetFamilySymbolIds())
                {
                    FamilySymbol symbol = revitDoc.GetElement(symbolId) as FamilySymbol;
                    if (symbol.Name == property.SectionProperty.Name)
                    {
                        return symbol;
                    }
                }
            }

            return CreateFamily(revitDoc, property);
        }

        public static FamilySymbol CreateFamily(Document revitDoc, BHoME.Bar property)
        {
            Document familyDoc = revitDoc.Application.NewFamilyDocument(GetTemplate(property));
            Transaction remove = new Transaction(familyDoc, "Remove Existing Extrusions");
            remove.Start();
            IList<Element> list = new FilteredElementCollector(familyDoc).OfClass(typeof(Extrusion)).ToElements();
            foreach (Element element in list)
            {
                familyDoc.Delete(element.Id);
            }

            remove.Commit();

            ReferencePlane rightPlanes = new FilteredElementCollector(familyDoc).OfClass(typeof(ReferencePlane)).First(e => e.Name == "Right") as ReferencePlane;
            ReferencePlane leftPlanes = new FilteredElementCollector(familyDoc).OfClass(typeof(ReferencePlane)).First(e => e.Name == "Left") as ReferencePlane;
            View view = new FilteredElementCollector(familyDoc).OfClass(typeof(View)).First(e => e.Name == "Front") as View;

            double rightX = rightPlanes.GetPlane().Origin.X;
            double length = rightPlanes.GetPlane().Origin.X - leftPlanes.GetPlane().Origin.X;

            BHoMG.Group<BHoMG.Curve> edges = property.SectionProperty.Edges.DuplicateGroup();

            BHoMG.Transform t = BHoMG.Transform.Rotation(BHoMG.Point.Origin, BHoMG.Vector.ZAxis(), Math.PI / 2);
            edges.Transform(t);
            edges.Project(new BHoMG.Plane(GeometryUtils.Convert(rightPlanes.GetPlane().Origin), BHoMG.Vector.XAxis()));

            Transaction create = new Transaction(familyDoc, "Create Extrusion");
            create.Start();
            Extrusion val = familyDoc.FamilyCreate.NewExtrusion(true, GeometryUtils.Convert(edges), SketchPlane.Create(familyDoc, rightPlanes.Id), length);
            create.Commit();
            
            Transaction align = new Transaction(familyDoc, "Create Alignment");
            align.Start();
            familyDoc.FamilyCreate.NewAlignment(view, GetReferenceFace(familyDoc, val, leftPlanes).Reference, leftPlanes.GetReference());
            familyDoc.FamilyCreate.NewAlignment(view, GetReferenceFace(familyDoc, val, rightPlanes).Reference, rightPlanes.GetReference());
            FamilyParameter p = familyDoc.FamilyManager.get_Parameter("Material for Model Behavior");
            familyDoc.FamilyManager.Set(p, 0);
            align.Commit();

            string familyName = Path.Combine(Path.GetTempPath(),  GetFamilyName(property) + ".rft");
            familyDoc.SaveAs(familyName);
            Family f = null;
            Transaction load = new Transaction(revitDoc, "Load " + familyName);
            
            load.Start();
            revitDoc.LoadFamily(familyName, out f);
            load.Commit();

            File.Delete(familyName);
            familyDoc.Close();

            FamilySymbol symbol = revitDoc.GetElement(f.GetFamilySymbolIds().First()) as FamilySymbol;
            symbol.Name = property.SectionProperty.Name;
            //symbol.
            return symbol;
        }

        static PlanarFace GetReferenceFace(Document doc, Extrusion box, ReferencePlane plane)
        {
            Options o = new Options();
            o.ComputeReferences = true;
            GeometryElement geom = box.get_Geometry(o);
            Plane p = plane.GetPlane();
            foreach (GeometryObject obj in geom)
            {
                if (obj is Solid)
                {
                    Solid s = obj as Solid;
                    foreach (Autodesk.Revit.DB.Face face in s.Faces)
                    {
                        PlanarFace pFace = face as PlanarFace;
                        double angle = pFace.FaceNormal.AngleTo(p.Normal);
                        double dotProduct = pFace.FaceNormal.DotProduct(p.Normal);
                        if ((dotProduct > 0 && angle < 0.001 && angle > -0.001) || dotProduct < 0 && angle < Math.PI + 0.001 && angle > Math.PI - 0.001)
                        {
                            if (pFace.Origin.X == p.Origin.X)
                            {
                                return pFace;
                            }
                        }
                    }
                }
            }
            return null;
        }

        //public static BHoMP.SectionType GetSectionType(Autodesk.Revit.DB.Structure.StructuralMaterialType matType, bool IsColumn)
        //{
        //    switch (matType)
        //    {
        //        case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
        //            return BHoMP.SectionType.Aluminium;
        //        case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
        //            return IsColumn ? BHoMP.SectionType.ConcreteColumn : BHoMP.SectionType.ConcreteBeam;
        //        case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
        //            return BHoMP.SectionType.Steel;
        //        case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
        //            return BHoMP.SectionType.Timber;
        //        default:
        //            return BHoMP.SectionType.Steel;
        //    }
        //}

        public static BHoMP.ShapeType GetShapeType(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape type)
        {
            switch (type)
            {
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.RectangularBar:
                    return BHoMP.ShapeType.Rectangle;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.RoundBar:
                    return BHoMP.ShapeType.Circle;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.StructuralTees:
                    return BHoMP.ShapeType.Tee;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.IParallelFlange:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.ISlopedFlange:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.ISplitParallelFlange:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.IWelded:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.IWideFlange:
                    return BHoMP.ShapeType.ISection;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.LAngle:
                    return BHoMP.ShapeType.Angle;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.RectangleHSS:
                    return BHoMP.ShapeType.Box;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.PipeStandard:
                    return BHoMP.ShapeType.Tube;
                default: return BHoMP.ShapeType.Rectangle;
            }
        }


        public static BHoMP.SectionProperty GetSectionProperty(FamilySymbol symbol, bool IsColumn)
        {
            BHoMP.SectionProperty sectionProperty = null;
            try
            {
                sectionProperty = BHoMP.SectionProperty.LoadFromSteelSectionDB(symbol.Name);
            }
            catch
            {

            }
            XYZ direction = IsColumn ? new XYZ(0, 0, 1) : new XYZ(1, 0, 0);

            if (sectionProperty == null)
            {
                BHoMG.Group<BHoMG.Curve> curves = new BHoMG.Group<BHoMG.Curve>();
                foreach (GeometryObject obj in symbol.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (Face face in (obj as Solid).Faces)
                        {
                            if (face is PlanarFace && (face as PlanarFace).FaceNormal.Normalize().IsAlmostEqualTo(direction, 0.001) || (face as PlanarFace).Normal.Normalize().IsAlmostEqualTo(-direction, 0.001))
                            {
                                foreach (EdgeArray curveArray in (face as PlanarFace).EdgeLoops)
                                {
                                    foreach (Edge c in curveArray)
                                    {
                                        curves.Add(GeometryUtils.Convert(c.AsCurve(), 9));
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                if (IsColumn)
                {
                    curves.Project(BHoMG.Plane.XY());
                }
                else
                {
                    curves.Project(BHoMG.Plane.YZ());

                    curves.Transform(BHoMG.Transform.Rotation(BHoMG.Point.Origin, BHoMG.Vector.XAxis(), -Math.PI / 2));
                    curves.Transform(BHoMG.Transform.Rotation(BHoMG.Point.Origin, BHoMG.Vector.YAxis(), -Math.PI / 2));
                }

                BHoMP.ShapeType type = symbol.Family.CanHaveStructuralSection() ? GetShapeType(symbol.Family.StructuralSectionShape) : BHoMP.ShapeType.Rectangle;
                BHoM.Materials.MaterialType matKey = Base.RevitUtils.GetMaterialType(symbol.Family.StructuralMaterialType);
                sectionProperty = BHoMP.SectionProperty.CreateSection(curves, type, matKey);              
            }
            return sectionProperty;
        }

        public static BHoMP.PanelProperty GetThicknessProperty(Wall wall, Document document)
        {
            double thickness = wall.WallType.LookupParameter("Width").AsDouble() * GeometryUtils.FeetToMetre;

            foreach (ElementId id in wall.WallType.GetMaterialIds(false))
            {
                Material m = document.GetElement(id) as Material;
                if (m != null)
                {
                    //m.
                }
            }
            return new BHoMP.ConstantThickness(wall.WallType.Name, thickness, BHoM.Structural.Properties.PanelType.Wall);
        }

        public static BHoMP.PanelProperty GetThicknessProperty(Floor floor, Document document)
        {
            double thickness = floor.LookupParameter("Thickness").AsDouble() * GeometryUtils.FeetToMetre;

            foreach (ElementId id in floor.FloorType.GetMaterialIds(false))
            {
                Material m = document.GetElement(id) as Material;
                if (m != null)
                {
                    //m.
                }
            }
            return new BHoMP.ConstantThickness(floor.FloorType.Name, thickness, BHoM.Structural.Properties.PanelType.Slab);
        }

        internal static BHoMP.PanelProperty GetFoundationProperty(FamilyInstance foundation, Document document)
        {
            if (foundation.Symbol.Category.Name.Contains("Foundation "))
            {
                double thickness = foundation.LookupParameter("Thickness").AsDouble() * GeometryUtils.FeetToMetre;
                return new BHoMP.ConstantThickness(foundation.Symbol.Name, thickness, BHoM.Structural.Properties.PanelType.Slab);
            }
            else
            {
                double thickness = foundation.Symbol.LookupParameter("Pile Cap Depth").AsDouble() * GeometryUtils.FeetToMetre;
                double pileDiam = foundation.Symbol.LookupParameter("Pile Diameter").AsDouble() * GeometryUtils.FeetToMetre;
                double pileDepth = foundation.LookupParameter("Pile Depth").AsDouble() * GeometryUtils.FeetToMetre;
                BHoMP.ConstantThickness pileCap = new BHoMP.ConstantThickness(foundation.Symbol.Name, thickness, BHoM.Structural.Properties.PanelType.PileCap);
                return pileCap;
            }

        }
    }
}
