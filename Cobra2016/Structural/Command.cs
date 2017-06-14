using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Cobra2016.Structural.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BHoM.Global;
using BHoM.Base;
using BHoM.Structural.Elements;
using Revit2016_Adapter.Structural;
using BHoM.Structural;

namespace Cobra2016.Structural
{
    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            new ExportForm(commandData.Application.ActiveUIDocument.Document).ShowDialog();

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class ImportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string filename = Path.Combine(Path.GetTempPath(), "RevitExchange");

            Document doc = commandData.Application.ActiveUIDocument.Document;
            Document familyDoc = commandData.Application.Application.NewFamilyDocument(@"L:\BH-Revit\Revit2016\Family Templates\English\Metric Structural Framing - Beams and Braces.rft");
            Transaction remove = new Transaction(familyDoc, "Remove Existing Extrusions");
            remove.Start();
            IList<Element> list= new FilteredElementCollector(familyDoc).OfClass(typeof(Extrusion)).ToElements(); 
            foreach (Element element in list)
            {
                familyDoc.Delete(element.Id);
            }

            remove.Commit();


            ReferencePlane rightPlanes = new FilteredElementCollector(familyDoc).OfClass(typeof(ReferencePlane)).First(e=>e.Name == "Right") as ReferencePlane;
            ReferencePlane leftPlanes = new FilteredElementCollector(familyDoc).OfClass(typeof(ReferencePlane)).First(e => e.Name == "Left") as ReferencePlane;
            View view = new FilteredElementCollector(familyDoc).OfClass(typeof(View)).First(e => e.Name == "Front") as View;

            double rightX = rightPlanes.GetPlane().Origin.X;
            double length = rightPlanes.GetPlane().Origin.X - leftPlanes.GetPlane().Origin.X;

            BHoM.Structural.Properties.SectionProperty section = BHoM.Structural.Properties.SectionProperty.LoadFromSteelSectionDB("UC254x254x89");

            CurveArrArray arr = Revit2016_Adapter.Geometry.GeometryUtils.Convert(section.Edges);

            Transaction create = new Transaction(familyDoc, "Create Extrusion");
            create.Start();            
            Extrusion val = familyDoc.FamilyCreate.NewExtrusion(true, arr, SketchPlane.Create(familyDoc, rightPlanes.Id), length);            
            create.Commit();
            
            Transaction align = new Transaction(familyDoc, "Create Alignment");
            align.Start();
            familyDoc.FamilyCreate.NewAlignment(view, GetFace(familyDoc, val, leftPlanes).Reference, leftPlanes.GetReference());
            familyDoc.FamilyCreate.NewAlignment(view, GetFace(familyDoc, val, rightPlanes).Reference, rightPlanes.GetReference());

            FamilyParameter p = familyDoc.FamilyManager.get_Parameter("Structural Material");

            familyDoc.FamilyManager.Set(p, Revit2016_Adapter.Base.RevitUtils.GetMaterial(familyDoc, BHoM.Materials.MaterialType.Steel).Id);
            List<Family> fams = new FilteredElementCollector(familyDoc).OfClass(typeof(Family)).Cast<Family>().ToList();

            if (fams.Count > 0)
            {
                Parameter par = fams[0].LookupParameter("Material for Model Behavior");
                par.Set(1);
                //1 Steel
                //2 Concrete
                //3 Prescast
                //4 Wood
                //5 Other
            }

            align.Commit();

            familyDoc.SaveAs(@"C:\Users\edalton\Desktop\test.rft");
            Family fam = null;
            doc.LoadFamily(@"C:\Users\edalton\Desktop\text.rft", out fam);

            File.Delete(@"C:\Users\edalton\Desktop\test.rft");

            return Result.Succeeded;
        }

        PlanarFace GetFace(Document doc, Extrusion box, ReferencePlane plane)
        {
            Options o = new Options();
            o.ComputeReferences = true;
            GeometryElement geom = box.get_Geometry(o);
            Plane p = plane.GetPlane();
            foreach (GeometryObject obj in geom)
            {
                if(obj is Solid)
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
    }



    [Transaction(TransactionMode.Automatic)]
    [Regeneration(RegenerationOption.Manual)]
    public class SetParameters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            string path = Path.Combine(Path.GetTempPath(), "RevitParams.csv");

            using (StreamReader fs = new StreamReader(path))
            {
                string[] headings = fs.ReadLine().Split(',');

                while (fs.Peek() >= 0)
                {
                    string[] data = fs.ReadLine().Split(',');

                    string id = data[0];

                    ElementId revitId = new ElementId(int.Parse(id));

                    Element e = doc.GetElement(revitId);
                    
                    if (e != null)
                    {
                        for (int i = 1; i < headings.Length; i++)
                        {
                            Parameter p = e.LookupParameter(headings[i]);
                            if (p != null)
                            {
                                bool succeeded = false;
                                try
                                {
                                    switch (p.StorageType)
                                    {
                                        case StorageType.Double:
                                            succeeded = e.LookupParameter(headings[i]).Set(double.Parse(data[i]));
                                            break;
                                        case StorageType.Integer:
                                            succeeded = e.LookupParameter(headings[i]).Set(int.Parse(data[i]));
                                            break;
                                        case StorageType.String:
                                            succeeded = e.LookupParameter(headings[i]).Set(data[i]);
                                            break;
                                        case StorageType.ElementId:
                                            succeeded = e.LookupParameter(headings[i]).Set(new ElementId(int.Parse(data[i])));
                                            break;
                                    }
                                }
                                catch
                                {
                                    message += "Error: Parameter data " + headings[i] + "->" + data[i] + " on Element " + e.Name + " (Id=" + e.Id + ") could not be cast to a " + p.StorageType + "\n";
                                }
                            }
                            else
                            {
                                message += "Error: Parameter " + headings[i] + " does not exists on Element " + e.Name;
                            }
                        }
                    }
                    else
                    {
                        message += "Error: Element " + id + " does not exists in the project";
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
