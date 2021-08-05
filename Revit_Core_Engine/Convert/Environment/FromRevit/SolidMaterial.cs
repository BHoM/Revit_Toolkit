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
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Environment.MaterialFragments;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Material to BH.oM.Environment.MaterialFragments.SolidMaterial.")]
        [Input("material", "Revit Material to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "BH.oM.Environment.MaterialFragments.SolidMaterial resulting from converting the input Revit Material.")]
        public static SolidMaterial SolidMaterialFromRevit(this Material material, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (material == null)
                return null;

            settings = settings.DefaultIfNull();

            SolidMaterial result = refObjects.GetValue<SolidMaterial>(material.Id.IntegerValue);
            if (result != null)
                return result;
            else
                result = new SolidMaterial();

            result.Name = material.Name;
            Parameter parameter = material.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION);
            if (parameter != null)
                result.Description = parameter.AsString();

            result.CopyCharacteristics(material);
            result.SetProperties(material, settings.MappingSettings);

            refObjects.AddOrReplace(material.Id, result);
            return result;
        }
        
        /***************************************************/
    }
}

