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
using System.Collections.Generic;
using System.Linq;
using System;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.MaterialFragments;
using BH.oM.Physical.Materials;
using BH.oM.Structure.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Physical.Materials.Material ToBHoMEmptyMaterial(this Autodesk.Revit.DB.Material material, PullSettings pullSettings = null)
        {
            if (material == null)
                return new oM.Physical.Materials.Material { Name = "Unknown Material" };

            pullSettings = pullSettings.DefaultIfNull();

            oM.Physical.Materials.Material aMaterial = pullSettings.FindRefObject<oM.Physical.Materials.Material>(material.Id.IntegerValue);
            if (aMaterial != null)
                return aMaterial;

            aMaterial = new oM.Physical.Materials.Material { Properties = new System.Collections.Generic.List<IMaterialProperties>(), Name = material.Name };
            
            aMaterial = Modify.SetIdentifiers(aMaterial, material) as oM.Physical.Materials.Material;
            if (pullSettings.CopyCustomData)
                aMaterial = Modify.SetCustomData(aMaterial, material, pullSettings.ConvertUnits) as oM.Physical.Materials.Material;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aMaterial);

            return aMaterial;
        }

        /***************************************************/
    }
}