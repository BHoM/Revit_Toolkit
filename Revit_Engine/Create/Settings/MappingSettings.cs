/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.oM.Adapters.Revit.Mapping;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        [Description("Creates an entity holding information about the enforced convert relationships between Revit families and BHoM types on Pull as well as mapping between Revit parameters and BHoM object properties.")]
        [InputFromProperty("parameterMaps")]
        [InputFromProperty("familyMaps")]
        [InputFromProperty("tagsParameter")]
        [InputFromProperty("materialGradeParameter")]
        [Output("mappingSettings")]
        public static MappingSettings MappingSettings(IEnumerable<ParameterMap> parameterMaps = null, IEnumerable<FamilyMap> familyMaps = null, string tagsParameter = "", string materialGradeParameter = "")
        {
            MappingSettings mappingSettings = new MappingSettings();
            if (parameterMaps != null)
                mappingSettings = mappingSettings.AddParameterMaps(parameterMaps);

            if (familyMaps != null)
                mappingSettings.FamilyMaps = familyMaps.ToList();

            mappingSettings.TagsParameter = tagsParameter;
            mappingSettings.MaterialGradeParameter = materialGradeParameter;

            return mappingSettings;
        }

        /***************************************************/

        [ToBeRemoved("4.3", "More up to date version with a larger number of parameters has been added.")]
        [Description("Created an entity holding information about conversion-specific Revit parameter names as well as relationships between object's property names (or names of RevitParameters attached to it) and Revit parameter names.")]
        [InputFromProperty("parameterMaps")]
        [InputFromProperty("tagsParameter")]
        [InputFromProperty("materialGradeParameter")]
        [Output("mappingSettings")]
        public static MappingSettings MappingSettings(IEnumerable<ParameterMap> parameterMaps = null, string tagsParameter = "", string materialGradeParameter = "")
        {
            MappingSettings mappingSettings = new MappingSettings();
            if (parameterMaps != null)
                mappingSettings = mappingSettings.AddParameterMaps(parameterMaps);

            mappingSettings.TagsParameter = tagsParameter;
            mappingSettings.MaterialGradeParameter = materialGradeParameter;

            return mappingSettings;
        }

        /***************************************************/
    }
}






