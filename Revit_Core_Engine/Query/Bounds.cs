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

        [Description("Returns the combined bounding box of a given colection of volumetric solids.")]
        [Input("solids", "A collection of solids to find the bounds for.")]
        [Input("transform", "Optional transform of the bounding box's coordinate system.")]
        [Output("bounds", "Combined bounding box of the input colection of volumetric solids.")]
        public static BoundingBoxXYZ Bounds(this IEnumerable<Solid> solids, Transform transform = null)
        {
            solids = solids?.Where(x => x != null && x.Volume > 1e-6).ToList();
            if (solids == null || !solids.Any())
                return null;

            Solid union = solids.First();
            foreach (Solid solid in solids.Skip(1))
            {
                union = BooleanOperationsUtils.ExecuteBooleanOperation(union, solid, BooleanOperationsType.Union);
            }

            if (transform != null)
                union = SolidUtils.CreateTransformed(union, transform);

            return union.GetBoundingBox();
        }

        /***************************************************/
    }
}
