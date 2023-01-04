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

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.MaterialFragments;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the construction of the given Revit energy analysis opening.")]
        [Input("energyAnalysisOpening", "Revit energy analysis opening to extract the construction from.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Output("construction", "Construction of the input Revit energy analysis opening.")]
        public static oM.Physical.Constructions.Construction Construction(this EnergyAnalysisOpening energyAnalysisOpening, RevitSettings settings = null)
        {
            if (energyAnalysisOpening == null)
                return null;

            Element element = energyAnalysisOpening.Element();
            if (element == null)
                return null;

            oM.Physical.Constructions.Construction construction = Query.Construction(element.FamilyTypeFullName(), energyAnalysisOpening.OpeningType.ToString());
            if (construction == null)
                return null;

            element.CopyTypeToFragment(construction);
            return construction;
        }


        /***************************************************/
        /****              Private methods              ****/
        /***************************************************/

        private static oM.Physical.Constructions.Construction Construction(string constructionName, string materialName)
        {
            if (string.IsNullOrEmpty(constructionName) || string.IsNullOrEmpty(materialName))
                return null;

            string matName = null;
            if (!string.IsNullOrEmpty(materialName))
                matName = string.Format("Default {0} Material", materialName);
            else
                matName = "Default Material";

            SolidMaterial transparentMaterialProperties = new SolidMaterial();
            transparentMaterialProperties.Name = matName;

            oM.Physical.Materials.Material material = new oM.Physical.Materials.Material();
            material.Properties.Add(transparentMaterialProperties);

            oM.Physical.Constructions.Construction construction = new oM.Physical.Constructions.Construction();
            construction.Name = constructionName;

            oM.Physical.Constructions.Layer layer = new oM.Physical.Constructions.Layer();
            layer.Material = material;

            construction.Layers.Add(layer);

            return construction;
        }

        /***************************************************/
    }
}

