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

using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/
        
        public static Level BottomLevel(this oM.Geometry.ICurve curve, Document document, bool convertUnits = true)
        {
            if (curve == null)
                return null;

            double aMinElevation = MinElevation(curve);

            //double aMinElevation = BH.Engine.Geometry.Query.Bounds(curve as dynamic).Min.Z;
            //if (convertUnits)
            //    aMinElevation = aMinElevation.FromSI(UnitType.UT_Length);

            return BottomLevel(aMinElevation, document, convertUnits);
        }


        /***************************************************/

        public static Level BottomLevel(this oM.Geometry.Point point, Document document, bool convertUnits = true)
        {
            if (point == null)
                return null;

            return BottomLevel(point.Z, document, convertUnits);
        }

        /***************************************************/

        public static Level BottomLevel(this double elevation, Document document, bool convertUnits = true, double tolerance = oM.Geometry.Tolerance.Distance)
        {
            if (double.IsNaN(elevation) || double.IsNegativeInfinity(elevation) || double.IsPositiveInfinity(elevation))
                return null;

            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            aLevelList.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            double aElevation = elevation;
            if (convertUnits)
                aElevation = aElevation.FromSI(UnitType.UT_Length);

            if (aElevation <= aLevelList.First().Elevation)
                return aLevelList.First();

            if (aElevation >= aLevelList.Last().Elevation)
                return aLevelList.Last();

            Level aLevel = aLevelList.Find(x => System.Math.Abs(x.Elevation - aElevation) < tolerance);
            if (aLevel != null)
                return aLevel;

            for (int i = 1; i < aLevelList.Count; i++)
                if (aLevelList[i].Elevation > aElevation)
                    return aLevelList[i - 1];

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