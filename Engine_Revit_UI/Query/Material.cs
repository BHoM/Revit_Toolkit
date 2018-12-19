using Autodesk.Revit.DB.Structure;

using BH.oM.Base;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public oM.Common.Materials.Material Material(this StructuralMaterialType structuralMaterialType, string materialGrade)
        {
            switch (structuralMaterialType)
            {
                case StructuralMaterialType.Aluminum:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                case StructuralMaterialType.Concrete:
                case StructuralMaterialType.PrecastConcrete:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject concrete in BH.Engine.Library.Query.Match("MaterialsEurope", "Type", "Concrete"))
                        {
                            if (materialGrade.Contains((concrete).Name))
                            {
                                return concrete as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                case StructuralMaterialType.Steel:
                    if (materialGrade != null)
                    {
                        foreach (IBHoMObject steel in BH.Engine.Library.Query.Match("MaterialsEurope", "Type", "Steel"))
                        {
                            if (materialGrade.Contains((steel).Name))
                            {
                                return steel as oM.Common.Materials.Material;
                            }
                        }
                    }
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                case StructuralMaterialType.Wood:
                    return BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                default:
                    materialGrade.MaterialNotFoundWarning();
                    return null;
            }
        }

        /***************************************************/
    }
}
