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
using Autodesk.Revit.DB.Structure;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Common.Materials;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Common.Materials.Material ToBHoMMaterial(this Autodesk.Revit.DB.Material material, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            if (material == null)
            {
                Compute.NullObjectWarning();
                return null;
            }

            oM.Common.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Common.Materials.Material>(material.Id.IntegerValue);
            if (aMaterial != null)
                return aMaterial;

            string materialGrade = material.MaterialGrade();
            StructuralMaterialType aStructuralMaterialType = Query.StructuralMaterialType(material.MaterialClass);
            if (!string.IsNullOrWhiteSpace(materialGrade) && aStructuralMaterialType != StructuralMaterialType.Undefined)
            {
                aMaterial = Query.LibraryMaterial(aStructuralMaterialType, materialGrade);
                if (aMaterial == null)
                    Compute.MaterialNotInLibraryNote(material);
            }

            if (aMaterial == null)
            {
                MaterialType? aMaterialType = Query.MaterialType(material.MaterialClass);
                if (aMaterialType != null && aMaterialType.HasValue)
                    aMaterial = BH.Engine.Common.Create.Material(material.Name, aMaterialType.Value, 0, 0, 0, 0);
                else
                    Compute.MaterialTypeNotFoundWarning(material);
            }

            if (aMaterial == null)
            {
                aMaterial = BH.Engine.Common.Create.Material(material.Name, MaterialType.Steel, 0, 0, 0, 0);
                Compute.InvalidDataMaterialWarning(material);
            }

            aMaterial.Update(material);

            aMaterial = Modify.SetIdentifiers(aMaterial, material) as oM.Common.Materials.Material;
            if (pullSettings.CopyCustomData)
                aMaterial = Modify.SetCustomData(aMaterial, material, pullSettings.ConvertUnits) as oM.Common.Materials.Material;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);


            return aMaterial;
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static void Update(this oM.Common.Materials.Material material_Destination, Autodesk.Revit.DB.Material material_Source)
        {
            if (material_Source == null)
            {
                Compute.NullRevitElementWarning(material_Destination);
                return;
            }
                
            ElementId aElementId = material_Source.StructuralAssetId;
            if (aElementId == null || aElementId == ElementId.InvalidElementId)
            {
                Compute.NullStructuralAssetWarning(material_Destination);
                return;
            }

            Document aDocument = material_Source.Document;

            PropertySetElement aPropertySetElement = aDocument.GetElement(aElementId) as PropertySetElement;
            material_Destination.Update(aPropertySetElement.GetStructuralAsset());
            material_Destination.Update(aPropertySetElement.GetThermalAsset());
        }

        /***************************************************/

        private static void Update(this oM.Common.Materials.Material material_Destination, StructuralAsset structuralAsset)
        {
            material_Destination.Density = UnitUtils.ConvertFromInternalUnits(structuralAsset.Density, DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER);
            material_Destination.CompressiveYieldStrength = UnitUtils.ConvertFromInternalUnits(structuralAsset.ConcreteCompression, DisplayUnitType.DUT_PASCALS);

            material_Destination.CustomData["Damping Ratio"] = structuralAsset.DampingRatio;
            material_Destination.CustomData["Lightweight"] = structuralAsset.Lightweight;

            switch (material_Destination.Type)
            {
                case oM.Common.Materials.MaterialType.Concrete:
                    material_Destination.CustomData["Concrete Bending Reinforcement"] = structuralAsset.ConcreteBendingReinforcement;
                    material_Destination.CustomData["Concrete Shear Reinforcement"] = structuralAsset.ConcreteShearReinforcement;
                    material_Destination.CustomData["Concrete Shear Strength Reduction"] = structuralAsset.ConcreteShearStrengthReduction;
                    break;
            }

            if (structuralAsset.Behavior != StructuralBehavior.Isotropic)
            {
                Compute.NonIsotopicStructuralAssetNote(material_Destination);
                return;
            }

            material_Destination.YoungsModulus = UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.X, DisplayUnitType.DUT_PASCALS);
            material_Destination.CoeffThermalExpansion = UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.X, DisplayUnitType.DUT_INV_CELSIUS);
            material_Destination.PoissonsRatio = structuralAsset.PoissonRatio.X;
        }

        /***************************************************/

        private static void Update(this oM.Common.Materials.Material material_Destination, ThermalAsset thermalAsset)
        {

        }

        /***************************************************/
    }
}
