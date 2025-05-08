
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
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using BH.oM.MEP.System.MaterialFragments;
using BH.oM.Physical.Materials;
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
        public static Autodesk.Revit.DB.Plumbing.PipeSegment ToRevitPipeSegement(this PipeMaterial pipeMaterial, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            if (document == null || pipeMaterial == null)
                return null;

            // Retrieve existing PipeSegment from refObjects
            PipeSegment revitPipeSegment = refObjects.GetValue<PipeSegment>(document, pipeMaterial.BHoM_Guid);

            // Prepare settings
            settings = settings.DefaultIfNull();

            // Roughness and pipe diameter are very small, so we need to scale down the tolerance
            double tolerance = settings.DistanceTolerance * 1E-3;
            ForgeTypeId bHoMUnit = SpecTypeId.Length;

            // Extract conversion data
            var sizeTable = pipeMaterial.CustomData["SizeTable"] as Dictionary<double, List<object>>;
            if (sizeTable == null)
            {
                BH.Engine.Base.Compute.RecordNote("No size table has been found in the PipeMaterial. Pipe creation requires a valid size table.");
                return null;
            }

            List<MEPSize> mEPSizes = new List<MEPSize>();
            foreach (var kvp in sizeTable)
            {
                mEPSizes.Add(new MEPSize(
                    kvp.Key.FromSI(bHoMUnit),
                    ((double)kvp.Value[0]).FromSI(bHoMUnit),
                    ((double)kvp.Value[1]).FromSI(bHoMUnit),
                    usedInSizeLists: true, usedInSizing: true));
            }

            PipeDesignDataset designDataset = pipeMaterial.Fragments.OfType<PipeDesignDataset>().FirstOrDefault();

            string materialName = designDataset.Material;
            string scheduleTypeName = designDataset.ScheduleType;

            // MaterialName must be valid
            Autodesk.Revit.DB.Material material = new FilteredElementCollector(document)
                .OfClass(typeof(Autodesk.Revit.DB.Material))
                .Cast<Autodesk.Revit.DB.Material>()
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

            // Retrieve PipeSegment from document if not in refObjects
            if (revitPipeSegment == null)
            {
                ElementId id = pipeMaterial.ElementId()?? ElementId.InvalidElementId;
                revitPipeSegment = document.GetElement(id) as PipeSegment;
            }

            // Retrieve PipeSegment by matching material and scheduleTypeId if not revitIdentifier is found
            if (revitPipeSegment == null)
            {
                revitPipeSegment = new FilteredElementCollector(document)
                    .OfClass(typeof(PipeSegment))
                    .Cast<PipeSegment>()
                    .FirstOrDefault(ps =>
                    ps.MaterialId == material.Id &&
                    ps.ScheduleTypeId == scheduleTypeId);
            }

            // Create new PipeSegment if not found in document
            if (revitPipeSegment == null)
            {
                PipeSegment created = PipeSegment.Create(document, material.Id, scheduleTypeId, mEPSizes);
                created.Description = pipeMaterial.CustomData["Description"].ToString();
                created.Roughness = pipeMaterial.Roughness.FromSI(bHoMUnit);
                refObjects?.AddOrReplace(pipeMaterial, created);
                return created;
            }

            // Update Roughness
            if (Math.Abs(revitPipeSegment.Roughness - pipeMaterial.Roughness.FromSI(bHoMUnit)) > tolerance)
            {
                revitPipeSegment.Roughness = pipeMaterial.Roughness.FromSI(bHoMUnit);
            }

            // Update Description
            revitPipeSegment.Description = pipeMaterial.CustomData["Description"].ToString();

            // Update Sizes
            ICollection<MEPSize> existingSizes = revitPipeSegment.GetSizes();
            Dictionary<double, MEPSize> checkedSizes = existingSizes.ToDictionary(x => x.NominalDiameter, x => x);

            // Update or Add
            foreach (var newSize in mEPSizes)
            {
                var similarSize = existingSizes.FirstOrDefault(x => Math.Abs(x.NominalDiameter - newSize.NominalDiameter) <= tolerance);

                if (similarSize == null)
                {
                    revitPipeSegment.AddSize(newSize);
                }
                else
                {
                    checkedSizes.Remove(similarSize.NominalDiameter);

                    if ((Math.Abs(similarSize.InnerDiameter - newSize.InnerDiameter) > tolerance) ||
                             (Math.Abs(similarSize.OuterDiameter - newSize.OuterDiameter) > tolerance))
                    {
                        revitPipeSegment.RemoveSize(similarSize.NominalDiameter);
                        revitPipeSegment.AddSize(newSize);
                    }
                }

            }

            // Clean oudated sizes
            foreach (var size in checkedSizes.Values)
            {
                revitPipeSegment.RemoveSize(size.NominalDiameter);
            }

            // Update refObjects
            refObjects?.AddOrReplace(pipeMaterial, revitPipeSegment);

            return revitPipeSegment;

        }

        /***************************************************/
    }
}





