using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit2017_Adapter.Base
{
    public class RevitUtils
    {
        public const string REVIT_ID_KEY = "Revit Id";

        /**********************************************/
        /****  Material                            ****/
        /**********************************************/

        /// <summary>
        /// Get MaterialType from Document.
        /// </summary>
        public static BHoM.Materials.MaterialType GetMaterialType(Autodesk.Revit.DB.Structure.StructuralMaterialType type)
        {
            switch (type)
            {
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum:
                    return BHoM.Materials.MaterialType.Aluminium;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete:
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.PrecastConcrete:
                    return BHoM.Materials.MaterialType.Concrete;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel:
                    return BHoM.Materials.MaterialType.Steel;
                case Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood:
                    return BHoM.Materials.MaterialType.Timber;
                default:
                    return BHoM.Materials.MaterialType.Steel;
            }
        }

        /// <summary>
        /// Get MaterialType from BHoMObject.
        /// </summary>
        public static Autodesk.Revit.DB.Structure.StructuralMaterialType GetMaterialType(BHoM.Materials.MaterialType type)
        {
            switch (type)
            {
                case BHoM.Materials.MaterialType.Aluminium:
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Aluminum;
                case BHoM.Materials.MaterialType.Concrete:
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete;
                case BHoM.Materials.MaterialType.Steel:
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Steel;
                case BHoM.Materials.MaterialType.Timber:
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Wood;
                default:
                    return Autodesk.Revit.DB.Structure.StructuralMaterialType.Concrete;
            }
        }


        internal static BHoM.Structural.Elements.BarStructuralUsage StructuralType(Autodesk.Revit.DB.Structure.StructuralType type)
        {
            switch (type)
            {
                case Autodesk.Revit.DB.Structure.StructuralType.Beam:
                    return BHoM.Structural.Elements.BarStructuralUsage.Beam;
                case Autodesk.Revit.DB.Structure.StructuralType.Brace:
                    return BHoM.Structural.Elements.BarStructuralUsage.Brace;
                case Autodesk.Revit.DB.Structure.StructuralType.Column:
                    return BHoM.Structural.Elements.BarStructuralUsage.Column;
                case Autodesk.Revit.DB.Structure.StructuralType.Footing:
                    return BHoM.Structural.Elements.BarStructuralUsage.Pile;
                case Autodesk.Revit.DB.Structure.StructuralType.NonStructural:
                default:
                    return BHoM.Structural.Elements.BarStructuralUsage.Undefined;
            }
        }

        internal static Autodesk.Revit.DB.Structure.StructuralType StructuralType(BHoM.Structural.Elements.BarStructuralUsage type)
        {
            switch (type)
            {
                case BHoM.Structural.Elements.BarStructuralUsage.Beam:
                    return Autodesk.Revit.DB.Structure.StructuralType.Beam;
                case BHoM.Structural.Elements.BarStructuralUsage.Brace:
                    return Autodesk.Revit.DB.Structure.StructuralType.Brace;
                case BHoM.Structural.Elements.BarStructuralUsage.Column:
                    return Autodesk.Revit.DB.Structure.StructuralType.Column;
                case BHoM.Structural.Elements.BarStructuralUsage.Pile:
                    return Autodesk.Revit.DB.Structure.StructuralType.Footing;
                default:
                    return Autodesk.Revit.DB.Structure.StructuralType.NonStructural;
            }
        }

        public static string RevitMaterialClass(BHoM.Materials.MaterialType type)
        {
            switch (type)
            {
                case BHoM.Materials.MaterialType.Aluminium:
                case BHoM.Materials.MaterialType.Steel:
                case BHoM.Materials.MaterialType.Cable:
                    return "Metal";
                case BHoM.Materials.MaterialType.Concrete:
                    return "Concrete";
                case BHoM.Materials.MaterialType.Glass:
                    return "Glass";
                case BHoM.Materials.MaterialType.Timber:
                    return "Wood";
                default:
                    return "Unassigned";

            }
        }

        public static int RevitMaterialBehaviour(BHoM.Materials.MaterialType type)
        {
            switch (type)
            {
                case BHoM.Materials.MaterialType.Aluminium:
                case BHoM.Materials.MaterialType.Steel:
                case BHoM.Materials.MaterialType.Cable:
                    return 0;
                case BHoM.Materials.MaterialType.Concrete:
                    return 1;
                case BHoM.Materials.MaterialType.Timber:
                    return 3;
                default:
                    return 4;

            }
        }

        /// <summary>
        /// Get Material from Document.
        /// </summary>
        public static Autodesk.Revit.DB.Material GetMaterial(Document revit, BHoM.Materials.MaterialType type)
        {
            foreach (Material m in new FilteredElementCollector(revit).OfClass(typeof(Material)))
            {
                if (m.MaterialClass == RevitMaterialClass(type))
                {
                    return m;
                }
            }

            return new FilteredElementCollector(revit).OfClass(typeof(Material)).First() as Material;
        }


        // -------------------------------------------------------------------------------------------------------------------------------------------

        /**********************************************/
        /****  FamilySymbols                       ****/
        /**********************************************/

        /// <summary>
        /// Get Family Symbol from Document.
        /// </summary>
        public static FamilySymbol GetFamilySymbolfromDocument(string FamilySymbolname, Document activeDoc)
        {
            FilteredElementCollector FamilySymbolCollector = new FilteredElementCollector(activeDoc).OfClass(typeof(Autodesk.Revit.DB.FamilySymbol));
            List<FamilySymbol> documentFamilySymbol = FamilySymbolCollector.ToElements().ToList().ConvertAll(x => x as Autodesk.Revit.DB.FamilySymbol);
            FamilySymbol familySymbol = documentFamilySymbol.Find(x => x.Name == FamilySymbolname);
            return familySymbol;
        }

        /// <summary>
        /// Load FamilySymbol from path into Document and get Family Symbol.
        /// </summary>
        public static FamilySymbol GetFamilySymbolfromPath(string FamilySymbolname, string pathKey, Document activeDoc)
        {
            string filename = RevitUtils.GetFilepath(pathKey);
            if (filename == null)
            {
                return null;
            }
            else
            {
                FamilySymbol familySymbol = null;
                activeDoc.LoadFamilySymbol(filename, FamilySymbolname, out familySymbol);
                return familySymbol;
            }
        }

        /// <summary>
        /// Get filepath from Json DB using key.
        /// </summary>
        public static string GetFilepath(string pathKey)
        {
            string filepath = null;
            Dictionary<string, string> pathDictionary = new Dictionary<string, string>();
            pathDictionary = (Dictionary<string, string>)BHoM.Base.JsonReader.ReadObject(Properties.Resources.FamilyPathDB);
            pathDictionary.TryGetValue(pathKey, out filepath);
            return filepath;
        }

        internal static PlanarFace GetFace(Document doc, Extrusion box, ReferencePlane plane)
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

        /// <summary>
        /// Not done!
        /// </summary>
        public static FamilySymbol CreateExtrusionFamilySymbol(BHoM.Structural.Elements.Bar BHobj, Document doc)
        {
            Autodesk.Revit.UI.UIApplication app = new Autodesk.Revit.UI.UIApplication(doc.Application);
            Document familyDoc = app.Application.NewFamilyDocument(@"L:\BH-Revit\Revit2016\Family Templates\English\Metric Structural Framing - Beams and Braces.rft");
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

            BHoM.Structural.Properties.SectionProperty section = BHobj.SectionProperty;

            CurveArrArray arr = Revit2017_Adapter.Geometry.GeometryUtils.Convert(section.Edges);

            Transaction create = new Transaction(familyDoc, "Create Extrusion");
            create.Start();
            Extrusion val = familyDoc.FamilyCreate.NewExtrusion(true, arr, SketchPlane.Create(familyDoc, rightPlanes.Id), length);
            create.Commit();

            Transaction align = new Transaction(familyDoc, "Create Alignment");
            align.Start();
            familyDoc.FamilyCreate.NewAlignment(view, GetFace(familyDoc, val, leftPlanes).Reference, leftPlanes.GetReference());
            familyDoc.FamilyCreate.NewAlignment(view, GetFace(familyDoc, val, rightPlanes).Reference, rightPlanes.GetReference());

            FamilyParameter p = familyDoc.FamilyManager.get_Parameter("Structural Material");

            familyDoc.FamilyManager.Set(p, Revit2017_Adapter.Base.RevitUtils.GetMaterial(familyDoc, BHoM.Materials.MaterialType.Steel).Id);
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

            familyDoc.SaveAs(@"C:\Users\oborgstr\Desktop\test.rft");
            Family fam = null;
            doc.LoadFamily(@"C:\Users\oborgstr\Desktop\text.rft", out fam);

            //File.Delete(@"C:\Users\oborgstr\Desktop\test.rft");
            return RevitUtils.GetFamilySymbolfromDocument(BHobj.Name, doc);
        }

        /// <summary>
        /// Get Revit Element.
        /// </summary>

        public static Element GetElement(Document doc, Type targetType, string targetName)
        {
            return new FilteredElementCollector(doc).OfClass(targetType).First(e => e.Name.Equals(targetName));
        }
        public static Level GetLevel(Document doc, double elevation)
        {
            List<Element> elelist = new FilteredElementCollector(doc).OfClass(typeof(Level)).ToList();
            Dictionary<double,Level> lvldic = new Dictionary<double, Level>();
            
            foreach (Element lvl in elelist)
            {
                Level level = (Level)lvl;
                lvldic.Add(level.get_Parameter(BuiltInParameter.LEVEL_ELEV).AsDouble(),level);
            }
            List<Level> lvllist = lvldic.Values.ToList();
            return Revit2017_Adapter.Structural.Elements.LevelIO.GetLevel(lvllist, elevation);
        }

        /**********************************************/
        /****  Parameters                          ****/
        /**********************************************/

        /// <summary>
        /// Set single parameter on Element.
        /// </summary>
        public static void SetLookupParameter(Element obj, string name, string value)
        {
            Parameter param = obj.LookupParameter(name);
            if (param != null)
            {
                if (param.IsReadOnly != true)
                {
                    if (param.StorageType.ToString() == "ElementId")
                    {
                        if (value == "<By Category>")
                        {
                            param.Set(value);
                        }
                        else
                        {
                            param.Set(Convert.ToDouble(value));
                        }
                    }
                    else
                    {
                        switch (param.StorageType)
                        {
                            case StorageType.Double:
                                param.Set(Convert.ToDouble(value));
                                break;
                            case StorageType.Integer:
                                param.Set(Convert.ToInt32(value));
                                break;
                            case StorageType.String:
                                param.Set(value);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set multiple parameters on Element.
        /// </summary>
        public static void SetLookupParameter(Element obj, List<string> names, List<string> values)
        {
            if (names.Count == values.Count)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    Parameter param = obj.LookupParameter(names[i]);
                    StorageType st = param.StorageType;
                    if (param != null)
                    {
                        if (param.IsReadOnly != true)
                        {
                            if (st.ToString() == "ElementId")
                            {
                                if (values[i] == "<By Category>")
                                {
                                    param.Set(values[i]);
                                }
                                else
                                {
                                    param.Set(Convert.ToDouble(values[i]));
                                }
                            }
                            else
                            {
                                switch (st)
                                {
                                    case StorageType.Double:
                                        param.Set(Convert.ToDouble(values[i]));
                                        break;
                                    case StorageType.Integer:
                                        param.Set(Convert.ToInt32(values[i]));
                                        break;
                                    case StorageType.String:
                                        param.Set(values[i]);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Disable end join for Bars.
        /// </summary>
        public static void DisableEndJoin(FamilyInstance bar)
        {
            Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(bar, 0);
            Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(bar, 1);
        }


        internal static void SetElementParameter(Element element, string parameterName, string value)
        {
            Parameter parameter = element.LookupParameter(parameterName);
            if (parameterName != null)
            {
                parameter.Set(value);
            }
        }

        internal static void SetElementParameter(Element element, string parameterName, double value)
        {
            if (element != null && !double.IsInfinity(value) && !double.IsNaN(value))
            {
                Parameter parameter = element.LookupParameter(parameterName);
                if (parameterName != null)
                {
                    parameter.Set(value);
                }
            }
        }

        internal static void SetElementParameter(Element element, string parameterName, int value)
        {
            if (element != null)
            {
                Parameter parameter = element.LookupParameter(parameterName);
                if (parameterName != null)
                {
                    parameter.Set(value);
                }
            }
        }


        internal static void SetElementParameter(Element element, BuiltInParameter builtInParameter, int p)
        {
            Parameter parameter = element.get_Parameter(builtInParameter);
            parameter.Set(p);
        }

    }
}