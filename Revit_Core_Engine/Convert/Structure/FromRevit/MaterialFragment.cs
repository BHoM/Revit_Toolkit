/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.MaterialFragments;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Material to BH.oM.Structure.MaterialFragments.IMaterialFragment.")]
        [Input("material", "Revit Material to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "BH.oM.Structure.MaterialFragments.IMaterialFragment resulting from converting the input Revit Material.")]
        public static IMaterialFragment MaterialFragmentFromRevit(this Material material, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return material.MaterialFragmentFromRevit(null, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit Material to BH.oM.Structure.MaterialFragments.IMaterialFragment.")]
        [Input("material", "Revit Material to be converted.")]
        [Input("grade", "Material grade extracted from the Revit element parent to the given Material, to be applied to the resultant BHoM IMaterialFragment.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "BH.oM.Structure.MaterialFragments.IMaterialFragment resulting from converting the input Revit Material.")]
        public static IMaterialFragment MaterialFragmentFromRevit(this Material material, string grade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (material == null)
                return null;

            string refId = material.Id.ReferenceIdentifier(grade);
            IMaterialFragment materialFragment = refObjects.GetValue<IMaterialFragment>(refId);
            if (materialFragment != null)
                return materialFragment;

            settings = settings.DefaultIfNull();

            StructuralMaterialType structuralMaterialType = material.MaterialClass.StructuralMaterialType();
            materialFragment = structuralMaterialType.LibraryMaterial(grade);
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
                    BH.Engine.Reflection.Compute.RecordWarning(String.Format("Revit material of structural type {0} is currently not supported, the material was converted to a generic isotropic BHoM material. Revit ElementId: {1}", structuralMaterialType, material.Id.IntegerValue));
                    materialFragment = new GenericIsotropicMaterial();
                    break;
            }
            
            materialFragment.CopyCharacteristics(material);

            string name = material.Name;
            if (!string.IsNullOrWhiteSpace(grade))
                name += " grade " + grade;

            materialFragment.Name = name;

            refObjects.AddOrReplace(refId, materialFragment);
            return materialFragment;
        }
        
        /***************************************************/
    }
}


