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

using BH.oM.Common.Interface;
using BH.oM.Environment.Elements;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static double LowElevation(ICurve curve)
        {
            return BH.Engine.Geometry.Query.Bounds(curve as dynamic).Min.Z;
        }

        /***************************************************/

        public static double LowElevation(this Panel panel)
        {
            if (panel == null || panel.ExternalEdges == null)
                return double.NaN;

            double aResult = double.NaN;
            foreach(Edge aEdge in panel.ExternalEdges)
            {
                BoundingBox aBoundingBox = BH.Engine.Geometry.Query.Bounds(aEdge.Curve as dynamic);
                if(aBoundingBox != null && (double.IsNaN(aResult) || aResult < aBoundingBox.Min.Z))
                        aResult = aBoundingBox.Min.Z;
            }

            return aResult;
        }

        /***************************************************/

        public static double LowElevation(this IObject2D object2D)
        {
            if (object2D == null || object2D.Surface == null)
                return double.NaN;

            BoundingBox aBoundingBox = BH.Engine.Geometry.Query.Bounds(object2D.Surface as dynamic);

            return aBoundingBox.Min.Z;
        }

        /***************************************************/

        public static double LowElevation(this oM.Physical.Elements.ISurface surface)
        {
            if (surface == null || surface.Location == null)
                return double.NaN;

            return LowElevation(surface.Location);
        }

        /***************************************************/

        public static double LowElevation(this ISurface surface)
        {
            if (surface == null || surface == null)
                return double.NaN;

            BoundingBox aBoundingBox = BH.Engine.Geometry.Query.Bounds(surface as dynamic);

            return aBoundingBox.Min.Z;
        }

        /***************************************************/
    }
}