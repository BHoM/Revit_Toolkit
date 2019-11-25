/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

using BH.oM.Geometry;
using BH.oM.Common.Interface;
using BH.oM.Environment.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static double HighElevation(ICurve curve)
        {
            return BH.Engine.Geometry.Query.Bounds(curve as dynamic).Max.Z;
        }

        /***************************************************/

        public static double HighElevation(this IObject2D object2D)
        {
            if (object2D == null || object2D.Surface == null)
                return double.NaN;

            BoundingBox bbox = BH.Engine.Geometry.Query.Bounds(object2D.Surface as dynamic);

            return bbox.Max.Z;
        }

        /***************************************************/

        public static double HighElevation(this Panel panel)
        {
            if (panel == null || panel.ExternalEdges == null)
                return double.NaN;

            double result = double.NaN;
            foreach (Edge edge in panel.ExternalEdges)
            {
                BoundingBox bbox = BH.Engine.Geometry.Query.Bounds(edge.Curve as dynamic);
                if (bbox != null && (double.IsNaN(result) || result > bbox.Max.Z))
                    result = bbox.Max.Z;
            }

            return result;
        }

        /***************************************************/

        public static double HighElevation(this oM.Physical.Elements.ISurface surface)
        {
            if (surface == null || surface.Location == null || !(surface.Location is PlanarSurface))
                return double.NaN;

            BoundingBox bbox = BH.Engine.Geometry.Query.Bounds(surface.Location as PlanarSurface);
            return bbox.Max.Z;
        }

        /***************************************************/
    }
}