using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BHoM.Structural.SectionProperties;
using BH = BHoM.Geometry;
using SP = BHoM.Structural.SectionProperties;

namespace RevitToolkit2015
{
    public class SectionIO
    {
        public static SP.SectionType GetSectionType(Autodesk.Revit.DB.Structure.StructuralMaterialType matType, bool IsColumn)
        {
            switch (matType)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    return SP.SectionType.Aluminium;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                    return IsColumn ? SP.SectionType.ConcreteColumn : SP.SectionType.ConcreteBeam;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    return BHoM.Structural.SectionProperties.SectionType.Steel;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    return BHoM.Structural.SectionProperties.SectionType.Timber;
                default:
                    return BHoM.Structural.SectionProperties.SectionType.Steel;
            }
        }

        public static BHoM.Structural.SectionProperties.ShapeType GetShapeType(Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape type)
        {
            switch (type)
            {
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.RectangularBar:
                    return ShapeType.Rectangle;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.RoundBar:
                    return ShapeType.Circle;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.StructuralTees:
                    return ShapeType.Tee;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.IParallelFlange:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.ISlopedFlange:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.ISplitParallelFlange:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.IWelded:
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.IWideFlange:
                    return ShapeType.ISection;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.LAngle:
                    return ShapeType.Angle;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.RectangleHSS:
                    return ShapeType.Box;
                case Autodesk.Revit.DB.Structure.StructuralSections.StructuralSectionShape.PipeStandard:
                    return ShapeType.Tube;
                default: return ShapeType.Rectangle;
            }
        }


        public static BHoM.Structural.SectionProperties.SectionProperty GetSectionProperty(FamilySymbol symbol, bool IsColumn)
        {
            SectionProperty sectionProperty = SectionProperty.LoadFromDB(symbol.Name);
            XYZ direction = IsColumn ? new XYZ(0, 0, 1) : new XYZ(1, 0, 0);

            if (sectionProperty == null)
            {
                BH.Group<BH.Curve> curves = new BH.Group<BHoM.Geometry.Curve>();
                foreach (GeometryObject obj in symbol.get_Geometry(new Options()))
                {
                    if (obj is Solid)
                    {
                        foreach (Face face in (obj as Solid).Faces)
                        {
                            if (face is PlanarFace && (face as PlanarFace).Normal.Normalize().IsAlmostEqualTo(direction, 0.001) || (face as PlanarFace).Normal.Normalize().IsAlmostEqualTo(-direction, 0.001))
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
                    curves.Project(BH.Plane.XY());
                }
                else
                {
                    curves.Project(BH.Plane.YZ());

                    curves.Transform(BH.Transform.Rotation(BH.Point.Origin, BH.Vector.XAxis(), -Math.PI / 2));
                    curves.Transform(BH.Transform.Rotation(BH.Point.Origin, BH.Vector.YAxis(), -Math.PI / 2));
                }

                BHoM.Structural.SectionProperties.ShapeType type = symbol.Family.HasStructuralSection() ? GetShapeType(symbol.Family.StructuralSectionShape) : ShapeType.Rectangle;
                sectionProperty = new SectionProperty(curves, type, GetSectionType(symbol.Family.StructuralMaterialType, IsColumn));
            }
            return sectionProperty;
        }

        public static BHoM.Structural.ThicknessProperty GetThicknessProperty(Wall wall, Document document)
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
            return new BHoM.Structural.ConstantThickness(wall.WallType.Name, thickness);
        }

        public static BHoM.Structural.ThicknessProperty GetThicknessProperty(Floor floor, Document document)
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
            return new BHoM.Structural.ConstantThickness(floor.FloorType.Name, thickness);
        }
    }
}
