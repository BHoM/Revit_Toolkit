/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts a BHoM representation of material from Revit FamilyInstance representing a framing element.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "BHoM representation of material extracted from Revit FamilyInstance representing a framing element.")]
        public static BH.oM.Physical.Materials.Material FramingMaterial(this FamilyInstance familyInstance, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Look up the material in structural material parameters
            ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
            if (structuralMaterialId.IntegerValue < 0)
                structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

            // If not found under structural material parameters, check if the solid representation has a consistent material assigned - if so, use it
            if (structuralMaterialId.IntegerValue < 0)
            {
                Options options = new Options();
                options.DetailLevel = Autodesk.Revit.DB.ViewDetailLevel.Coarse;
                List<Autodesk.Revit.DB.Face> faces = familyInstance.Faces(options);
                if (faces != null)
                {
                    IEnumerable<ElementId> materialIdsFromFaces = faces.Where(x => x.MaterialElementId != null).Select(x => x.MaterialElementId);
                    List<ElementId> uniqueMaterialIds = materialIdsFromFaces.Distinct().ToList();
                    if (uniqueMaterialIds.Count == 1)
                        structuralMaterialId = uniqueMaterialIds[0];
                }
            }

            // Convert the Revit material to BHoM
            Material revitMaterial = familyInstance.Document.GetElement(structuralMaterialId) as Material;
            if (revitMaterial == null)
                revitMaterial = familyInstance.Category.Material;

            string materialGrade = familyInstance.MaterialGrade(settings);
            BH.oM.Physical.Materials.Material material = revitMaterial.MaterialFromRevit(materialGrade, settings, refObjects);

            // If Revit material is null, rename the BHoM material based on material type of framing family.
            if (material != null && revitMaterial == null)
            {
                material.Name = $"Unknown {familyInstance.StructuralMaterialType} Material";
                material.Properties.Add(familyInstance.StructuralMaterialType.EmptyMaterialFragment(materialGrade));
            }

            return material;
        }

        /***************************************************/
    }
}


