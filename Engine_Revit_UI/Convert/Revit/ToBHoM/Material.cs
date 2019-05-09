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
                aMaterial = new oM.Physical.Materials.Material() { Name = material.Name };

            aMaterial = aMaterial.Update(material);

            aMaterial = Modify.SetIdentifiers(aMaterial, material) as oM.Physical.Materials.Material;
            if (pullSettings.CopyCustomData)
                aMaterial = Modify.SetCustomData(aMaterial, material, pullSettings.ConvertUnits) as oM.Physical.Materials.Material;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);


            return aMaterial;
        }

        /***************************************************/

        //TODO: Remove this method. The CompundLayerStructure should use the method above, not this one!
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

        private static oM.Physical.Materials.Material Update(this oM.Physical.Materials.Material material_Destination, Autodesk.Revit.DB.Material material_Source)
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
            return material_Destination.Update(aPropertySetElement.GetStructuralAsset(), material_Source.MaterialClass);
            //material_Destination.Update(aPropertySetElement.GetThermalAsset());
        }

        /***************************************************/

        private static oM.Physical.Materials.Material Update(this oM.Physical.Materials.Material material_Destination, StructuralAsset structuralAsset, string materialClass)
        {
            double density = UnitUtils.ConvertFromInternalUnits(structuralAsset.Density, DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER);
            double dampingRatio = structuralAsset.DampingRatio;
            oM.Geometry.Vector youngsModulus = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.X, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.Y, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.YoungModulus.Z, DisplayUnitType.DUT_PASCALS));
            oM.Geometry.Vector thermalExpansionCoeff = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.X, DisplayUnitType.DUT_INV_CELSIUS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.Y, DisplayUnitType.DUT_INV_CELSIUS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ThermalExpansionCoefficient.Z, DisplayUnitType.DUT_INV_CELSIUS));
            oM.Geometry.Vector poissonsRatio = BH.Engine.Geometry.Create.Vector(structuralAsset.PoissonRatio.X, structuralAsset.PoissonRatio.Y, structuralAsset.PoissonRatio.Z);
            oM.Geometry.Vector shearModulus = BH.Engine.Geometry.Create.Vector(UnitUtils.ConvertFromInternalUnits(structuralAsset.ShearModulus.X, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ShearModulus.Y, DisplayUnitType.DUT_PASCALS), UnitUtils.ConvertFromInternalUnits(structuralAsset.ShearModulus.Z, DisplayUnitType.DUT_PASCALS));


            switch (materialClass.ToLower())
            {
                case "aluminium":
                    material_Destination = material_Destination.SetAluminium(material_Destination.Name, youngsModulus.X, poissonsRatio.X, thermalExpansionCoeff.X, density, dampingRatio);
                    break;
                case "concrete":
                    double cubeStrength = 0;
                    double cylinderStrength = 0;
                    material_Destination = material_Destination.SetConcrete(material_Destination.Name, youngsModulus.X, poissonsRatio.X, thermalExpansionCoeff.X, density, dampingRatio, cubeStrength, cylinderStrength);
                    material_Destination.CustomData["Concrete Bending Reinforcement"] = structuralAsset.ConcreteBendingReinforcement;
                    material_Destination.CustomData["Concrete Shear Reinforcement"] = structuralAsset.ConcreteShearReinforcement;
                    material_Destination.CustomData["Concrete Shear Strength Reduction"] = structuralAsset.ConcreteShearStrengthReduction;
                    break;
                case "steel":
                case "metal":
                    double yieldStress = UnitUtils.ConvertFromInternalUnits(structuralAsset.MinimumYieldStress, DisplayUnitType.DUT_PASCALS);
                    double ultimateStress = UnitUtils.ConvertFromInternalUnits(structuralAsset.MinimumTensileStrength, DisplayUnitType.DUT_PASCALS);
                    material_Destination = material_Destination.SetSteel(material_Destination.Name, youngsModulus.X, poissonsRatio.X, thermalExpansionCoeff.X, density, dampingRatio, yieldStress, ultimateStress);
                    break;
                case "wood":
                    material_Destination = material_Destination.SetTimber(material_Destination.Name, youngsModulus, poissonsRatio, shearModulus, thermalExpansionCoeff, density, dampingRatio);
                    break;
            }

            return material_Destination;
            

        }

        /***************************************************/

        private static void Update(this oM.Physical.Materials.Material material_Destination, ThermalAsset thermalAsset)
        {

        }

        /***************************************************/
    }
}
