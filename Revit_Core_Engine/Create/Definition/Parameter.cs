/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Definition Parameter(Document document, string parameterName, string groupName, string typeName, string group, bool instance, IEnumerable<string> categoryNames, bool shared)
        {
            var parameterType = ParameterType.Text;
            if (!System.Enum.TryParse(typeName, out parameterType))
            {
                BH.Engine.Reflection.Compute.RecordError($"Parameter type named {typeName} does not exist.");
                return null;
            }

            BuiltInParameterGroup parameterGroup = BuiltInParameterGroup.INVALID;
            foreach (BuiltInParameterGroup bpg in System.Enum.GetValues(typeof(BuiltInParameterGroup)))
            {
                if (LabelUtils.GetLabelFor(bpg) == group)
                {
                    parameterGroup = bpg;
                    break;
                }
            }

            if (parameterGroup == BuiltInParameterGroup.INVALID)
            {
                BH.Engine.Reflection.Compute.RecordError($"Parameter group named {group} does not exist.");
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
                BH.Engine.Reflection.Compute.RecordError("Parameter could not be created due to the following categories do not exist in the active document: " +
                                                          string.Join(", ", distinctCategoryNames.Except(categories.Select(x => x.Name))) + ".");
                return null;
            }

            if (shared)
                return Create.SharedParameter(document, parameterName, groupName, parameterType, parameterGroup, instance, categories);
            else
                return Create.ProjectParameter(document, parameterName, groupName, parameterType, parameterGroup, instance, categories);
        }

        /***************************************************/
    }
}


