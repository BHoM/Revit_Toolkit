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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.MEP.System.MaterialFragments;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a Revit PipeSegment into a BHoM PipeMaterial object, including its properties such as name, roughness, and size table. Avoids duplicate processing by utilizing a reference object collection.")]
        [Input("revitPipeSegment", "Revit PipeSegment to be converted.")]
        [Input("settings", "Revit adapter settings.")]
        [Input("refObjects", "A collection of objects processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("pipes", "List of BHoM MEP pipes converted from a Revit pipes.")]
        public static PipeMaterial PipeMaterialFromRevit(this PipeSegment revitPipeSegment, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            PipeMaterial pipeMaterial = refObjects.GetValue<PipeMaterial>(revitPipeSegment.Id);

            if (pipeMaterial != null)
            {
                return pipeMaterial;
            }
            else
            {
                pipeMaterial = new PipeMaterial();
            }

            Document document = revitPipeSegment.Document;
            pipeMaterial.Name = revitPipeSegment.Name;
            pipeMaterial.Roughness = revitPipeSegment.Roughness.ToSI(SpecTypeId.Length);

            ForgeTypeId forgeTypeId = SpecTypeId.Length;

            Dictionary<double, IEnumerable<double>> sizeTable = new Dictionary<double, IEnumerable<double>>();

            foreach (MEPSize size in revitPipeSegment.GetSizes())
            {
                double nominalDiameter = size.NominalDiameter.ToSI(forgeTypeId);
                double innerDiameter = size.InnerDiameter.ToSI(forgeTypeId);
                double outerDiameter = size.OuterDiameter.ToSI(forgeTypeId);

                sizeTable.Add(nominalDiameter, new List<double> { innerDiameter, outerDiameter});
            }

            pipeMaterial.CustomData.Add("Description", revitPipeSegment.Description);
            pipeMaterial.CustomData.Add("Material", document.GetElement(revitPipeSegment.MaterialId).Name);
            pipeMaterial.CustomData.Add("ScheduleType", document.GetElement(revitPipeSegment.ScheduleTypeId).Name);
            pipeMaterial.CustomData.Add("SizeTable", sizeTable);

            pipeMaterial.SetIdentifiers(revitPipeSegment);

            refObjects.AddOrReplace(revitPipeSegment.Id, pipeMaterial);
            return pipeMaterial;
        }

        /***************************************************/
    }
}





