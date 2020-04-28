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

using BH.oM.Adapters.Revit.Generic;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Environment.Fragments;
using BH.oM.Geometry.ShapeProfiles;
using BH.oM.Reflection.Attributes;
using BH.oM.Structure.MaterialFragments;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates an instance of MapSettings with default values.")]
        [Output("mapSettings")]
        public static ParameterSettings DefaultMapSettings()
        {
            List<ParameterMap> typeMaps = new List<ParameterMap>();

            ParameterMap typeMap = null;

            typeMap = Create.ParameterMap(typeof(PanelContextFragment));
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "IsAir", ParameterNames = new HashSet<string> { "IsAir", "BHE_IsAir", "SAM_BuildingElementAir" } });
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "Colour", ParameterNames = new HashSet<string> { "Colour", "BHE_Colour", "SAM_BuildingElementColour" } });
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "IsGround", ParameterNames = new HashSet<string> { "SAM_BuildingElementGround" } });
            typeMaps.Add(typeMap);

            typeMap = Create.ParameterMap(typeof(OriginContextFragment));
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "Description", ParameterNames = new HashSet<string> { "SAM_BuildingElementDescription" } });
            typeMaps.Add(typeMap);

            typeMap = Create.ParameterMap(typeof(PanelAnalyticalFragment));
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "UValue", ParameterNames = new HashSet<string> { "SAM_UValue" } });
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "GValue", ParameterNames = new HashSet<string> { "SAM_gValue" } });
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "LTValue", ParameterNames = new HashSet<string> { "SAM_LtValue" } });
            typeMaps.Add(typeMap);

            typeMap = Create.ParameterMap(typeof(BuildingAnalyticalFragment));
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "NorthAngle", ParameterNames = new HashSet<string> { "SAM_NorthAngle" } });
            typeMaps.Add(typeMap);

            typeMap = Create.ParameterMap(typeof(SpaceContextFragment));
            typeMap.ParameterLinks.Add(new ParameterLink { PropertyName = "IsExternal", ParameterNames = new HashSet<string> { "SAM_ExternalZone" } });
            typeMaps.Add(typeMap);

            ParameterSettings result = Create.ParameterSettings(typeMaps);
            result.Name = "BH Default Parameter Settings";
            result.MaterialGradeParameter = "BHE_Material Grade";
            result.TagsParameter = "BHE_Tags";
            return result;
        }

        /***************************************************/
    }
}

