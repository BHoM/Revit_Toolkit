/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Environment.MaterialFragments;
using BH.oM.Structure.MaterialFragments;
using System;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****             Interface methods             ****/
        /***************************************************/

        public static void ICopyCharacteristics(this BH.oM.Physical.Materials.IMaterialProperties toMaterial, Material fromMaterial)
        {
            CopyCharacteristics(toMaterial as dynamic, fromMaterial as dynamic);
        }

        /***************************************************/

        public static void ICopyCharacteristics(this Material toMaterial, BH.oM.Physical.Materials.IMaterialProperties fromMaterial)
        {
            CopyCharacteristics(toMaterial as dynamic, fromMaterial as dynamic);
        }

        
        /***************************************************/
        /****        Public methods - Materials         ****/
        /***************************************************/

        public static void CopyCharacteristics(this SolidMaterial toMaterial, Material fromMaterial)
        {
            if (fromMaterial == null)
            {
                toMaterial.NullRevitElementWarning();
                return;
            }

            ElementId elementID = fromMaterial.ThermalAssetId;
            if (elementID == null || elementID == ElementId.InvalidElementId)
            {
                toMaterial.NullThermalAssetWarning();
                return;
            }

            PropertySetElement propertySetElement = fromMaterial.Document.GetElement(elementID) as PropertySetElement;
            ThermalAsset thermalAsset = propertySetElement?.GetThermalAsset();
            if (thermalAsset == null)
            {
                Compute.NullThermalAssetWarning(toMaterial);
                return;
            }

            toMaterial.CopyCharacteristics(thermalAsset);
        }

        /***************************************************/

        public static void CopyCharacteristics(this IMaterialFragment toMaterial, Material fromMaterial)
        {
            if (fromMaterial == null)
            {
                toMaterial.NullRevitElementWarning();
                return;
            }

            ElementId elementID = fromMaterial.StructuralAssetId;
            if (elementID == null || elementID == ElementId.InvalidElementId)
            {
                toMaterial.NullStructuralAssetWarning();
                return;
            }

            PropertySetElement propertySetElement = fromMaterial.Document.GetElement(elementID) as PropertySetElement;
            StructuralAsset structuralAsset = propertySetElement?.GetStructuralAsset();
            if (structuralAsset == null)
            {
                Compute.NullStructuralAssetWarning(toMaterial);
                return;
            }

            toMaterial.CopyCharacteristics(structuralAsset);
        }


        /***************************************************/
        /****       Fallback methods - Materials        ****/
        /***************************************************/

        private static void CopyCharacteristics(this BH.oM.Physical.Materials.IMaterialProperties toMaterial, Material fromMaterial)
        {
            BH.Engine.Reflection.Compute.RecordWarning(String.Format("Copying characteristics to BHoM material fragment of type {0} is currently not supported. Revit ElementId: {1}", toMaterial.GetType().Name, fromMaterial.Id.IntegerValue));
        }

        /***************************************************/

        private static void CopyCharacteristics(this Material toMaterial, BH.oM.Physical.Materials.IMaterialProperties fromMaterial)
        {
            BH.Engine.Reflection.Compute.RecordWarning(String.Format("Copying characteristics from BHoM material fragment of type {0} is currently not supported. BHoM_Guid: {1}", toMaterial.GetType().Name, fromMaterial.BHoM_Guid));
        }


        /***************************************************/
        /****          Public methods - Assets          ****/
        /***************************************************/

        public static void CopyCharacteristics(this SolidMaterial toMaterial, ThermalAsset fromAsset)
        {
            toMaterial.Conductivity = fromAsset.ThermalConductivity.ToSI(UnitType.UT_HVAC_ThermalConductivity);
            toMaterial.SpecificHeat = fromAsset.SpecificHeat.ToSI(UnitType.UT_HVAC_SpecificHeat);
            toMaterial.Density = fromAsset.Density.ToSI(UnitType.UT_MassDensity);
        }

        /***************************************************/

        public static void CopyCharacteristics(this IMaterialFragment toMaterial, StructuralAsset fromAsset)
        {
            double density = fromAsset.Density.ToSI(UnitType.UT_MassDensity);

#if REVIT2020

#else
            double dampingRatio = fromAsset.DampingRatio;
#endif

            oM.Geometry.Vector youngsModulus = BH.Engine.Geometry.Create.Vector(fromAsset.YoungModulus.X.ToSI(UnitType.UT_Stress), fromAsset.YoungModulus.Y.ToSI(UnitType.UT_Stress), fromAsset.YoungModulus.Z.ToSI(UnitType.UT_Stress));
            oM.Geometry.Vector thermalExpansionCoeff = BH.Engine.Geometry.Create.Vector(fromAsset.ThermalExpansionCoefficient.X.ToSI(UnitType.UT_ThermalExpansion), fromAsset.ThermalExpansionCoefficient.Y.ToSI(UnitType.UT_ThermalExpansion), fromAsset.ThermalExpansionCoefficient.Z.ToSI(UnitType.UT_ThermalExpansion));
            oM.Geometry.Vector poissonsRatio = BH.Engine.Geometry.Create.Vector(fromAsset.PoissonRatio.X, fromAsset.PoissonRatio.Y, fromAsset.PoissonRatio.Z);
            oM.Geometry.Vector shearModulus = BH.Engine.Geometry.Create.Vector(fromAsset.ShearModulus.X.ToSI(UnitType.UT_Stress), fromAsset.ShearModulus.Y.ToSI(UnitType.UT_Stress), fromAsset.ShearModulus.Z.ToSI(UnitType.UT_Stress));

            toMaterial.Density = density;
#if REVIT2020

#else
            toMaterial.DampingRatio = dampingRatio;
#endif

            if (toMaterial is Aluminium)
            {
                Aluminium material = toMaterial as Aluminium;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
            }
            else if (toMaterial is Concrete)
            {
                Concrete material = toMaterial as Concrete;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                material.CustomData["Concrete Bending Reinforcement"] = fromAsset.ConcreteBendingReinforcement;
                material.CustomData["Concrete Shear Reinforcement"] = fromAsset.ConcreteShearReinforcement;
                material.CustomData["Concrete Shear Strength Reduction"] = fromAsset.ConcreteShearStrengthReduction;
            }
            else if (toMaterial is Steel)
            {
                Steel material = toMaterial as Steel;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
            }
            else if (toMaterial is Timber)
            {
                Timber material = toMaterial as Timber;
                material.YoungsModulus = youngsModulus;
                material.ThermalExpansionCoeff = thermalExpansionCoeff;
                material.PoissonsRatio = poissonsRatio;
            }
            else if (toMaterial is GenericIsotropicMaterial)
            {
                GenericIsotropicMaterial material = toMaterial as GenericIsotropicMaterial;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
            }
        }

        /***************************************************/
    }
}