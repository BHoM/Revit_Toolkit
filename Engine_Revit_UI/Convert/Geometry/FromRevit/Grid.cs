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
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static BH.oM.Geometry.SettingOut.Grid GridFromRevit(this Grid revitGrid, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Geometry.SettingOut.Grid grid = refObjects.GetValue<oM.Geometry.SettingOut.Grid>(revitGrid.Id);
            if (grid != null)
                return grid;

            grid = BH.Engine.Geometry.SettingOut.Create.Grid(revitGrid.Curve.IFromRevit());
            grid.Name = revitGrid.Name;

            //Set identifiers & custom data
            grid.SetIdentifiers(revitGrid);
            grid.SetCustomData(revitGrid);

            refObjects.AddOrReplace(revitGrid.Id, grid);
            return grid;
        }

        /***************************************************/

        public static BH.oM.Geometry.SettingOut.Grid GridFromRevit(this MultiSegmentGrid revitGrid, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            settings = settings.DefaultIfNull();

            oM.Geometry.SettingOut.Grid grid = refObjects.GetValue<oM.Geometry.SettingOut.Grid>(revitGrid.Id);
            if (grid != null)
                return grid;

            List<Grid> gridSegments = revitGrid.GetGridIds().Select(x => revitGrid.Document.GetElement(x) as Grid).ToList();
            if (gridSegments.Count == 0)
                return null;
            else if (gridSegments.Count == 1)
                return gridSegments[0].GridFromRevit(settings, refObjects);
            else
            {
                List<BH.oM.Geometry.PolyCurve> joinedGridCurves = BH.Engine.Geometry.Compute.IJoin(gridSegments.Select(x => x.Curve.IFromRevit()).ToList());
                if (joinedGridCurves.Count != 1)
                {
                    BH.Engine.Reflection.Compute.RecordError(String.Format("Revit grid consists of disjoint segments. Element id: {0}", revitGrid.Id));
                    return null;
                }

                grid = BH.Engine.Geometry.SettingOut.Create.Grid(joinedGridCurves[0]);
                grid.Name = revitGrid.Name;
            }

            //Set identifiers & custom data
            grid.SetIdentifiers(revitGrid);
            grid.SetCustomData(revitGrid);

            refObjects.AddOrReplace(revitGrid.Id, grid);
            return grid;
        }

        /***************************************************/
    }
}
