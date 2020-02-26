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

using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.Materials;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static Material ToBHoMEmptyMaterial(this Autodesk.Revit.DB.Material revitMaterial, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (revitMaterial == null)
                return new Material { Name = "Unknown Material" };

            settings = settings.DefaultIfNull();

            Material material = refObjects.GetValue<Material>(revitMaterial.Id);
            if (material != null)
                return material;

            material = new Material { Properties = new List<IMaterialProperties>(), Name = revitMaterial.Name };

            //Set identifiers & custom data
            material.SetIdentifiers(revitMaterial);
            material.SetCustomData(revitMaterial);

            refObjects.AddOrReplace(revitMaterial.Id, material);
            return material;
        }

        /***************************************************/
    }
}
