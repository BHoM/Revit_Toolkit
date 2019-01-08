/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Common.Materials.Material ToBHoMMaterial(this Material material, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            if(material == null)
            {
                Compute.NullObjectWarning();
                return null;
            }

            oM.Common.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Common.Materials.Material>(material.Id.IntegerValue);
            if (aMaterial != null)
                return aMaterial;

            string materialGrade = material.MaterialGrade();
            switch (material.MaterialClass)
            {
                case "Aluminium":
                    //aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "ALUM") as oM.Common.Materials.Material;

                    aMaterial = BH.Engine.Common.Create.Material(material.Name, oM.Common.Materials.MaterialType.Aluminium, 0, 0, 0, 0);
                    aMaterial.Update(material);
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
                    //aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "C30/37") as oM.Common.Materials.Material;
                    aMaterial = BH.Engine.Common.Create.Material(material.Name, oM.Common.Materials.MaterialType.Concrete, 0, 0, 0, 0);
                    aMaterial.Update(material);
                    break;
                case "Steel":
                    aMaterial = BH.Engine.Common.Create.Material(material.Name, oM.Common.Materials.MaterialType.Steel, 0, 0, 0, 0);
                    aMaterial.Update(material);
                    break;
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
                    aMaterial = BH.Engine.Common.Create.Material(material.Name, oM.Common.Materials.MaterialType.Steel, 0, 0, 0, 0);
                    aMaterial.Update(material);
                    break;
                case "Wood":
                    //aMaterial = BH.Engine.Library.Query.Match("MaterialsEurope", "TIMBER") as oM.Common.Materials.Material;
                    aMaterial = BH.Engine.Common.Create.Material(material.Name, oM.Common.Materials.MaterialType.Timber, 0, 0, 0, 0);
                    aMaterial.Update(material);
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
        /****             Private methods               ****/
        /***************************************************/

        private static void Update(this oM.Common.Materials.Material material_Destination, Material material_Source)
        {
            if (material_Source == null)
                return;

            ElementId aElementId = material_Source.StructuralAssetId;
            if (aElementId == null || aElementId == ElementId.InvalidElementId)
                return;

            Document aDocument = material_Source.Document;

            PropertySetElement aPropertySetElement = aDocument.GetElement(aElementId) as PropertySetElement;

            StructuralAsset aStructuralAsset = aPropertySetElement.GetStructuralAsset();

            if (aStructuralAsset.Behavior != StructuralBehavior.Isotropic)
                return;

            material_Destination.YoungsModulus = UnitUtils.ConvertFromInternalUnits(aStructuralAsset.YoungModulus.X, DisplayUnitType.DUT_PASCALS);
            material_Destination.Density = UnitUtils.ConvertFromInternalUnits(aStructuralAsset.Density, DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER);
            material_Destination.CoeffThermalExpansion = UnitUtils.ConvertFromInternalUnits(aStructuralAsset.ThermalExpansionCoefficient.X,DisplayUnitType.DUT_INV_CELSIUS);
            material_Destination.PoissonsRatio = aStructuralAsset.PoissonRatio.X;
            material_Destination.CompressiveYieldStrength = UnitUtils.ConvertFromInternalUnits(aStructuralAsset.ConcreteCompression, DisplayUnitType.DUT_PASCALS);

            //material_Destination.CustomData["Damping Ratio"] = aStructuralAsset.DampingRatio;
            //material_Destination.CustomData["Lightweight"] = aStructuralAsset.Lightweight;

            //switch (material_Destination.Type)
            //{
            //    case oM.Common.Materials.MaterialType.Concrete:
            //        material_Destination.CustomData["Concrete Bending Reinforcement"] = aStructuralAsset.ConcreteBendingReinforcement;
            //        material_Destination.CustomData["Concrete Shear Reinforcement"] = aStructuralAsset.ConcreteShearReinforcement;
            //        material_Destination.CustomData["Concrete Shear Strength Reduction"] = aStructuralAsset.ConcreteShearStrengthReduction;
            //        break;
            //}
        }
    }
}
