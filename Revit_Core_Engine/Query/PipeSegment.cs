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
using Autodesk.Revit.DB.Plumbing;
using BH.oM.Base.Attributes;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Queries a PipeSegment by its material name and schedule type name from the Revit document.")]
        [Input("document", "Revit document to query the PipeSegment from.")]
        [Input("materialName", "Name of the material of PipeSegment to be queried.")]
        [Input("scheduleTypeName", "Name of the schedule type of PipeSegment to be queried.")]
        [Output("pipeSegment", "Revit PipeSegment with the specified material and schedule type, or null if not found.")]
        public static PipeSegment PipeSegment(this Document document, string materialName, string scheduleTypeName)
        {
            ElementId materialId = document.Material(materialName)?.Id ?? Autodesk.Revit.DB.ElementId.InvalidElementId;
            if (materialId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            ElementId scheduleTypeId = PipeScheduleType.GetPipeScheduleId(document, scheduleTypeName);
            if (scheduleTypeId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return new FilteredElementCollector(document)
                    .OfClass(typeof(PipeSegment))
                    .Cast<PipeSegment>()
                    .FirstOrDefault(ps =>
                    ps.MaterialId == materialId &&
                    ps.ScheduleTypeId == scheduleTypeId);
        }

        /***************************************************/
    }
}

