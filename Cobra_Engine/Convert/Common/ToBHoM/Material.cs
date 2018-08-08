using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using BH.oM.Base;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Common.Materials.Material ToBHoMMaterial(this StructuralMaterialType structuralMaterialType, string materialGrade)
        {
            switch (structuralMaterialType)
            {
                case StructuralMaterialType.Aluminum:
                    return Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case StructuralMaterialType.Concrete:
                case StructuralMaterialType.PrecastConcrete:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case StructuralMaterialType.Steel:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case StructuralMaterialType.Wood:
                    return Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    materialGrade.MaterialNotFoundWarning();
                    return null;
            }
        }

        /***************************************************/

        internal static oM.Common.Materials.Material ToBHoMMaterial(this Material material)
        {
            string materialGrade = material.MaterialGrade();
            switch (material.MaterialClass)
            {
                case "Aluminium":
                    return Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case "Concrete":
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case "Steel":
                case "Metal":
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case "Wood":
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    materialGrade.MaterialNotFoundWarning();
                    return null;
            }
        }

        /***************************************************/
    }
}
