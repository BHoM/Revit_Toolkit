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

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.Properties;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static MaterialPropertiesOpaque ToBHoMMaterialPropertiesOpaque(this Material material, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            MaterialPropertiesOpaque aResult = pullSettings.FindRefObject<MaterialPropertiesOpaque>(material.Id.IntegerValue);
            if (aResult != null)
                return aResult;

            aResult = new MaterialPropertiesOpaque();
            aResult.Name = material.Name;
            Parameter aParameter = material.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION);
            if (aParameter != null)
                aResult.Description = aParameter.AsString();

            Update(aResult, material, pullSettings);

            //Set custom data
            aResult = Modify.SetIdentifiers(aResult, material) as MaterialPropertiesOpaque;
            if (pullSettings.CopyCustomData)
                aResult = Modify.SetCustomData(aResult, material, pullSettings.ConvertUnits) as MaterialPropertiesOpaque;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aResult);
            return aResult;
        }

        /***************************************************/

        /***************************************************/
        /****             Private methods               ****/
        /***************************************************/

        private static void Update(this MaterialPropertiesOpaque materialPropertiesOpaque, Material material, PullSettings pullSettings = null)
        {
            if (material == null)
            {
                Compute.NullRevitElementWarning(materialPropertiesOpaque);
                return;
            }

            ElementId aElementId = material.ThermalAssetId;
            if (aElementId == null || aElementId == ElementId.InvalidElementId)
            {
                Compute.NullThermalAssetWarning(materialPropertiesOpaque);
                return;
            }

            Document aDocument = material.Document;

            PropertySetElement aPropertySetElement = aDocument.GetElement(aElementId) as PropertySetElement;
            materialPropertiesOpaque.Update(aPropertySetElement.GetThermalAsset(), pullSettings);
        }

        /***************************************************/

        private static void Update(this MaterialPropertiesOpaque materialPropertiesOpaque, StructuralAsset structuralAsset, PullSettings pullSettings = null)
        {

        }

        /***************************************************/

        private static void Update(this MaterialPropertiesOpaque materialPropertiesOpaque, ThermalAsset thermalAsset, PullSettings pullSettings = null)
        {
            materialPropertiesOpaque.Density = thermalAsset.Density;
            if (pullSettings.ConvertUnits)
                materialPropertiesOpaque.Density = UnitUtils.ConvertFromInternalUnits(materialPropertiesOpaque.Density, DisplayUnitType.DUT_KILOGRAMS_PER_CUBIC_METER);

            materialPropertiesOpaque.Conductivity = thermalAsset.ThermalConductivity;
            materialPropertiesOpaque.SpecificHeat = thermalAsset.SpecificHeat;
        }

        /***************************************************/
    }
}