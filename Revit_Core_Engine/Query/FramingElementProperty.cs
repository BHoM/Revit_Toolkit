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
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Physical.FramingProperties;
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the framing element property from a Revit FamilyInstance.")]
        [Input("familyInstance", "Revit FamilyInstance to be queried.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("property", "BHoM framing element property extracted from the input Revit FamilyInstance.")]
        public static IFramingElementProperty FramingElementProperty(this FamilyInstance familyInstance, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (familyInstance == null || familyInstance.Symbol == null)
                return null;
            
            ConstantFramingProperty framingProperty = refObjects.GetValue<ConstantFramingProperty>(familyInstance.Id);
            if (framingProperty != null)
                return framingProperty;

            // Convert the material to BHoM.
            ElementId structuralMaterialId = familyInstance.StructuralMaterialId;
            if (structuralMaterialId.IntegerValue < 0)
                structuralMaterialId = familyInstance.Symbol.LookupParameterElementId(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);

            Material revitMaterial = familyInstance.Document.GetElement(structuralMaterialId) as Material;
            if (revitMaterial == null)
                revitMaterial = familyInstance.Category.Material;

            string materialGrade = familyInstance.MaterialGrade(settings);
            BH.oM.Physical.Materials.Material material = revitMaterial.MaterialFromRevit(materialGrade, settings, refObjects);

            // If Revit material is null, rename the BHoM material based on material type of framing family.
            if (material != null && revitMaterial == null)
            {
                material.Name = String.Format("Unknown {0} Material", familyInstance.StructuralMaterialType);
                material.Properties.Add(familyInstance.StructuralMaterialType.EmptyMaterialFragment(materialGrade));
            }

            IProfile profile = familyInstance.Symbol.ProfileFromRevit(settings, refObjects);
            if (profile == null)
                familyInstance.Symbol.NotConvertedWarning();

            if (familyInstance.Mirrored)
            {
                if (profile is FreeFormProfile)
                {
                    BH.oM.Geometry.Plane mirror = new oM.Geometry.Plane { Normal = BH.oM.Geometry.Vector.XAxis };
                    profile = BH.Engine.Spatial.Create.FreeFormProfile(profile.Edges.Select(x => x.IMirror(mirror)));
                }
                else if (profile is AngleProfile)
                {
                    AngleProfile angle = ((AngleProfile)profile);
                    profile = BH.Engine.Spatial.Create.AngleProfile(angle.Height, angle.Width, angle.WebThickness, angle.FlangeThickness, angle.RootRadius, angle.ToeRadius, true, false);
                }
                else if (profile is ChannelProfile)
                {
                    ChannelProfile channel = ((ChannelProfile)profile);
                    profile = BH.Engine.Spatial.Create.ChannelProfile(channel.Height, channel.FlangeWidth, channel.WebThickness, channel.FlangeThickness, channel.RootRadius, channel.ToeRadius, true);
                }
            }
            
            double rotation = familyInstance.OrientationAngle(settings);
            
            framingProperty = BH.Engine.Physical.Create.ConstantFramingProperty(profile, material, rotation, familyInstance.Symbol.Name);

            //Set identifiers, parameters & custom data
            framingProperty.SetIdentifiers(familyInstance.Symbol);
            framingProperty.CopyParameters(familyInstance.Symbol, settings.MappingSettings);
            framingProperty.SetProperties(familyInstance.Symbol, settings.MappingSettings);

            refObjects.AddOrReplace(familyInstance.Id, framingProperty);
            return framingProperty;
        }

        /***************************************************/
    }
}


