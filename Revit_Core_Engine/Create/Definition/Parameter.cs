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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

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
#if (REVIT2018 || REVIT2019 || REVIT2020 || REVIT2021)
            List<ParameterType> parameterTypes = new List<ParameterType>();
            foreach (ParameterType pt in Enum.GetValues(typeof(ParameterType)))
            {
                try
                {
                    if (LabelUtils.GetLabelFor(pt) == typeName)
                        parameterTypes.Add(pt);
                }
                catch
                {

                }
            }
            
            ParameterType parameterType = ParameterType.Invalid;
            
            if (parameterTypes.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError($"Parameter type named {typeName} does not exist.");
                return null;
            }
            else if (parameterTypes.Count == 1)
                parameterType = parameterTypes[0];
            else
            {
                if (discipline == "Common")
                {
                    string[] forbidden = { "Electrical", "HVAC", "Piping", "Structural" };
                    parameterType = parameterTypes.FirstOrDefault(x => forbidden.All(y => !x.ToString().StartsWith(y)));
                }
                else if (!string.IsNullOrWhiteSpace(discipline))
                    parameterType = parameterTypes.FirstOrDefault(x => x.ToString().StartsWith(discipline));

                if (parameterType == ParameterType.Invalid)
                {
                    BH.Engine.Base.Compute.RecordError("The parameter type with given name exists in more than one discipline, therefore the parameter could not be created. To successfully create the parameter, please specify it using one of the following: HVAC, Piping, Electrical, Structural.");
                    return null;
                }
            }
#else
            List<ForgeTypeId> parameterTypes = new List<ForgeTypeId>();
            ForgeTypeId parameterType = typeof(SpecTypeId).GetProperties().FirstOrDefault(x => x.Name == parameterName).GetValue(null) as ForgeTypeId;
            if (parameterType == null)
            {
                BH.Engine.Base.Compute.RecordError($"Parameter type named {typeName} does not exist.");
                return null;
            }
#endif

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
                BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.INVALID;
                foreach (BuiltInParameterGroup bpg in System.Enum.GetValues(typeof(BuiltInParameterGroup)))
                {
                    if (LabelUtils.GetLabelFor(bpg) == groupName)
                    {
                        parameterGroup = bpg;
                        break;
                    }
                }

                if (parameterGroup == BuiltInParameterGroup.INVALID)
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



