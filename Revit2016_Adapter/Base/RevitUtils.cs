using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revit2016_Adapter.Base
{
    public class RevitUtils
    {
        public const string REVIT_ID_KEY = "Revit Id";

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
                    return BHoM.Materials.MaterialType.Concrete;
            }
        }

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

        public static Autodesk.Revit.DB.Material GetMaterial(Document revit, BHoM.Materials.MaterialType type)
        {
            foreach (Material m in new FilteredElementCollector(revit).OfClass(typeof(Material)))
            {
                if (m.Name == type.ToString())
                {
                    return m;
                }
            }

            return new FilteredElementCollector(revit).OfClass(typeof(Material)).First() as Material;
        }
    }
}
