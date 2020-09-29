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
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Level LevelAbove(this Document document, double elevation, RevitSettings settings = null, bool closestIfNotFound = true)
        {
            settings = settings.DefaultIfNull();

            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().OrderBy(x => x.Elevation).ToList();
            if (levels.Count == 0)
                return null;

            Level level = levels.FirstOrDefault(x => x.ProjectElevation + settings.DistanceTolerance >= elevation);
            if (level == null && closestIfNotFound)
                level = levels[levels.Count - 1];

            return level;
        }

        /***************************************************/

        public static Level LevelAbove(this Document document, IGeometry geometry, RevitSettings settings = null, bool closestIfNotFound = true)
        {
            BoundingBox bbox = geometry.IBounds();
            if (bbox == null)
                return null;

            return document.LevelAbove(bbox.Max.Z.FromSI(UnitType.UT_Length), settings, closestIfNotFound);
        }

        /***************************************************/
    }
}
