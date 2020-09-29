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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using System;
using System.Collections.Generic;
using BHP = BH.oM.Physical.Materials;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

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
                BH.Engine.Reflection.Compute.RecordError(String.Format("Revit material could not be created because a material with the same name already exists in the model or the name is incorrect. BHoM_Guid: {0}", material.BHoM_Guid));
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
