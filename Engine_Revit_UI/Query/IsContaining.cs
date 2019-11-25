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

using System;
using System.Collections.Generic;

using BH.oM.Geometry;

using Autodesk.Revit.DB;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static bool IsContaining(this BoundingBoxXYZ box, XYZ pt, bool acceptOnEdge = true, double tolerance = Tolerance.Distance)
        {
            XYZ max = box.Max;
            XYZ min = box.Min;

            if (acceptOnEdge)
            {
                return (pt.X >= min.X - tolerance && pt.X <= max.X + tolerance &&
                        pt.Y >= min.Y - tolerance && pt.Y <= max.Y + tolerance &&
                        pt.Z >= min.Z - tolerance && pt.Z <= max.Z + tolerance);
            }
            else
            {
                return (pt.X > min.X + tolerance && pt.X < max.X - tolerance &&
                        pt.Y > min.Y + tolerance && pt.Y < max.Y - tolerance &&
                        pt.Z > min.Z + tolerance && pt.Z < max.Z - tolerance);
            }
        }

        /***************************************************/

        //TODO: Not accurate method may cause issue with opening assigement. To be fixed
        public static bool IsContaining(this PlanarSurface planarSurface1, PlanarSurface planarSurface2)
        {
            if (planarSurface1 == null || planarSurface2 == null)
                return false;

            ICurve curve2 = planarSurface2.ExternalBoundary;
            if (curve2 == null)
                return false;

            List<oM.Geometry.Point> points = BH.Engine.Geometry.Query.IControlPoints(curve2);
            if (points == null || points.Count == 0)
                return false;

            return IsContaining(planarSurface1, points);
        }

        /***************************************************/

        //TODO: Not accurate method may cause issue with opening assigement. To be fixed
        public static bool IsContaining(this PlanarSurface planarSurface, IEnumerable<oM.Geometry.Point> points)
        {
            if(planarSurface == null || points == null)
                return false;

            ICurve curve = planarSurface.ExternalBoundary;
            if (curve == null)
                return false;

            BoundingBox bbox = BH.Engine.Geometry.Query.IBounds(curve);

            foreach (oM.Geometry.Point pt in points)
            {
                if (BH.Engine.Geometry.Query.IIsContaining(bbox, pt))
                    return true;
            }

            return false;
        }

        /***************************************************/
    }
}