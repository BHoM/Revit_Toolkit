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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        static public oM.Physical.Constructions.Layer Layer(this CompoundStructureLayer compoundStructureLayer, Document document, BuiltInCategory builtInCategory = Autodesk.Revit.DB.BuiltInCategory.INVALID, PullSettings pullSettings = null)
        {
            if (compoundStructureLayer == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            oM.Physical.Constructions.Layer aLayer = new oM.Physical.Constructions.Layer();
            oM.Physical.Materials.Material aMaterial = new oM.Physical.Materials.Material();

            double aThickness = compoundStructureLayer.Width;
            if (pullSettings.ConvertUnits)
                aThickness = UnitUtils.ConvertFromInternalUnits(aThickness, DisplayUnitType.DUT_METERS);

            aLayer.Thickness = aThickness;

            ElementId aElementId = compoundStructureLayer.MaterialId;
            Material aMaterial_Revit = null;
            if(aElementId != null && aElementId != Autodesk.Revit.DB.ElementId.InvalidElementId)
                aMaterial_Revit = document.GetElement(aElementId) as Material;

            if (aMaterial_Revit == null && builtInCategory != Autodesk.Revit.DB.BuiltInCategory.INVALID)
            {
                Category aCategory = document.Settings.Categories.get_Item(builtInCategory);
                if (aCategory != null)
                    aMaterial_Revit = aCategory.Material;
            }

            if (aMaterial_Revit == null)
            {
                Compute.MaterialNotFoundWarning(aMaterial);
                return aLayer;
            } 

            /*switch(aMaterial_Revit.MaterialClass)
            {
                case "Aluminium":
                    aMaterial.MaterialType = oM.Environment.Elements.MaterialType.Opaque;
                    break;
                case "Concrete":
                    aMaterial.MaterialType = oM.Environment.Elements.MaterialType.Opaque;
                    break;
                case "Steel":
                    aMaterial.MaterialType = oM.Environment.Elements.MaterialType.Opaque;
                    break;
                case "Metal":
                    aMaterial.MaterialType = oM.Environment.Elements.MaterialType.Opaque;
                    break;
                case "Wood":
                    aMaterial.MaterialType = oM.Environment.Elements.MaterialType.Opaque;
                    break;
            }*/

            aMaterial.Name = aMaterial_Revit.Name;

            IEnvironmentMaterial aMaterialProperties = Convert.ToBHoM(aMaterial_Revit, pullSettings) as IEnvironmentMaterial;

            aMaterial.Properties.Add(aMaterialProperties);

            aLayer.Material = aMaterial;
            return aLayer;
        }

        /***************************************************/
    }
}