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

        [Description("Find and return an existing workset with a specified name or return null if it doesn't exist.")]
        [Input("document", "Revit model file.")]
        [Input("worksetName", "Name of the requested workset as it is shown in the UI of Revit.")]
        [Output("workset", "Existing workset with a specified name if it exists or null if it doesn't.")]
        public static Workset Workset(this Document document, string worksetName)
        {
            // Skip non-workshared documents and null/empty workset names
            if (!document.IsWorkshared || string.IsNullOrWhiteSpace(worksetName))
            {
                BH.Engine.Base.Compute.RecordError("Document must be work shared to contain worksets, and workset name cannot be null or empty.");
                return null;
            }
            
            // Find a workset with a specified name if it exists
            FilteredWorksetCollector worksets = new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset); // Find all user worksets
            return worksets.Where(x => x.Name == worksetName).FirstOrDefault(); // Return the specified workset if it exists or null if it doesn't
        }

        /***************************************************/
    }
}


