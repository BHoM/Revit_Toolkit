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
using BH.Engine.Structure;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Structure.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Structure.MaterialFragments.IMaterialFragment ToBHoMMaterialFragment(this Autodesk.Revit.DB.Material material, PullSettings pullSettings = null, string materialGrade = null)
        {
            if (material == null)
            {
                Compute.NullObjectWarning();
                return null;
            }

            pullSettings = pullSettings.DefaultIfNull();
            
            StructuralMaterialType aStructuralMaterialType = Query.StructuralMaterialType(material.MaterialClass);
            IMaterialFragment materialFragment = Query.LibraryMaterial(aStructuralMaterialType, materialGrade);
            if (materialFragment != null)
                return materialFragment;

            Compute.MaterialNotInLibraryNote(material);
            
            switch (aStructuralMaterialType)
            {
                case StructuralMaterialType.Concrete:
                case StructuralMaterialType.PrecastConcrete:
                    materialFragment = new oM.Structure.MaterialFragments.Concrete();
                    break;
                case StructuralMaterialType.Aluminum:
                    materialFragment = new oM.Structure.MaterialFragments.Aluminium();
                    break;
                case StructuralMaterialType.Steel:
                    materialFragment = new oM.Structure.MaterialFragments.Steel();
                    break;
                case StructuralMaterialType.Wood:
                    materialFragment = new oM.Structure.MaterialFragments.Timber();
                    break;
                default:
                    //TODO: default steel warning
                    materialFragment = new oM.Structure.MaterialFragments.Steel();
                    break;
            }

            //TODO: zero strength warning + raise an issue
            materialFragment = materialFragment.Update(material);

            materialFragment.Name = material.Name;

            return materialFragment;
        }

        /***************************************************/

        //TODO: to be deprecated!
        //TODO: Remove this method. The CompundLayerStructure should use the method above, not this one!
        //public static oM.Physical.Materials.Material ToBHoMMaterial(this CompoundStructureLayer compoundStructureLayer, Document document, BuiltInCategory builtInCategory = BuiltInCategory.INVALID, PullSettings pullSettings = null)
        //{
        //    if (compoundStructureLayer == null)
        //        return null;

        //    pullSettings = pullSettings.DefaultIfNull();

        //    oM.Physical.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(compoundStructureLayer.MaterialId.IntegerValue);
        //    if (aMaterial != null)
        //        return aMaterial;

        //    ElementId aElementId = compoundStructureLayer.MaterialId;
        //    Autodesk.Revit.DB.Material aMaterial_Revit = null;
        //    if (aElementId != null && aElementId != ElementId.InvalidElementId)
        //        aMaterial_Revit = document.GetElement(aElementId) as Autodesk.Revit.DB.Material;

        //    if (aMaterial_Revit == null && builtInCategory != BuiltInCategory.INVALID)
        //    {
        //        Category aCategory = document.Settings.Categories.get_Item(builtInCategory);
        //        if (aCategory != null)
        //            aMaterial_Revit = aCategory.Material;
        //    }

        //    if (aMaterial_Revit != null)
        //    {
        //        aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(aMaterial_Revit.Id.IntegerValue);
        //        if (aMaterial != null)
        //            return aMaterial;
        //    }
        //    else
        //    {
        //        Compute.MaterialNotFoundWarning(aMaterial);
        //        return aMaterial;
        //    }

        //    if (aMaterial == null)
        //        aMaterial = new oM.Physical.Materials.Material() { Name = aMaterial_Revit.Name };

        //    aMaterial = aMaterial.Update(aMaterial_Revit);

        //    aMaterial = Modify.SetIdentifiers(aMaterial, aMaterial_Revit) as oM.Physical.Materials.Material;
        //    if (pullSettings.CopyCustomData)
        //        aMaterial = Modify.SetCustomData(aMaterial, aMaterial_Revit, pullSettings.ConvertUnits) as oM.Physical.Materials.Material;

        //    aMaterial = aMaterial.UpdateValues(pullSettings, aMaterial_Revit) as oM.Physical.Materials.Material;

        //    pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);

        //    return aMaterial;
        //}


        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static IMaterialFragment Update(this IMaterialFragment material_Destination, Autodesk.Revit.DB.Material material_Source)
        {
            if (material_Source == null)
            {
                Compute.NullRevitElementWarning(material_Destination);
                return material_Destination;
            }
                
            ElementId aElementId = material_Source.StructuralAssetId;
            if (aElementId == null || aElementId == ElementId.InvalidElementId)
            {
                Compute.NullStructuralAssetWarning(material_Destination);
                return material_Destination;
            }

            Document aDocument = material_Source.Document;

            PropertySetElement aPropertySetElement = aDocument.GetElement(aElementId) as PropertySetElement;
            StructuralAsset structuralAsset = aPropertySetElement.GetStructuralAsset();
            if (structuralAsset == null)
            {
                Compute.NullStructuralAssetWarning(material_Destination);
                return material_Destination;
            }

            return material_Destination.Update(structuralAsset);
        }

        /***************************************************/

        private static IMaterialFragment Update(this IMaterialFragment materialFragment, StructuralAsset structuralAsset)
        {
            double density = UnitUtils.ConvertFromInternalUnits(structuralAsset.Density, DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER);
            double dampingRatio = structuralAsset.DampingRatio;
            oM.Geometry.Vector youngsModulus = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.X, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.Y, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.Z, DisplayUnitType.DUT_PASCALS));
            oM.Geometry.Vector thermalExpansionCoeff = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.X, DisplayUnitType.DUT_INV_CELSIUS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.Y, DisplayUnitType.DUT_INV_CELSIUS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.Z, DisplayUnitType.DUT_INV_CELSIUS));
            oM.Geometry.Vector poissonsRatio = BH.Engine.Geometry.Create.Vector(structuralAsset.PoissonRatio.X, structuralAsset.PoissonRatio.Y, structuralAsset.PoissonRatio.Z);
            oM.Geometry.Vector shearModulus = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.ShearModulus.X, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ShearModulus.Y, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ShearModulus.Z, DisplayUnitType.DUT_PASCALS));

            materialFragment.Density = density;
            materialFragment.DampingRatio = dampingRatio;

            if (materialFragment is BH.oM.Structure.MaterialFragments.Aluminium)
            {
                BH.oM.Structure.MaterialFragments.Aluminium material = materialFragment as BH.oM.Structure.MaterialFragments.Aluminium;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                materialFragment = material;
            }
            else if (materialFragment is BH.oM.Structure.MaterialFragments.Concrete)
            {
                BH.oM.Structure.MaterialFragments.Concrete material = materialFragment as BH.oM.Structure.MaterialFragments.Concrete;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                material.CustomData["Concrete Bending Reinforcement"] = structuralAsset.ConcreteBendingReinforcement;
                material.CustomData["Concrete Shear Reinforcement"] = structuralAsset.ConcreteShearReinforcement;
                material.CustomData["Concrete Shear Strength Reduction"] = structuralAsset.ConcreteShearStrengthReduction;
                materialFragment = material;
            }
            else if (materialFragment is BH.oM.Structure.MaterialFragments.Steel)
            {
                BH.oM.Structure.MaterialFragments.Steel material = materialFragment as BH.oM.Structure.MaterialFragments.Steel;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                materialFragment = material;
            }
            else if (materialFragment is BH.oM.Structure.MaterialFragments.Timber)
            {
                BH.oM.Structure.MaterialFragments.Timber material = materialFragment as BH.oM.Structure.MaterialFragments.Timber;
                material.YoungsModulus = youngsModulus;
                material.ThermalExpansionCoeff = thermalExpansionCoeff;
                material.PoissonsRatio = poissonsRatio;
                materialFragment = material;
            }

            return materialFragment;
        }

        /***************************************************/

        //private static IMaterialFragment SetEmptyStructurallMaterial(this IMaterialFragment material_Destination, string materialClass)
        //{
        //    IMaterialFragment materialFragment = null;

        //    switch (materialClass.ToLower())
        //    {
        //        case "aluminium":
        //            materialFragment = new oM.Structure.MaterialFragments.Aluminium() { Name = material_Destination.Name };
        //            break;
        //        case "concrete":
        //            materialFragment = new oM.Structure.MaterialFragments.Concrete() { Name = material_Destination.Name };
        //            break;
        //        case "steel":
        //        case "metal":
        //            materialFragment = new oM.Structure.MaterialFragments.Steel() { Name = material_Destination.Name };
        //            break;
        //        case "wood":
        //        case "timber":
        //            materialFragment = new oM.Structure.MaterialFragments.Timber() { Name = material_Destination.Name };
        //            break;
        //    }

        //    return materialFragment;
        //}

        /***************************************************/

        //private static void Update(this oM.Physical.Materials.Material material_Destination, ThermalAsset thermalAsset)
        //{

        //}

        /***************************************************/
    }
}
