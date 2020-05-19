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

        [Description("Creates a new workset if one does not exist under the specified name, otherwise returns the existing one.")]
        [Input("document", "Revit model file.")]
        [Input("worksetName", "Name of the new workset to be added.")]
        [Output("workset", "Existing workset if it exists or a newly created workset.")]
        public static Workset Workset(Document document, string worksetName)
        {
            // Skip non-workshared documents and null/empty workset names
            if (!document.IsWorkshared || string.IsNullOrWhiteSpace(worksetName))
            {
                BH.Engine.Reflection.Compute.RecordError("Document must be work shared, and workset name cannot be null or empty.");
                return null;
            }

            // Avoid recreating a workset if one with the same name already exists
            FilteredWorksetCollector worksets = new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset); // Find all user worksets
            Workset workset = worksets.Where(x => x.Name == worksetName).FirstOrDefault(); // Find the specified workset if it exists
            if (workset != null)
            {
                // The specified workset already exists
                BH.Engine.Reflection.Compute.RecordWarning("A workset with the same name already exists and is therefore reused.");
                return workset; // Return the existing workset
            }
            else
            {
                // No existing workset with the same name was found
                // Create a new workset
                workset = Autodesk.Revit.DB.Workset.Create(document, worksetName); // Full namespace path specified to avoid a name clash with "BH.UI.Revit.Engine.Create", which cannot be omitted with "using Autodesk.Revit.DB;"

                return workset; // Return the newly created workset
            }
        }

        /***************************************************/
    }
}

