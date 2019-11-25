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
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Grid ToRevitGrid(this oM.Geometry.SettingOut.Grid grid, Document document, PushSettings pushSettings = null)
        {
            Grid revitGrid = pushSettings.FindRefObject<Grid>(document, grid.BHoM_Guid);
            if (revitGrid != null)
                return revitGrid;

            pushSettings.DefaultIfNull();

            Curve curve = grid.Curve.ToRevitCurve();

            if (curve is Line)
                revitGrid = Grid.Create(document, (Line)curve);
            if (curve is Arc)
                revitGrid = Grid.Create(document, (Arc)curve);

            revitGrid.CheckIfNullPush(grid);
            if (revitGrid == null)
                return null;

            if (pushSettings.CopyCustomData)
                Modify.SetParameters(revitGrid, grid, null);

            pushSettings.RefObjects = pushSettings.RefObjects.AppendRefObjects(grid, revitGrid);

            return revitGrid;
        }

        /***************************************************/
    }
}