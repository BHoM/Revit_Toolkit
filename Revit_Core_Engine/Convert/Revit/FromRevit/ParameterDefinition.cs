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

using Autodesk.Revit.DB;
using BH.Engine.Adapters.Revit;
using BH.oM.Adapters.Revit.Parameters;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/


        [Description("Converts a ParameterElement to BH.oM.Adapters.Revit.Parameters.ParameterDefinition.")]
        [Input("parameter", "Revit Parameter to be converted.")]
        [Input("parameterLinks", "A collection of names of RevitParameters and sets of their correspondent Revit parameter names to be used on name mapping.")]
        [Input("onlyLinked", "If true, there needs to be a valid, relevant parameter link in parameterLinks in order for convert to succeed.")]
        [Output("parameterDefinition", "BH.oM.Adapters.Revit.Parameters.ParameterDefinition resulting from converting the input Revit Parameter.")]
        public static ParameterDefinition ParameterDefinitionFromRevit(this ParameterElement parameter,Document document, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();
            ParameterDefinition paramDef = refObjects.GetValue<ParameterDefinition>(parameter.Id);
            if (paramDef != null)
                return paramDef;

            Definition def = parameter.GetDefinition();
            Binding binding = document.ParameterBindings.get_Item(def);
            ElementBinding elementBinding = binding as ElementBinding;

            string name = parameter.Name;

            string parameterType = def.GetDataType().TypeId;

            ForgeTypeId groupTypeId = def.GetGroupTypeId();
            string parameterGroup = LabelUtils.GetLabelForGroup(groupTypeId);

            bool instance = elementBinding is InstanceBinding;

            List<string> categories = new List<string>();
            if (elementBinding?.Categories != null)
            {
                categories = elementBinding.Categories.Cast<Category>().Select(c => c.Name).ToList();
            }

            bool shared = def is ExternalDefinition;

            paramDef = new ParameterDefinition
            {
                Name = name,
                ParameterType = parameterType,
                ParameterGroup = parameterGroup,
                Instance = instance,
                Categories = categories,
                Shared = shared
            };

            refObjects.AddOrReplace(parameter.Id, paramDef);
            return paramDef;
        }

        /***************************************************/
    }
}
