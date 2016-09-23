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


namespace Revit2016_Adapter.Structural.Properties
{
    public class SectionIO
    {
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
                sectionProperty = BHoMP.SectionProperty.LoadFromDB(symbol.Name);
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

                BHoMP.ShapeType type = symbol.Family.HasStructuralSection() ? GetShapeType(symbol.Family.StructuralSectionShape) : BHoMP.ShapeType.Rectangle;
                sectionProperty = new BHoMP.SectionProperty(curves, type);
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
