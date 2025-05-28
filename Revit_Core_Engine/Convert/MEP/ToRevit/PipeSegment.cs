
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
using Autodesk.Revit.DB.Plumbing;
using BH.Engine.Adapters.Revit;
using BH.Engine.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.MEP.System.MaterialFragments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.MEP.System.Pipe to a Revit Pipe.")]
        [Input("pipe", "BH.oM.MEP.System.Pipe to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("pipe", "Revit Pipe resulting from converting the input BH.oM.MEP.System.Pipe.")]
        public static PipeSegment ToRevitPipeSegement(this PipeMaterial pipeMaterial, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (document == null || pipeMaterial == null)
                return null;

            // Retrieve existing PipeSegment from refObjects
            PipeSegment revitPipeSegment = refObjects.GetValue<PipeSegment>(document, pipeMaterial.BHoM_Guid);
            if (revitPipeSegment != null)
                return revitPipeSegment;

            // Prepare settings
            settings = settings.DefaultIfNull();

            PipeDesignData designData = pipeMaterial.FindFragment<PipeDesignData>();

            // Extract conversion data
            Dictionary<double, PipeSize> sizeSet = designData?.SizeSet;
            if (sizeSet == null)
            {
                BH.Engine.Base.Compute.RecordWarning("No size table has been found in the PipeMaterial. Pipe creation requires a valid size table.");
                return null;
            }

            ForgeTypeId bHoMUnit = SpecTypeId.Length;
            List<MEPSize> mepSizes = new List<MEPSize>();
            foreach (KeyValuePair<double, PipeSize> size in sizeSet)
            {
                mepSizes.Add(new MEPSize(
                    size.Key.FromSI(bHoMUnit),
                    size.Value.InnerDiameter.FromSI(bHoMUnit),
                    size.Value.OuterDiameter.FromSI(bHoMUnit),
                    usedInSizeLists: true, usedInSizing: true));
            }

            string materialName = designData.Material;
            string scheduleTypeName = designData.ScheduleType;

            // MaterialName must be valid
            Material material = new FilteredElementCollector(document)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .FirstOrDefault(x => x.Name == materialName);
            if (material == null)
            {
                BH.Engine.Base.Compute.RecordNote("No valid material has been found in the Revit model. Pipe creation requires the presence of a valid material.");
                return null;
            }

            // PipeScheduleType is created new if not found
            ElementId scheduleTypeId = PipeScheduleType.GetPipeScheduleId(document, scheduleTypeName);
            if (scheduleTypeId == ElementId.InvalidElementId)
            {
                PipeScheduleType scheduleType = PipeScheduleType.Create(document, scheduleTypeName);
                scheduleTypeId = scheduleType.Id;
            }

            revitPipeSegment = PipeSegment.Create(document, material.Id, scheduleTypeId, mepSizes);
            revitPipeSegment.Name = pipeMaterial.Name;
            revitPipeSegment.Description = designData.Description;
            revitPipeSegment.Roughness = pipeMaterial.Roughness.FromSI(bHoMUnit);

            // Copy parameters from BHoM object to Revit element
            revitPipeSegment.CopyParameters(pipeMaterial, settings);

            // Add to refObjects and return
            refObjects?.AddOrReplace(pipeMaterial, revitPipeSegment);
            return revitPipeSegment;
        }

        /***************************************************/
    }
}
