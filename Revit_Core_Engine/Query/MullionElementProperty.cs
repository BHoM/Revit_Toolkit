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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Facade.SectionProperties;
using BH.oM.Physical.FramingProperties;
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
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

        [PreviousVersion("6.3", "BH.Revit.Engine.Core.Query.MullionElementProperty(Autodesk.Revit.DB.FamilyInstance, BH.oM.Adapters.Revit.Settings.RevitSettings, System.Collections.Generic.Dictionary<System.String, System.Collections.Generic.List<BH.oM.Base.IBHoMObject>>)")]
        [Description("Extracts the mullion element property from a Revit Mullion.")]
        [Input("mullion", "Revit Mullion to be queried.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("property", "BHoM mullion element property extracted from the input Revit Mullion.")]
        public static FrameEdgeProperty MullionElementProperty(this Mullion mullion, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (mullion?.Symbol == null)
                return null;
            
            FrameEdgeProperty frameEdgeProperty = refObjects.GetValue<FrameEdgeProperty>(mullion.Id);
            if (frameEdgeProperty != null)
                return frameEdgeProperty;

            // Convert the material to BHoM
            BH.oM.Physical.Materials.Material material = mullion.FramingMaterial(settings, refObjects);

            // Convert the profile to BHoM
            IProfile profile = mullion.MullionType.ProfileFromRevit(settings, refObjects);

            if (profile == null)
                BH.Engine.Base.Compute.RecordWarning($"Mullion profile could not be extracted. ElementId: {mullion.Id.IntegerValue}");

            List<ConstantFramingProperty> sectionProperties = new List<ConstantFramingProperty> { BH.Engine.Physical.Create.ConstantFramingProperty(profile, material, 0, mullion.Symbol.Name) };
            frameEdgeProperty = new FrameEdgeProperty { Name = mullion.Symbol.Name, SectionProperties = sectionProperties };

            //Set identifiers, parameters & custom data
            frameEdgeProperty.SetIdentifiers(mullion.Symbol);
            frameEdgeProperty.CopyParameters(mullion.Symbol, settings.MappingSettings);
            frameEdgeProperty.SetProperties(mullion.Symbol, settings.MappingSettings);

            refObjects.AddOrReplace(mullion.Id, frameEdgeProperty);
            return frameEdgeProperty;
        }

        /***************************************************/
    }
}



