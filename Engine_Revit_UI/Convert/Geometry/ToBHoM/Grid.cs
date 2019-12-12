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

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static BH.oM.Geometry.SettingOut.Grid ToBHoMGrid(this Grid revitGrid, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Geometry.SettingOut.Grid grid = pullSettings.FindRefObject<oM.Geometry.SettingOut.Grid>(revitGrid.Id.IntegerValue);
            if (grid != null)
                return grid;

            grid = BH.Engine.Geometry.SettingOut.Create.Grid(revitGrid.Curve.IToBHoM());
            grid.Name = revitGrid.Name;

            grid = Modify.SetIdentifiers(grid, revitGrid) as oM.Geometry.SettingOut.Grid;
            if (pullSettings.CopyCustomData)
                grid = Modify.SetCustomData(grid, revitGrid) as oM.Geometry.SettingOut.Grid;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(grid);

            return grid;
        }

        /***************************************************/

        public static BH.oM.Geometry.SettingOut.Grid ToBHoMGrid(this MultiSegmentGrid revitGrid, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Geometry.SettingOut.Grid grid = pullSettings.FindRefObject<oM.Geometry.SettingOut.Grid>(revitGrid.Id.IntegerValue);
            if (grid != null)
                return grid;

            List<Grid> gridSegments = revitGrid.GetGridIds().Select(x => revitGrid.Document.GetElement(x) as Grid).ToList();
            if (gridSegments.Count == 0)
                return null;
            else if (gridSegments.Count == 1)
                return gridSegments[0].ToBHoMGrid(pullSettings);
            else
            {
                grid = BH.Engine.Geometry.SettingOut.Create.Grid(new BH.oM.Geometry.PolyCurve { Curves = gridSegments.Select(x => x.Curve.IToBHoM()).ToList() });
                grid.Name = revitGrid.Name;
            }
            
            grid = Modify.SetIdentifiers(grid, revitGrid) as oM.Geometry.SettingOut.Grid;
            if (pullSettings.CopyCustomData)
                grid = Modify.SetCustomData(grid, revitGrid) as oM.Geometry.SettingOut.Grid;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(grid);

            return grid;
        }

        /***************************************************/
    }
}