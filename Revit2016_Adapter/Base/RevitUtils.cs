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
                    return BHoM.Materials.MaterialType.Steel;
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
