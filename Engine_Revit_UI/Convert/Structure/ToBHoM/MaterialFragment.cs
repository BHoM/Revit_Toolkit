/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System;
using BH.oM.Structure.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Structure.MaterialFragments.IMaterialFragment ToBHoMMaterialFragment(this Autodesk.Revit.DB.Material material, PullSettings pullSettings = null, string materialGrade = null)
        {
            if (material == null)
            {
                //TODO: add more sensible null!
                //Compute.NullObjectWarning();
                return null;
            }

            pullSettings = pullSettings.DefaultIfNull();
            
            StructuralMaterialType structuralMaterialType = Query.StructuralMaterialType(material.MaterialClass);
            IMaterialFragment materialFragment = Query.LibraryMaterial(structuralMaterialType, materialGrade);
            if (materialFragment != null)
                return materialFragment;

            Compute.MaterialNotInLibraryNote(material);
            
            switch (structuralMaterialType)
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

        public static oM.Structure.MaterialFragments.IMaterialFragment BHoMEmptyMaterialFragment(this Autodesk.Revit.DB.Structure.StructuralMaterialType structuralMaterialType, PullSettings pullSettings = null)
        {
            string name;
            if (structuralMaterialType == StructuralMaterialType.Undefined)
                name = "Unknown Material";
            else
                name = String.Format("Unknown {0} Material", structuralMaterialType);

            switch (structuralMaterialType)
            {
                case StructuralMaterialType.Aluminum:
                    {
                        return new Aluminium() { Name = name };
                    }
                case StructuralMaterialType.Concrete:
                case StructuralMaterialType.PrecastConcrete:
                    {
                        return new Concrete() { Name = name };
                    }
                case StructuralMaterialType.Wood:
                    {
                        return new Timber() { Name = name };
                    }
                default:
                    {
                        return new Steel() { Name = name };
                    }
            }
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static IMaterialFragment Update(this IMaterialFragment materialDestination, Autodesk.Revit.DB.Material materialSource)
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
    }
}
