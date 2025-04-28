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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.MEP.System.SectionProperties;
using BH.oM.Base.Attributes;
using BH.oM.Spatial.ShapeProfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BH.oM.MEP.System.MaterialFragments;
using Autodesk.Revit.DB.Plumbing;
using System.Drawing;

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
            double tolerance = settings.DistanceTolerance;
            var bHoMUnit = SpecTypeId.Length;

            // Extract conversion data
            var sizeTable = pipeMaterial.CustomData["SizeTable"] as Dictionary<double, List<double>>;
            var mEPSizes = sizeTable.Select(x => new MEPSize(
                    x.Key.FromSI(bHoMUnit),
                    x.Value[0].FromSI(bHoMUnit),
                    x.Value[1].FromSI(bHoMUnit),
                    true, true)).ToArray();

            ElementId materialId = pipeMaterial.CustomData["MaterialId"] as ElementId;
            ElementId scheduleTypeId = pipeMaterial.CustomData["ScheduleTypeId"] as ElementId;

            // Retrieve from document if not in refObjects
            if (revitPipeSegment == null)
            {
                revitPipeSegment = document.GetElement(pipeMaterial.ElementId()) as PipeSegment;
            }

            // Create if not found
            if (revitPipeSegment == null)
            {
                PipeSegment created = PipeSegment.Create(document, materialId, scheduleTypeId, mEPSizes);
                created.Description = pipeMaterial.CustomData["Description"].ToString();
                refObjects?.AddOrReplace(pipeMaterial, created);
                return created;
            }

            // Update Description
            revitPipeSegment.Description = pipeMaterial.CustomData["Description"].ToString();

            // Update Sizes
            ICollection<MEPSize> existingSizes = revitPipeSegment.GetSizes();

            foreach (var newSize in mEPSizes)
            {
                var similarSize = existingSizes.FirstOrDefault(x => Math.Abs(x.NominalDiameter - newSize.NominalDiameter) <= tolerance);

                if (similarSize == null)
                {
                    revitPipeSegment.AddSize(newSize);
                }
                else if ((Math.Abs(similarSize.InnerDiameter - newSize.InnerDiameter) > tolerance) ||
                         (Math.Abs(similarSize.OuterDiameter - newSize.OuterDiameter) > tolerance))
                {
                    revitPipeSegment.RemoveSize(similarSize.NominalDiameter);
                    revitPipeSegment.AddSize(newSize);
                }
            }

            // Update refObjects
            refObjects?.AddOrReplace(pipeMaterial, revitPipeSegment);

            return revitPipeSegment;

        }

		/***************************************************/
	}
}





