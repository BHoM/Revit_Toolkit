using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Common.Materials.Material ToBHoMMaterial(this Material material, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Common.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Common.Materials.Material>(material.Id.IntegerValue);
            if (aMaterial != null)
                return aMaterial;

            string materialGrade = material.MaterialGrade();
            switch (material.MaterialClass)
            {
                case "Aluminium":
                    aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;
                    break;
                case "Concrete":
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
                    aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                    break;
                case "Steel":
                case "Metal":
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
                    aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "S355") as oM.Common.Materials.Material;
                    break;
                case "Wood":
                    aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                    break;
                default:
                    materialGrade.MaterialNotFoundWarning();
                    break;
            }

            aMaterial = Modify.SetIdentifiers(aMaterial, material) as oM.Common.Materials.Material;
            if (pullSettings.CopyCustomData)
                aMaterial = Modify.SetCustomData(aMaterial, material, pullSettings.ConvertUnits) as oM.Common.Materials.Material;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);

            return aMaterial;
        }

        /***************************************************/
    }
}
