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
using BH.oM.Environment.MaterialFragments;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static SolidMaterial ToBHoMSolidMaterial(this Material material, PullSettings pullSettings = null)
        {
            if (material == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            SolidMaterial result = pullSettings.FindRefObject<SolidMaterial>(material.Id.IntegerValue);
            if (result != null)
                return result;
            else
                result = new SolidMaterial();

            result.Name = material.Name;
            Parameter parameter = material.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION);
            if (parameter != null)
                result.Description = parameter.AsString();

            Update(result, material, pullSettings);

            result = result.UpdateValues(pullSettings, material) as SolidMaterial;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(result);
            return result;
        }

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static void Update(this SolidMaterial solidMaterial, Material material, PullSettings pullSettings = null)
        {
            if (material == null)
            {
                Compute.NullRevitElementWarning(solidMaterial);
                return;
            }

            ElementId elementID = material.ThermalAssetId;
            if (elementID == null || elementID == ElementId.InvalidElementId)
            {
                Compute.NullThermalAssetWarning(solidMaterial);
                return;
            }

            Document document = material.Document;

            PropertySetElement propertySetElement = document.GetElement(elementID) as PropertySetElement;
            solidMaterial.Update(propertySetElement.GetThermalAsset(), pullSettings);
        }

        /***************************************************/

        private static void Update(this SolidMaterial solidMaterial, StructuralAsset structuralAsset, PullSettings pullSettings = null)
        {

        }

        /***************************************************/

        private static void Update(this SolidMaterial solidMaterial, ThermalAsset thermalAsset, PullSettings pullSettings = null)
        {
            solidMaterial.Conductivity = thermalAsset.ThermalConductivity;
            solidMaterial.SpecificHeat = thermalAsset.SpecificHeat;
            solidMaterial.Density = thermalAsset.Density.ToSI(UnitType.UT_MassDensity);
        }

        /***************************************************/
    }
}
