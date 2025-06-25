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

using BH.oM.Adapters.Revit.Elements;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Validates a collection of PipeSize objects, ensuring they are not empty, have valid dimensions, and do not contain duplicates. Returns a boolean indicating validity and a list of unique PipeSize objects.")]
        [Input("pipeSizes", "Collection of PipeSize objects to validate.")]
        [MultiOutput(0, "isValid", "Boolean indicating whether the collection is valid.")]
        [MultiOutput(1, "uniquePipeSizes", "List of unique PipeSize objects, ordered by NominalDiameter.")]
        public static Output<bool, List<PipeSize>> Validate(this List<PipeSize> pipeSizes)
        {
            if (pipeSizes == null)
            {
                BH.Engine.Base.Compute.RecordWarning("Pipe size collection is null.");
                return new Output<bool, List<PipeSize>>
                {
                    Item1 = false,
                    Item2 = null
                };
            }

            if (pipeSizes.Any(x => double.IsNaN(x.NominalDiameter) || double.IsNaN(x.InnerDiameter) || double.IsNaN(x.OuterDiameter) || x.NominalDiameter <= Tolerance.Distance || x.InnerDiameter <= Tolerance.Distance || x.OuterDiameter <= Tolerance.Distance))
            {
                BH.Engine.Base.Compute.RecordWarning("Some sizes in the collection have invalid values.");
                return new Output<bool, List<PipeSize>>
                {
                    Item1 = false,
                    Item2 = null
                };
            }

            if (pipeSizes.Any(x => x.InnerDiameter > x.OuterDiameter))
            {
                BH.Engine.Base.Compute.RecordWarning("Some sizes in the collection have larger outer diameters than inner ones.");
                return new Output<bool, List<PipeSize>>
                {
                    Item1 = false,
                    Item2 = null
                };
            }

            List<PipeSize> unique = pipeSizes
                .GroupBy(x => (x.NominalDiameter, x.InnerDiameter, x.OuterDiameter))
                .Select(g => g.First())
                .ToList();

            if (unique.Count != pipeSizes.Count)
                BH.Engine.Base.Compute.RecordNote("Some sizes in the collection were duplicated, they have been removed.");

            return new Output<bool, List<PipeSize>>
            {
                Item1 = true,
                Item2 = unique.OrderBy(x => x.NominalDiameter).ToList()
            };
        }

        /***************************************************/
    }
}
