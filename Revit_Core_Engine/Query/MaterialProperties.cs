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
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Extracts the material properties from a given Revit material and converts them to BHoM.")]
        [Input("material", "Revit material to extract the properties from.")]
        [Input("materialGrade", "Material grade value to be applied to the extracted material property objects.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("properties", "Properties extracted from the input Revit material and converted to BHoM.")]
        public static List<IMaterialProperties> MaterialProperties(this Autodesk.Revit.DB.Material material, string materialGrade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            // Add material fragment per discipline.
            List<IMaterialProperties> result = new List<IMaterialProperties>();
            result.Add(material.SolidMaterialFromRevit(settings, refObjects));
            result.Add(material.MaterialFragmentFromRevit(materialGrade, settings, refObjects));

            return result.Where(x => x != null).ToList();
        }

        /***************************************************/
    }
}



