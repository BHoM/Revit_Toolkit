/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts a Revit CompoundStructureLayer to BH.oM.Physical.Constructions.Layer.")]
        [Input("compoundStructureLayer", "Revit CompoundStructureLayer to be converted.")]
        [Input("owner", "Revit HostObjAttributes that represents the layered construction, to which the given layer belongs.")]
        [Input("materialGrade", "Material grade extracted from the Revit element parent to the given CompoundStructureLayer, to be applied to the resultant BHoM Layer.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("layer", "Physical.Constructions.Layer resulting from converting the input Revit CompoundStructureLayer.")]
        public static oM.Physical.Constructions.Layer LayerFromRevit(this CompoundStructureLayer compoundStructureLayer, HostObjAttributes owner, string materialGrade = null, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (compoundStructureLayer == null)
                return null;

            settings = settings.DefaultIfNull();

            oM.Physical.Constructions.Layer layer = new oM.Physical.Constructions.Layer();
            layer.Thickness = compoundStructureLayer.Width.ToSI(SpecTypeId.Length);

            if (owner != null)
            {
                Material revitMaterial = owner.Document.GetElement(compoundStructureLayer.MaterialId) as Material;
                if (revitMaterial == null)
                    revitMaterial = owner.Category.Material;

                layer.Material = revitMaterial.MaterialFromRevit(materialGrade, settings, refObjects);

                if (compoundStructureLayer.DeckProfileId.IntegerValue != -1)
                    BH.Engine.Reflection.Compute.RecordWarning("Conversion of Revit composite deck layers is not supported - it has been converted into a zero thickness BHoM layer instead.");

                if (compoundStructureLayer.LayerId == owner.GetCompoundStructure().VariableLayerIndex)
                    BH.Engine.Reflection.Compute.RecordWarning("Conversion of Revit layers with variable thickness is not supported - it has been converted into a constant thickness BHoM layer instead.");
            }
            else
                BH.Engine.Reflection.Compute.RecordError("Material and variable thickness information could not be determined for the layer due to lack of owner construction information.");

            return layer;
        }

        /***************************************************/
    }
}


