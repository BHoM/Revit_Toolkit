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

using Autodesk.Revit.DB;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a new workset with a specified name and sets the 'Workset' dropdown parameter to it.")]
        [Input("document", "Revit model file")]
        [Input("parameter", "Revit parameter")]
        [Input("worksetName", "Name of the new workset")]
        [Output("bool", "true for success, false for failure")]
        public static bool Workset(Document document, Parameter parameter, string worksetName)
        {
            // Skip non-workshared documents because they can't contain worksets
            if (document.IsWorkshared == false)
                return false;

            // Skip non-string workset names
            if (worksetName is string == false)
                return false;

            // Skip null or empty workset names
            if (string.IsNullOrWhiteSpace(worksetName))
                return false;

            // Skip non-workset parameters
            if (parameter.Id.IntegerValue != (int)BuiltInParameter.ELEM_PARTITION_PARAM)
                return false;

            // Skip null and read-only parameters
            if (parameter == null || parameter.IsReadOnly)
                return false;

            // Avoid recreating a workset if one with the same name already exists
            FilteredWorksetCollector worksets = new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset); // Find all user worksets
            Workset existingWorkset = worksets.Where(x => x.Name == worksetName).FirstOrDefault(); // Find the specified workset if it exists
            if (existingWorkset != null)
            {
                // Set the "Workset" dropdown parameter to the existing workset without recreating a new one
                parameter.Set(existingWorkset.Id.IntegerValue); // Select a dropdown list item
                return true;
            }

            // No existing worksets with the same name were found
            // Create a new workset
            Workset workset = null;
            workset = Autodesk.Revit.DB.Workset.Create(document, worksetName); // Full namespace specified to avoid a name conflict with an Autodesk method

            // Set the dropdown parameter to the newly created workset
            parameter.Set(workset.Id.IntegerValue); // Select a dropdown list item

            return true;
        }

        /***************************************************/
    }
}

