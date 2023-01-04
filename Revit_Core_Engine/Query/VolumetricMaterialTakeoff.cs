/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
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

        [Description("Extracts the material takeoff from a given Revit Element.")]
        [Input("element", "Revit Element to extract the material takeoff from.")]
        [Input("settings", "Revit adapter settings to be used while performing the extraction.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("takeoff", "Material takeoff extracted from the input Revit Element.")]
        public static oM.Physical.Materials.VolumetricMaterialTakeoff VolumetricMaterialTakeoff(this Element element, RevitSettings settings, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            Dictionary<oM.Physical.Materials.Material, double> takeoff = new Dictionary<BH.oM.Physical.Materials.Material, double>();
            double totalVolume = 0;
            string grade = element.MaterialGrade(settings);
            foreach (ElementId materialId in element.GetMaterialIds(false))
            {
                double volume = element.GetMaterialVolume(materialId).ToSI(SpecTypeId.Volume);
                if (volume <= settings.DistanceTolerance)
                    continue;

                Material material = (Material)element.Document.GetElement(materialId);
                BH.oM.Physical.Materials.Material bHoMMaterial = material.MaterialFromRevit(grade, settings, refObjects);
                if (takeoff.ContainsKey(bHoMMaterial))
                    takeoff[bHoMMaterial] += volume;
                else
                    takeoff[bHoMMaterial] = volume;

                totalVolume += volume;
            }

            if (takeoff.Count != 0)
                return new oM.Physical.Materials.VolumetricMaterialTakeoff(takeoff.Keys.ToList(), takeoff.Values.ToList());
            else
                return null;
        }

        /***************************************************/
    }
}


