using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ARA = Autodesk.Revit.ApplicationServices;
using ARC = Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Engine.Convert;
using Revit2017_Adapter.Base;

namespace SportVenueEvent_Cobra
{
    class Utils
    {
        public static FamilySymbol CreateExtrusionFamilySymbol(BHoM.Structural.Elements.ConcreteRakerBeam BHobj, Document m_document)
        {
            ARA.Application app = m_document.Application;
            Document famdoc = app.NewFamilyDocument(RevitUtils.GetFilepath("ComplexFramingTemplate"));
            
            Transform trans1 = Transform.CreateTranslation(new XYZ(0, -RevitGeometry.MeterToFeet(BHobj.Width) / 2, 0));

            PolyLine crvA = RevitGeometry.Write(BHobj.Profile);
            CurveArray profile = new CurveArray();
            for (int i = 0; i < crvA.GetCoordinates().Count - 1; ++i)
            {
                profile.Append(Line.CreateBound(trans1.OfPoint(crvA.GetCoordinates()[i]), trans1.OfPoint(crvA.GetCoordinates()[i + 1])));
            }
            CurveArrArray profilearr = new CurveArrArray();
            profilearr.Append(profile);

            Extrusion ext = RevitUtils.CreateExtrusion(famdoc, profilearr, Plane.CreateByNormalAndOrigin(new XYZ(0, 1, 0), new XYZ(0, -RevitGeometry.MeterToFeet(BHobj.Width) / 2, 0)), RevitGeometry.MeterToFeet(BHobj.Width));

            if (BHobj.Bars != null)
            {

                //ReferencePlane LeftPlane = (ReferencePlane)RevitUtils.GetElement(famdoc, typeof(ReferencePlane), "Center (Left/Right)");
                //ReferencePlane FrontPlane = (ReferencePlane)RevitUtils.GetElement(famdoc, typeof(ReferencePlane), "Center(Front / Back)");
                //PlanarFace face = RevitUtils.GetFace(famdoc, ext, LeftPlane);
                //Extrusion ext2 = RevitUtils.CreateExtrusion(famdoc, RevitGeometry.EdgesToCurves(face.EdgeLoops), LeftPlane.GetPlane(), RevitGeometry.MeterToFeet(BHobj.Bars[0].Length));
                RevitElement.Write(BHobj.Bars[0], famdoc);
                //ReferenceArray refer = new ReferenceArray();
                //refer.Append(LeftPlane.GetReference());
                //refer.Append(RevitUtils.GetFace(famdoc, ext2, RevitGeometry.MeterToFeet(BHobj.Bars[0].EndPoint.X)).Reference);
                //Dimension dim = famdoc.FamilyCreate.NewDimension(view, RevitGeometry.Write(BHobj.Bars[0].Line), refer);

                //FamilyParameter param = famdoc.FamilyManager.AddParameter("BeamLength", BuiltInParameterGroup.PG_IDENTITY_DATA, ParameterType.Length, true);
                //dim.FamilyLabel = param;
            }
            Transaction t = new Transaction(famdoc, "Set Parameters");
            t.Start();


            Family fam = famdoc.OwnerFamily;
            fam.FamilyCategory = m_document.Settings.Categories.get_Item(BuiltInCategory.OST_StructuralFraming);

            RevitUtils.SetElementParameter(fam, BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, RevitUtils.RevitMaterialBehaviourIndex("Precast Concrete").ToString());

            Material mat = RevitUtils.GetMaterial(famdoc, "Precast Concrete");
            if (mat != null)
            {
                Parameter param = fam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
                if (param != null)
                {
                    RevitUtils.SetElementParameter(fam, BuiltInParameter.STRUCTURAL_MATERIAL_PARAM, mat.Id);
                }
            }

            t.Commit();

            RevitUtils.ReloadFamily(famdoc, m_document, BHobj.Name);

            return (FamilySymbol)RevitUtils.GetElement( m_document,typeof(FamilySymbol), BHobj.Name);
        }
    }
}
