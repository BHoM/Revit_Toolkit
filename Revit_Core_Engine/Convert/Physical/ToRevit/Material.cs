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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using BHP = BH.oM.Physical.Materials;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts BH.oM.Physical.Materials.Material to a Revit Material.")]
        [Input("material", "BH.oM.Physical.Materials.Material to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "Revit Material resulting from converting the input BH.oM.Physical.Materials.Material.")]
        public static Material ToRevitMaterial(this BHP.Material material, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (material == null)
                return null;

            Material revitMaterial = refObjects.GetValue<Material>(document, material.BHoM_Guid);
            if (revitMaterial != null)
                return revitMaterial;

            settings = settings.DefaultIfNull();

            try
            {
                revitMaterial = document.GetElement(Material.Create(document, material.Name)) as Material;
            }
            catch
            {
                BH.Engine.Base.Compute.RecordError(String.Format("Revit material could not be created because a material with the same name already exists in the model or the name is incorrect. BHoM_Guid: {0}", material.BHoM_Guid));
                return null;
            }

            foreach (BHP.IMaterialProperties property in material.Properties)
            {
                revitMaterial.ICopyCharacteristics(property);
            }

            // Copy parameters from BHoM object to Revit element
            revitMaterial.CopyParameters(material, settings);

            refObjects.AddOrReplace(material, revitMaterial);
            return revitMaterial;
        }

        /***************************************************/
    }
}





