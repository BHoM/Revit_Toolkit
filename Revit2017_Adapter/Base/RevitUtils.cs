﻿using Autodesk.Revit.DB;
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

        public static void GetRevitParameters(Element element, BHoM.Base.BHoMObject bhomObj)
        {
            foreach (Parameter p in element.Parameters)
            {
                if (p.HasValue && p.Definition.ParameterType != ParameterType.Invalid)
                {
                    try
                    {
                        switch (p.StorageType)
                        {
                            case StorageType.Double:
                                bhomObj.CustomData.Add(p.Definition.Name, p.AsDouble());
                                break;
                            case StorageType.ElementId:
                                bhomObj.CustomData.Add(p.Definition.Name, p.AsElementId().IntegerValue);
                                break;
                            case StorageType.Integer:
                                bhomObj.CustomData.Add(p.Definition.Name, p.AsInteger());
                                break;
                            default:
                                string s = p.AsString();
                                if (!string.IsNullOrEmpty(s))
                                {
                                    bhomObj.CustomData.Add(p.Definition.Name, s);
                                }
                                break;
                        }

                    }
                    catch (Exception ex)
                    {

                    }
                }
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
