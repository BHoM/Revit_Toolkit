/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a Revit parameter Definition based on the given properties.")]
        [Input("document", "Revit document, in which the new parameter Definition will be created.")]
        [Input("parameterName", "Name of the created parameter.")]
        [Input("typeName", "Name of the type of the created parameter. One of the UI dropdown values, e.g. Length, Area, Currency etc.")]
        [Input("groupName", "Name of the parameter group, to which the created parameter belongs. One of the UI dropdown values for project parameters (e.g. Dimensions), any value for shared parameter.")]
        [Input("instance", "If true, the created parameter will be an instance parameter, otherwise it will be a type parameter.")]
        [Input("categoryNames", "Categories, to which the created parameter is bound. It will get bound to all categories if this value is null.")]
        [Input("shared", "If true, the created parameter will be a shared parameter, otherwise it will be a project parameter.")]
        [Input("discipline", "Name of the Revit discipline, to which the created parameter belongs. One of the UI dropdown values (Common/Structural/HVAC/Electrical/Piping/Energy).\n" +
                             "Only relevant in case if there is more than one types with the same name (e.g. piping, HVAC and structural Velocity).")]
        [Output("definition", "Revit parameter Definition created based on the input properties.")]
        public static Definition Parameter(Document document, string parameterName, string typeName, string groupName, bool instance, IEnumerable<string> categoryNames, bool shared, string discipline = "")
        {
            List<ForgeTypeId> parameterTypes = new List<ForgeTypeId>();
            ForgeTypeId parameterType = typeof(SpecTypeId).GetProperties().FirstOrDefault(x => x.Name == typeName)?.GetValue(null) as ForgeTypeId;
            if (parameterType == null)
            {
                BH.Engine.Base.Compute.RecordError($"Parameter type named {typeName} does not exist.");
                return null;
            }

            List<string> distinctCategoryNames = categoryNames.Distinct().ToList();
            List<Category> categories = new List<Category>();
            foreach (Category cat in document.Settings.Categories)
            {
                if (distinctCategoryNames.Any(x => x == cat.Name))
                    categories.Add(cat);
            }

            if (distinctCategoryNames.Count != categories.Count)
            {
                BH.Engine.Base.Compute.RecordError("Parameter could not be created due to the following categories do not exist in the active document: " +
                                                          string.Join(", ", distinctCategoryNames.Except(categories.Select(x => x.Name))) + ".");
                return null;
            }

            if (shared)
                return Create.SharedParameter(document, parameterName, parameterType, groupName, instance, categories);
            else
            {
#if REVIT2023 || REVIT2024
                BuiltInParameterGroup parameterGroup = groupName.GroupFromName();
                if (parameterGroup == BuiltInParameterGroup.INVALID)
#else
                ForgeTypeId parameterGroup = groupName.GroupFromName();
                if (parameterGroup == null)
#endif
                {
                    BH.Engine.Base.Compute.RecordError($"Parameter group named {groupName} does not exist.");
                    return null;
                }

                return Create.ProjectParameter(document, parameterName, parameterType, parameterGroup, instance, categories);
            }
        }

        /***************************************************/
    }
}







