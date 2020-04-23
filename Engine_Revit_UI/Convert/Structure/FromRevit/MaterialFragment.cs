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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Structure.MaterialFragments;
using System;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static IMaterialFragment MaterialFragmentFromRevit(this Material material, string materialGrade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (material == null)
            {
                //TODO: add more sensible null!
                //Compute.NullObjectWarning();
                return null;
            }

            settings = settings.DefaultIfNull();

            StructuralMaterialType structuralMaterialType = material.MaterialClass.StructuralMaterialType();
            IMaterialFragment materialFragment = structuralMaterialType.LibraryMaterial(materialGrade);
            if (materialFragment != null)
                return materialFragment;

            Compute.MaterialNotInLibraryNote(material);
            
            switch (structuralMaterialType)
            {
                case StructuralMaterialType.Concrete:
                case StructuralMaterialType.PrecastConcrete:
                    materialFragment = new Concrete();
                    break;
                case StructuralMaterialType.Aluminum:
                    materialFragment = new Aluminium();
                    break;
                case StructuralMaterialType.Steel:
                    materialFragment = new Steel();
                    break;
                case StructuralMaterialType.Wood:
                    materialFragment = new Timber();
                    break;
                default:
                    //TODO: default generic material warning
                    materialFragment = new GenericIsotropicMaterial();
                    break;
            }

            //TODO: zero strength warning + raise an issue
            materialFragment = materialFragment.Update(material);

            materialFragment.Name = material.Name;
            return materialFragment;
        }


        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static IMaterialFragment Update(this IMaterialFragment materialDestination, Material materialSource)
        {
            if (materialSource == null)
            {
                Compute.NullRevitElementWarning(materialDestination);
                return materialDestination;
            }
                
            ElementId elementID = materialSource.StructuralAssetId;
            if (elementID == null || elementID == ElementId.InvalidElementId)
            {
                Compute.NullStructuralAssetWarning(materialDestination);
                return materialDestination;
            }

            Document document = materialSource.Document;

            PropertySetElement propertySetElement = document.GetElement(elementID) as PropertySetElement;
            StructuralAsset structuralAsset = propertySetElement.GetStructuralAsset();
            if (structuralAsset == null)
            {
                Compute.NullStructuralAssetWarning(materialDestination);
                return materialDestination;
            }

            return materialDestination.Update(structuralAsset);
        }

        /***************************************************/

        private static IMaterialFragment Update(this IMaterialFragment materialFragment, StructuralAsset structuralAsset)
        {
            double density = structuralAsset.Density.ToSI(UnitType.UT_MassDensity);

#if REVIT2020

#else
            double dampingRatio = structuralAsset.DampingRatio;
#endif

            oM.Geometry.Vector youngsModulus = BH.Engine.Geometry.Create.Vector(structuralAsset.YoungModulus.X.ToSI(UnitType.UT_Stress), structuralAsset.YoungModulus.Y.ToSI(UnitType.UT_Stress), structuralAsset.YoungModulus.Z.ToSI(UnitType.UT_Stress));
            oM.Geometry.Vector thermalExpansionCoeff = BH.Engine.Geometry.Create.Vector(structuralAsset.ThermalExpansionCoefficient.X.ToSI(UnitType.UT_ThermalExpansion), structuralAsset.ThermalExpansionCoefficient.Y.ToSI(UnitType.UT_ThermalExpansion), structuralAsset.ThermalExpansionCoefficient.Z.ToSI(UnitType.UT_ThermalExpansion));
            oM.Geometry.Vector poissonsRatio = BH.Engine.Geometry.Create.Vector(structuralAsset.PoissonRatio.X, structuralAsset.PoissonRatio.Y, structuralAsset.PoissonRatio.Z);
            oM.Geometry.Vector shearModulus = BH.Engine.Geometry.Create.Vector(structuralAsset.ShearModulus.X.ToSI(UnitType.UT_Stress), structuralAsset.ShearModulus.Y.ToSI(UnitType.UT_Stress), structuralAsset.ShearModulus.Z.ToSI(UnitType.UT_Stress));

            materialFragment.Density = density;
#if REVIT2020

#else
            materialFragment.DampingRatio = dampingRatio;
#endif


            if (materialFragment is Aluminium)
            {
                Aluminium material = materialFragment as Aluminium;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                materialFragment = material;
            }
            else if (materialFragment is Concrete)
            {
                Concrete material = materialFragment as Concrete;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                materialFragment = material;
            }
            else if (materialFragment is Steel)
            {
                Steel material = materialFragment as Steel;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                materialFragment = material;
            }
            else if (materialFragment is Timber)
            {
                Timber material = materialFragment as Timber;
                material.YoungsModulus = youngsModulus;
                material.ThermalExpansionCoeff = thermalExpansionCoeff;
                material.PoissonsRatio = poissonsRatio;
                materialFragment = material;
            }
            else if (materialFragment is GenericIsotropicMaterial)
            {
                GenericIsotropicMaterial material = materialFragment as GenericIsotropicMaterial;
                material.YoungsModulus = youngsModulus.X;
                material.ThermalExpansionCoeff = thermalExpansionCoeff.X;
                material.PoissonsRatio = poissonsRatio.X;
                materialFragment = material;
            }

            return materialFragment;
        }

        /***************************************************/
    }
}

