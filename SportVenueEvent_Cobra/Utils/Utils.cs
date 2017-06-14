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

            string path = RevitUtils.GetFilepath("Generic");
            Document famdoc = app.NewFamilyDocument(path);
            ARC.FamilyItemFactory factory = famdoc.FamilyCreate;
            ARC.Application crapp = famdoc.Application.Create;

            Transaction t = new Transaction(famdoc, "Create Family Extrusion");

            t.Start();

            Transform trans1 = Transform.CreateTranslation(new XYZ(0, -RevitGeometry.MeterToFeet(BHobj.Width) / 2, 0));

            PolyLine crvA = RevitGeometry.Write(BHobj.Profile);
            CurveArray profile = crapp.NewCurveArray();
            for (int i = 0; i < crvA.GetCoordinates().Count - 1; ++i)
            {
                profile.Append(Line.CreateBound(trans1.OfPoint(crvA.GetCoordinates()[i]), trans1.OfPoint(crvA.GetCoordinates()[i + 1])));
            }
            CurveArrArray profilearr = crapp.NewCurveArrArray();
            profilearr.Append(profile);

            Plane pln = Plane.CreateByNormalAndOrigin(new XYZ(0, 1, 0), new XYZ(0, -RevitGeometry.MeterToFeet(BHobj.Width) / 2, 0));
            SketchPlane spln = SketchPlane.Create(famdoc, pln);
            Extrusion ext = factory.NewExtrusion(true, profilearr, spln, RevitGeometry.MeterToFeet(BHobj.Width));

            Family fam = famdoc.OwnerFamily;
            Categories categories = m_document.Settings.Categories;
            fam.FamilyCategory = categories.get_Item(BuiltInCategory.OST_StructuralFraming);
            RevitUtils.SetLookupParameter(fam, "Material for Model Behavior", "5");

            t.Commit();

            string _rfa_ext = ".rfa";
            string _family_name = BHobj.Name;

            string filename = Path.Combine(
            Path.GetTempPath(), _family_name + _rfa_ext);

            SaveAsOptions opt = new SaveAsOptions();
            opt.OverwriteExistingFile = true;

            famdoc.SaveAs(filename, opt);
            famdoc.Close(false);
            m_document.LoadFamily(filename);

            return RevitUtils.GetFamilySymbolfromDocument(BHobj.Name, m_document);
        }
    }
}
