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

        [Description("Finds a bounding box of an element in on its local coordinate system, which is:" +
                     "\n- for FamilyInstance: coordinate system as defined in the object definition (result of GetTotalTransform method )" +
                     "\n- for everything else: coordinate system based on normals of the major planar faces of the element")]
        [Input("element", "Element to query for aligned bounding box.")]
        [Input("options", "Options to apply when extracting solid representation of an element.")]
        [Output("alignedBBox", "Bounding box of the input element in on its local coordinate system.")]
        public static Solid AlignedBoundingBox(this Element element, Options options)
        {
            Transform transform = element.LocalCoordinateSystem();
            List<Solid> solids = element.Solids(options).Select(x => SolidUtils.CreateTransformed(x, transform.Inverse)).ToList();
            BoundingBoxXYZ bounds = solids.Select(x => x.GetBoundingBox()).Bounds();
            return SolidUtils.CreateTransformed(bounds.ToSolid(), transform);
        }

        /***************************************************/
    }
}
