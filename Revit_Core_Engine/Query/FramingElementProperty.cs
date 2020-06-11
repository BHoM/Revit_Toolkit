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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Physical.FramingProperties;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static IFramingElementProperty FramingElementProperty(this FamilyInstance familyInstance, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null || familyInstance.Symbol == null)
                return null;

            // Check if an instance or type Structural Material parameter exists.
            ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
            if (structuralMaterialId.IntegerValue < 0)
                structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

            Material revitMaterial = familyInstance.Document.GetElement(structuralMaterialId) as Material;
            BH.oM.Physical.Materials.Material material = refObjects.GetValue<oM.Physical.Materials.Material>(structuralMaterialId.IntegerValue);

            if (material == null)
                material = revitMaterial.EmptyMaterialFromRevit(settings, refObjects);

            string materialGrade = familyInstance.MaterialGrade(settings);
            material = material.UpdateMaterialProperties(revitMaterial, materialGrade, familyInstance.StructuralMaterialType, settings);
            
            IProfile profile = familyInstance.Symbol.ProfileFromRevit(settings, refObjects);
            if (profile == null)
                familyInstance.Symbol.NotConvertedWarning();

            //TODO: check category of familyInstance to recognize which rotation query to use
            double rotation = familyInstance.AdjustedRotation(settings);
            
            return BH.Engine.Physical.Create.ConstantFramingProperty(profile, material, rotation, familyInstance.Symbol.Name);
        }

        /***************************************************/
    }
}
