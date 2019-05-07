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
using BH.oM.Common.Materials;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Physical.Materials.Material ToBHoMMaterial(this Autodesk.Revit.DB.Material material, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            if (material == null)
            {
                Compute.NullObjectWarning();
                return null;
            }

            oM.Physical.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(material.Id.IntegerValue);
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
                switch (material.MaterialClass)
                {
                    case "aluminium":
                        aMaterial = BH.Engine.Structure.Create.AluminiumMaterial(material.Name);
                        break;
                    case "concrete":
                        aMaterial = BH.Engine.Structure.Create.ConcreteMaterial(material.Name);
                        break;
                    case "steel":
                        aMaterial = BH.Engine.Structure.Create.SteelMaterial(material.Name);
                        break;
                    case "metal":
                        aMaterial = BH.Engine.Structure.Create.SteelMaterial(material.Name);
                        break;
                    case "wood":
                        aMaterial = BH.Engine.Structure.Create.TimberMaterial(material.Name, BH.Engine.Geometry.Create.Vector(), BH.Engine.Geometry.Create.Vector(), BH.Engine.Geometry.Create.Vector(), BH.Engine.Geometry.Create.Vector(), double.MinValue, double.MinValue);
                        break;
                }

                if (aMaterial == null)
                    Compute.MaterialTypeNotFoundWarning(material);
            }

            if (aMaterial == null)
            {
                aMaterial = BH.Engine.Structure.Create.ConcreteMaterial(material.Name);
                Compute.InvalidDataMaterialWarning(material);
            }

            aMaterial.Update(material);

            aMaterial = Modify.SetIdentifiers(aMaterial, material) as oM.Physical.Materials.Material;
            if (pullSettings.CopyCustomData)
                aMaterial = Modify.SetCustomData(aMaterial, material, pullSettings.ConvertUnits) as oM.Physical.Materials.Material;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);


            return aMaterial;
        }

        /***************************************************/

        static public oM.Physical.Materials.Material ToBHoMMaterial(this CompoundStructureLayer compoundStructureLayer, Document document, BuiltInCategory builtInCategory = BuiltInCategory.INVALID, PullSettings pullSettings = null)
        {
            if (compoundStructureLayer == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            oM.Physical.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(compoundStructureLayer.MaterialId.IntegerValue);
            if (aMaterial != null)
                return aMaterial;

            ElementId aElementId = compoundStructureLayer.MaterialId;
            Autodesk.Revit.DB.Material aMaterial_Revit = null;
            if (aElementId != null && aElementId != ElementId.InvalidElementId)
                aMaterial_Revit = document.GetElement(aElementId) as Autodesk.Revit.DB.Material;

            if (aMaterial_Revit == null && builtInCategory != BuiltInCategory.INVALID)
            {
                Category aCategory = document.Settings.Categories.get_Item(builtInCategory);
                if (aCategory != null)
                    aMaterial_Revit = aCategory.Material;
            }

            if (aMaterial_Revit != null)
            {
                aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(aMaterial_Revit.Id.IntegerValue);
                if (aMaterial != null)
                    return aMaterial;
            }
            else
            {
                Compute.MaterialNotFoundWarning(aMaterial);
                return aMaterial;
            }

            switch (aMaterial_Revit.MaterialClass)
            {
                case "Aluminium":
                    aMaterial = BH.Engine.Structure.Create.AluminiumMaterial(aMaterial_Revit.Name);
                    break;
                case "Concrete":
                    aMaterial = BH.Engine.Structure.Create.ConcreteMaterial(aMaterial_Revit.Name);
                    break;
                case "Steel":
                    aMaterial = BH.Engine.Structure.Create.SteelMaterial(aMaterial_Revit.Name);
                    break;
                case "Metal":
                    aMaterial = BH.Engine.Structure.Create.SteelMaterial(aMaterial_Revit.Name);
                    break;
                case "Wood":
                    aMaterial = BH.Engine.Structure.Create.TimberMaterial(aMaterial_Revit.Name, BH.Engine.Geometry.Create.Vector(), BH.Engine.Geometry.Create.Vector(), BH.Engine.Geometry.Create.Vector(), BH.Engine.Geometry.Create.Vector(), double.MinValue, double.MinValue);
                    break;
            }

            aMaterial = Modify.SetIdentifiers(aMaterial, aMaterial_Revit) as oM.Physical.Materials.Material;
            if (pullSettings.CopyCustomData)
                aMaterial = Modify.SetCustomData(aMaterial, aMaterial_Revit, pullSettings.ConvertUnits) as oM.Physical.Materials.Material;

            aMaterial = aMaterial.UpdateValues(pullSettings, aMaterial_Revit) as oM.Physical.Materials.Material;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);

            return aMaterial;
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static void Update(this oM.Physical.Materials.Material material_Destination, Autodesk.Revit.DB.Material material_Source)
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
            //material_Destination.Update(aPropertySetElement.GetThermalAsset());
        }

        /***************************************************/

        private static void Update(this oM.Physical.Materials.Material material_Destination, StructuralAsset structuralAsset)
        {
            material_Destination.Density = UnitUtils.ConvertFromInternalUnits(structuralAsset.Density, DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER);


            List<oM.Structure.MaterialFragments.IStructuralMaterial> aStructuralMaterialList = material_Destination.Properties.FindAll(x => x is oM.Structure.MaterialFragments.IStructuralMaterial).Cast<oM.Structure.MaterialFragments.IStructuralMaterial>().ToList();
            if (aStructuralMaterialList != null && aStructuralMaterialList.Count > 0)
            {
                double aDampingRatio = structuralAsset.DampingRatio;
                foreach (oM.Structure.MaterialFragments.IStructuralMaterial aStructuralMaterial in aStructuralMaterialList)
                    aStructuralMaterial.DampingRatio = aDampingRatio;
            }


            if (structuralAsset.Behavior != StructuralBehavior.Isotropic)
            {
                Compute.NonIsotopicStructuralAssetNote(material_Destination);
                return;
            }
            else
            {
                List<oM.Structure.MaterialFragments.IIsotropic> aIsotropicList = material_Destination.Properties.FindAll(x => x is oM.Structure.MaterialFragments.IIsotropic).Cast<oM.Structure.MaterialFragments.IIsotropic>().ToList();
                if (aIsotropicList != null && aIsotropicList.Count > 0)
                {
                    double aYoungsModulus = UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.X, DisplayUnitType.DUT_PASCALS);
                    double aThermalExpansionCoeff = UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.X, DisplayUnitType.DUT_INV_CELSIUS);
                    double aPoissonsRatio = structuralAsset.PoissonRatio.X;
                    foreach (oM.Structure.MaterialFragments.IIsotropic aIsotropic in aIsotropicList)
                    {
                        aIsotropic.YoungsModulus = aYoungsModulus;
                        aIsotropic.ThermalExpansionCoeff = aThermalExpansionCoeff;
                        aIsotropic.PoissonsRatio = aPoissonsRatio;
                    }
                    aIsotropicList.ForEach(x => x.YoungsModulus = aYoungsModulus);
                }
            }

            List<oM.Structure.MaterialFragments.Concrete> aConcreteList = material_Destination.Properties.FindAll(x => x is oM.Structure.MaterialFragments.Concrete).Cast<oM.Structure.MaterialFragments.Concrete>().ToList();
            if (aConcreteList != null && aConcreteList.Count > 0)
            {
                double aConcreteBendingReinforcement = structuralAsset.ConcreteBendingReinforcement;
                double aConcreteShearReinforcement = structuralAsset.ConcreteShearReinforcement;
                double aConcreteShearStrengthReduction = structuralAsset.ConcreteShearStrengthReduction;
                foreach (oM.Structure.MaterialFragments.Concrete aConcrete in aConcreteList)
                {
                    aConcrete.CustomData["Concrete Bending Reinforcement"] = aConcreteBendingReinforcement;
                    aConcrete.CustomData["Concrete Shear Reinforcement"] = aConcreteShearReinforcement;
                    aConcrete.CustomData["Concrete Shear Strength Reduction"] = aConcreteShearStrengthReduction;
                }
            }

            List<oM.Structure.MaterialFragments.Timber> aTimberList = material_Destination.Properties.FindAll(x => x is oM.Structure.MaterialFragments.Timber).Cast<oM.Structure.MaterialFragments.Timber>().ToList();
            if (aTimberList != null && aTimberList.Count > 0)
            {
                oM.Geometry.Vector aYoungsModulus = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.X, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.Y, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.Z, DisplayUnitType.DUT_PASCALS));
                oM.Geometry.Vector aThermalExpansionCoeff = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.X, DisplayUnitType.DUT_INV_CELSIUS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.Y, DisplayUnitType.DUT_INV_CELSIUS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.Z, DisplayUnitType.DUT_INV_CELSIUS));
                oM.Geometry.Vector aPoissonsRatio = BH.Engine.Geometry.Create.Vector(structuralAsset.PoissonRatio.X, structuralAsset.PoissonRatio.Y, structuralAsset.PoissonRatio.Z);
                foreach (oM.Structure.MaterialFragments.Timber aTimber in aTimberList)
                {
                    aTimber.YoungsModulus = aYoungsModulus;
                    aTimber.ThermalExpansionCoeff = aThermalExpansionCoeff;
                    aTimber.PoissonsRatio = aPoissonsRatio;
                }
            }

        }

        /***************************************************/

        private static void Update(this oM.Physical.Materials.Material material_Destination, ThermalAsset thermalAsset)
        {

        }

        /***************************************************/
    }
}
