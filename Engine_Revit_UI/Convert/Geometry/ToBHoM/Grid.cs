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

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static BHoMObject ToBHoMGrid(this Grid grid, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            oM.Geometry.SettingOut.Grid aGrid = pullSettings.FindRefObject<oM.Geometry.SettingOut.Grid>(grid.Id.IntegerValue);
            if (aGrid != null)
                return aGrid;

            aGrid = BH.Engine.Geometry.SettingOut.Create.Grid(grid.Curve.ToBHoM(pullSettings));
            aGrid.Name = grid.Name;

            aGrid = Modify.SetIdentifiers(aGrid, grid) as oM.Geometry.SettingOut.Grid;
            if (pullSettings.CopyCustomData)
                aGrid = Modify.SetCustomData(aGrid, grid, pullSettings.ConvertUnits) as oM.Geometry.SettingOut.Grid;

            pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(aGrid);

            return aGrid;
        }

        /***************************************************/
    }
}