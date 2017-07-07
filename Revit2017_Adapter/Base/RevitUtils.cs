using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.IO;
using System.ComponentModel;

namespace Revit2017_Adapter.Base
{
    /// <summary>
    /// Object for Revit Section Property Names.
    /// </summary>
    public class SectionPropertyName
    {
        public string pathID { get; private set; }
        public List<string> typeName { get; private set; }
        public string secName { get; private set; }
        public SectionPropertyName(string sectionName)
        {
            string name = sectionName;
            secName = sectionName;

            if (sectionName.Contains("RHSH"))
            {
                name = sectionName.Replace("RHSH", "RHS");
            }
            pathID = name.Split(' ')[0];

            List<string> typelist = new List<string>();
            typelist.Add(name.Replace(" ", ""));
            if (name.ElementAt(name.Length - 2) != '.')
            {
                string extname = name.Replace(" ", "");
                typelist.Add(extname + ".0");
            }

            typeName = typelist;
        }

    }

    /// <summary>
    /// Utilities to interact with Revit.
    /// </summary>
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

        public static Autodesk.Revit.DB.Structure.StructuralType StructuralType(BHoM.Structural.Elements.BarStructuralUsage type)
        {
            switch (type)
            {
                case BHoM.Structural.Elements.BarStructuralUsage.Beam:
                    return Autodesk.Revit.DB.Structure.StructuralType.Beam;
                case BHoM.Structural.Elements.BarStructuralUsage.Undefined:
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
                    return "Metal";
                case BHoM.Materials.MaterialType.Steel:
                    return "Metal";
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

        public static int RevitMaterialBehaviourIndex(string type)
        {
            switch (type)
            {
                case "Cable":
                    return 1;
                case "Aluminium":
                    return 1;
                case "Steel":
                    return 1;
                case "Metal":
                    return 1;
                case "Concrete":
                    return 2;
                case "Wood":
                    return 3;
                case "Precast Concrete":
                    return 5;
                default:
                    return 4;
            }
        }

        /// <summary>
        /// Get Material from Document.
        /// </summary>
        public static Autodesk.Revit.DB.Material GetMaterial(Document revit, BHoM.Materials.MaterialType type)
        {
            foreach (Material m in GetElement(revit,typeof(Material)))
            {
                if (m.MaterialClass == RevitMaterialClass(type))
                {
                    return m;
                }
            }
            return (Material)GetElement(revit, typeof(Material))[0];
        }

        /// <summary>
        /// Get Material from Document by name.
        /// </summary>
        public static Autodesk.Revit.DB.Material GetMaterial(Document revit, string name)
        {
            foreach (Material m in GetElement(revit, typeof(Material)))
            {
                if (m.Name.Contains(name))
                {
                    return m;
                }
            }
            return (Material)GetElement(revit, typeof(Material))[0];
        }

        // -------------------------------------------------------------------------------------------------------------------------------------------

        /**********************************************/
        /****  FamilySymbols                       ****/
        /**********************************************/

        /// <summary>
        /// Load FamilySymbol.
        /// </summary>
        public static FamilySymbol GetFamilySymbol(Document activeDoc, SectionPropertyName name)
        {
            FamilySymbol familySymbol = null;
            foreach (string symName in name.typeName)
            {
                familySymbol = (FamilySymbol)GetElement(activeDoc, typeof(FamilySymbol), symName);
                if (familySymbol != null)
                {
                    return familySymbol;
                }
            }
            foreach (string symName in name.typeName)
            {
                familySymbol = GetFamilySymbolfromPath(symName, name.pathID, activeDoc);
                if (familySymbol != null)
                {
                    return familySymbol;
                }
            }
            return null;
        }

        /// <summary>
        /// Load FamilySymbol from path into Document and get Family Symbol.
        /// </summary>
        public static FamilySymbol GetFamilySymbolfromPath(string FamilySymbolname, string pathKey, Document activeDoc)
        {
            string filename = GetFilepath(pathKey);
            if (filename != null)
            {
                FamilySymbol familySymbol = null;
                activeDoc.LoadFamilySymbol(filename, FamilySymbolname, out familySymbol);
                return familySymbol;
            }
            return null;
        }

        /// <summary>
        /// Get filepath from Resources using key.
        /// </summary>
        public static string GetFilepath(string pathKey)
        {
            ResourceSet resourceSet = Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                if (pathKey == entry.Key.ToString())
                {
                    return (string)entry.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Create Structural Framing Family.
        /// </summary>
        public static FamilySymbol CreateExtrusionFamilySymbol(BHoM.Structural.Elements.Bar BHobj, Document doc)
        {
            Autodesk.Revit.UI.UIApplication app = new Autodesk.Revit.UI.UIApplication(doc.Application);
            Document familyDoc = app.Application.NewFamilyDocument(GetFilepath("BeamTemplate"));

            RemoveExtrusions(familyDoc);

            ReferencePlane rightPlane = (ReferencePlane)GetElement(familyDoc, typeof(ReferencePlane), "Right");
            ReferencePlane leftPlane = (ReferencePlane)GetElement(familyDoc, typeof(ReferencePlane), "Left");
            View view = (View)GetElement(familyDoc, typeof(View), "Front");

            CurveArrArray arr = Engine.Convert.RevitGeometry.OrientSectionToYZPlane(BHobj.SectionProperty.Edges, Engine.Convert.RevitGeometry.Read(rightPlane.GetPlane()));
            if (Engine.Convert.RevitGeometry.CheckPlanar(arr, rightPlane.GetPlane()) != true)
            {
                return null;
            }
            if (Engine.Convert.RevitGeometry.CheckClosed(arr) != true)
            {
                return null;
            }

            Extrusion val = CreateExtrusion(familyDoc, arr, rightPlane.GetPlane(), rightPlane.GetPlane().Origin.X - leftPlane.GetPlane().Origin.X);

            Transaction align = new Transaction(familyDoc, "Create Alignment");
            align.Start();
            familyDoc.FamilyCreate.NewAlignment(view, GetFace(familyDoc, val, leftPlane).Reference, leftPlane.GetReference());
            familyDoc.FamilyCreate.NewAlignment(view, GetFace(familyDoc, val, rightPlane).Reference, rightPlane.GetReference());

            familyDoc.FamilyManager.Set(familyDoc.FamilyManager.get_Parameter("Structural Material"), GetMaterial(familyDoc, BHobj.Material.Type).Id);
            SetElementParameter(familyDoc.OwnerFamily, BuiltInParameter.FAMILY_STRUCT_MATERIAL_TYPE, RevitMaterialBehaviourIndex(RevitMaterialClass(BHobj.Material.Type)).ToString());
            align.Commit();
            string name = new SectionPropertyName(BHobj.SectionProperty.Name).typeName[0];
            ReloadFamily(familyDoc, doc, name);
            return (FamilySymbol)GetElement(doc, typeof(FamilySymbol), name);
        }

        /**********************************************/
        /****  Elements                            ****/
        /**********************************************/

        /// <summary>
        /// Get Revit Element.
        /// </summary>
        public static Element GetElement(Document doc, Type targetType, string targetName)
        {
            return new FilteredElementCollector(doc).OfClass(targetType).ToElements().ToList().Find(e => e.Name.Equals(targetName));
        }

        /// <summary>
        /// Get Revit ElementList.
        /// </summary>
        public static IList<Element> GetElement(Document doc, Type targetType)
        {
            return new FilteredElementCollector(doc).OfClass(targetType).ToElements();
        }

        /// <summary>
        /// Get Revit Level from elevation,
        /// 0 = Closest Below,
        /// 1 = Closest Above,
        /// 2 = Closest
        /// </summary>
        public static Level GetLevel(Document doc, double elevation, int mode)
        {
            List<Element> elelist = new FilteredElementCollector(doc).OfClass(typeof(Level)).ToList();
            SortedDictionary<double, Level> lvldic = new SortedDictionary<double, Level>();
            foreach (Element lvl in elelist)
            {
                Level level = (Level)lvl;
                lvldic.Add(level.ProjectElevation, level);
            }
            List<Level> lvllist = lvldic.Values.ToList();

            switch (mode)
            {
                case 0:
                    for (int i = 0; i < lvllist.Count; i++)
                    {
                        if (lvllist[i].ProjectElevation > elevation)
                        {
                            return lvllist[i - 1];
                        }
                    }
                    return lvllist.Last();

                case 1:
                    for (int i = 0; i < lvllist.Count; i++)
                    {
                        if (lvllist[i].ProjectElevation > elevation)
                        {
                            return lvllist[i];
                        }
                    }
                    return lvllist.Last();

                case 2:
                    int index = 0;
                    double dist = lvllist[0].ProjectElevation;
                    for (int i = 1; i < lvllist.Count; i++)
                    {
                        if (Math.Abs(lvllist[i].ProjectElevation - elevation) < Math.Abs(dist - elevation))
                        {
                            dist = Math.Abs(lvllist[i].ProjectElevation - elevation);
                            index = i;
                        }
                    }
                    return lvllist[index];

                default:
                    return lvllist.Last();
            }   

            
            
        }

        /**********************************************/
        /****  Parameters                          ****/
        /**********************************************/

        /// <summary>
        /// Set lookup parameter on element.
        /// </summary>
        public static void SetElementParameter(Element obj, string name, string value)
        {
            Parameter param = obj.LookupParameter(name);
            if (param != null)
            {
                if (param.IsReadOnly != true)
                {
                    if (param.StorageType.ToString() == "ElementId")
                    {
                        param.Set(Convert.ToDouble(value));
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
        public static void SetElementParameter(Element obj, string name, ElementId id)
        {
            Parameter param = obj.LookupParameter(name);
            if (param != null)
            {
                if (param.IsReadOnly != true)
                {
                    param.Set(id);
                }
            }
        }

        /// <summary>
        /// Set built in parameter on element.
        /// </summary>
        public static void SetElementParameter(Element obj, BuiltInParameter builtInParameter, string value)
        {
            Parameter param = obj.get_Parameter(builtInParameter);
            if (param != null)
            {
                if (param.IsReadOnly != true)
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
        public static void SetElementParameter(Element obj, BuiltInParameter builtInParameter, ElementId id)
        {
            Parameter param = obj.get_Parameter(builtInParameter);
            if (param != null)
            {
                if (param.IsReadOnly != true)
                {
                    param.Set(id);
                }
            }
        }



        /**********************************************/
        /****  Misc                                ****/
        /**********************************************/

        /// <summary>
        /// Disable end join for bars.
        /// </summary>
        public static void DisableEndJoin(FamilyInstance bar)
        {
            if (bar.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam)
            {
                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(bar, 0);
                Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(bar, 1);
            }
        }

        public static PlanarFace GetFace(Document doc, Extrusion box, ReferencePlane plane)
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
                        //if ((dotProduct > 0 && angle < 0.001 && angle > -0.001) || dotProduct < 0 && angle < Math.PI + 0.001 && angle > Math.PI - 0.001)
                        //{
                        if (Math.Abs(pFace.Origin.X - p.Origin.X) < 0.001)
                        {
                            return pFace;
                        }
                        //}
                    }
                }
            }
            return null;
        }

        public static PlanarFace GetFace(Document doc, Extrusion box, double X)
        {
            Options o = new Options();
            o.ComputeReferences = true;
            GeometryElement geom = box.get_Geometry(o);
            foreach (GeometryObject obj in geom)
            {
                if (obj is Solid)
                {
                    Solid s = obj as Solid;
                    foreach (Autodesk.Revit.DB.Face face in s.Faces)
                    {
                        PlanarFace pFace = face as PlanarFace;
                        if (Math.Abs(pFace.Origin.X - X) < 0.001)
                        {
                            return pFace;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Removes extrusions from document.
        /// </summary>
        public static void RemoveExtrusions(Document familyDoc)
        {
            Transaction remove = new Transaction(familyDoc, "Remove Existing Extrusions");
            remove.Start();
            IList<Element> list = GetElement(familyDoc,typeof(Extrusion));
            foreach (Element element in list)
            {
                familyDoc.Delete(element.Id);
            }
            remove.Commit();
        }

        /// <summary>
        /// Creates extrusion in document.
        /// </summary>
        public static Extrusion CreateExtrusion(Document familyDoc, CurveArrArray arr, Plane pln, double length)
        {
            Transaction create = new Transaction(familyDoc, "Create Extrusion");
            create.Start();
            Extrusion val = familyDoc.FamilyCreate.NewExtrusion(true, arr, SketchPlane.Create(familyDoc, pln), length);
            create.Commit();
            return val;
        }

        /// <summary>
        /// Saves and reloads family into main document.
        /// </summary>
        public static void ReloadFamily(Document familyDoc, Document projectDoc, string familyName)
        {
            string _rfa_ext = ".rfa";
            string filename = Path.Combine(Path.GetTempPath(), familyName + _rfa_ext);
            SaveAsOptions opt = new SaveAsOptions();
            opt.OverwriteExistingFile = true;
            familyDoc.SaveAs(filename, opt);
            familyDoc.Close(false);
            projectDoc.LoadFamily(filename);
        }
    }
}