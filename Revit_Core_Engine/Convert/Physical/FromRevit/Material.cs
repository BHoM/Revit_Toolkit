/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit Material to BH.oM.Physical.Materials.Material.")]
        [Input("revitMaterial", "Revit Material to be converted.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "BH.oM.Physical.Materials.Material resulting from converting the input Revit Material.")]
        public static Material MaterialFromRevit(this Autodesk.Revit.DB.Material revitMaterial, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            return revitMaterial.MaterialFromRevit(null, settings, refObjects);
        }

        /***************************************************/

        [Description("Converts a Revit Material to BH.oM.Physical.Materials.Material.")]
        [Input("revitMaterial", "Revit Material to be converted.")]
        [Input("grade", "Material grade extracted from the Revit element parent to the given Material, to be applied to the resultant BHoM Material.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("material", "BH.oM.Physical.Materials.Material resulting from converting the input Revit Material.")]
        public static Material MaterialFromRevit(this Autodesk.Revit.DB.Material revitMaterial, string grade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (revitMaterial == null)
                return new Material { Name = "Unknown Material" };

            settings = settings.DefaultIfNull();

            string refId = revitMaterial.Id.ReferenceIdentifier(grade);
            Material material = refObjects.GetValue<Material>(refId);
            if (material != null)
                return material;

            material = new Material { Properties = revitMaterial.MaterialProperties(grade, settings, refObjects), Name =  revitMaterial.Name};

            //Set identifiers, parameters & custom data
            material.SetIdentifiers(revitMaterial);
            material.CopyParameters(revitMaterial, settings.MappingSettings);
            material.SetProperties(revitMaterial, settings.MappingSettings);
            
            refObjects.AddOrReplace(refId, material);
            return material;
        }

        /***************************************************/
    }
}



