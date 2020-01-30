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
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Common.Interface;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Level LowLevel(this Document document, double elevation)
        {
            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (levels == null || levels.Count == 0)
                return null;

            levels.Sort((x, y) => y.Elevation.CompareTo(x.Elevation));

            double levelElevation = elevation.FromSI(UnitType.UT_Length);

            if (Math.Abs(levelElevation - levels.First().Elevation) < oM.Geometry.Tolerance.MacroDistance)
                return levels.First();

            for (int i = 1; i < levels.Count; i++)
            {
                if (Math.Round(levelElevation, 3, MidpointRounding.AwayFromZero) >= Math.Round(levels[i].Elevation, 3, MidpointRounding.AwayFromZero))
                    return levels[i];
            }

            return levels.Last();
        }

        /***************************************************/

        public static Level LowLevel(this Document document, oM.Geometry.ICurve curve)
        {
            double elevation = LowElevation(curve);

            return LowLevel(document, elevation);
        }

        /***************************************************/

        public static Level LowLevel(this Document document, IObject2D object2D)
        {
            double elevation = LowElevation(object2D);

            return LowLevel(document, elevation);
        }

        /***************************************************/
    }
}
