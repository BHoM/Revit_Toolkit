/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<Curve> Curves(this Element element, Options options, Transform transform = null, RevitSettings settings = null, bool includeEdges = false)
        {
            List<GeometryObject> geometryPrimitives = element.GeometryPrimitives(options, transform, settings);
            if (geometryPrimitives == null)
                return null;

            List<Curve> result = geometryPrimitives.Where(x => x is Curve).Cast<Curve>().ToList();

            if (includeEdges)
                result.AddRange(geometryPrimitives.Where(x => x is Solid).Cast<Solid>().SelectMany(x => x.EdgeCurves()));

            return result;
        }

        /***************************************************/
    }
}


