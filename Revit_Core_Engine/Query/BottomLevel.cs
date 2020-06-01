/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static Level BottomLevel(this oM.Geometry.ICurve curve, Document document)
        {
            if (curve == null)
                return null;

            double minElevation = MinElevation(curve);

            return BottomLevel(minElevation, document);
        }


        /***************************************************/

        public static Level BottomLevel(this oM.Geometry.Point point, Document document)
        {
            if (point == null)
                return null;

            return BottomLevel(point.Z, document);
        }

        /***************************************************/

        public static Level BottomLevel(this double elevation, Document document, double tolerance = oM.Geometry.Tolerance.Distance)
        {
            if (double.IsNaN(elevation) || double.IsNegativeInfinity(elevation) || double.IsPositiveInfinity(elevation))
                return null;

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            double levelElevation = elevation.FromSI(UnitType.UT_Length);

            if (levelElevation <= levels.First().Elevation)
                return levels.First();

            if (levelElevation >= levels.Last().Elevation)
                return levels.Last();

            Level level = levels.Find(x => System.Math.Abs(x.Elevation - levelElevation) < tolerance);
            if (level != null)
                return level;

            for (int i = 1; i < levels.Count; i++)
                if (levels[i].Elevation > levelElevation)
                    return levels[i - 1];

            return null;
        }


        /***************************************************/
        /****              Private Methods              ****/
        /***************************************************/

        static private double MinElevation(oM.Geometry.ICurve curve)
        {
            //TODO: Method to be removed as zoon as a proper Bounds method is implemented for NurbsCurve
            if (curve is oM.Geometry.NurbsCurve)
                return BH.Engine.Geometry.Query.Bounds(((oM.Geometry.NurbsCurve)curve).ControlPoints).Min.Z;

            return BH.Engine.Geometry.Query.Bounds(curve as dynamic).Min.Z;
        }

        /***************************************************/
    }
}