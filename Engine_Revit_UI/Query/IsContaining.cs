/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
        /**** Public Methods                            ****/
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
        public static bool IsContaining(this PlanarSurface planarSurface_1, PlanarSurface planarSurface_2)
        {
            if (planarSurface_1 == null || planarSurface_2 == null)
                return false;

            ICurve aICurve_1 = planarSurface_1.ExternalBoundary;
            if (aICurve_1 == null)
                return false;

            ICurve aICurve_2 = planarSurface_2.ExternalBoundary;
            if (aICurve_2 == null)
                return false;

            List<oM.Geometry.Point> aPointList = BH.Engine.Geometry.Query.IControlPoints(aICurve_2);
            if (aPointList == null || aPointList.Count == 0)
                return false;
            
            BoundingBox aBoundingBox = BH.Engine.Geometry.Query.IBounds(aICurve_1);

            foreach (oM.Geometry.Point aPoint in aPointList)
            {
                
                if (BH.Engine.Geometry.Query.IIsContaining(aBoundingBox, aPoint))
                    return true;
            }

            return false;
        }
    }
}