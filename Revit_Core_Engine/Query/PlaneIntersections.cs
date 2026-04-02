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
using BH.oM.Base.Attributes;
using System;
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

        [Description("Finds all intersections between a solid and plane.")]
        [Input("solid", "Solid to compute intersections.")]
        [Input("plane", "Plane to compute intersections.")]
        [Output("intersections", "All intersections between the input solid and plane.")]
        public static IEnumerable<PlanarFace> PlaneIntersections(this Solid solid, Plane plane)
        {
            Solid cut = BooleanOperationsUtils.CutWithHalfSpace(solid, plane);
            if (cut != null)
            {
                foreach (PlanarFace pf in cut.Faces.OfType<PlanarFace>())
                {
                    if (1 - Math.Abs(plane.Normal.DotProduct(pf.FaceNormal)) <= 1e-6 && plane.Distance(pf.Origin) <= 1e-6)
                        yield return pf;
                }
            }
        }

        /***************************************************/
    }
}

