/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Base;
using BH.oM.Geometry;
using BH.oM.Facade.Elements;
using BH.oM.Physical.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<FrameEdge> CurtainWallMullions(this CurtainGrid curtainGrid, Document document, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (curtainGrid == null)
                return null;

            //List<IOpening> result = new List<IOpening>();
            //List<Element> panels = curtainGrid.GetPanelIds().Select(x => document.GetElement(x)).ToList();
            //List<CurtainCell> cells = curtainGrid.GetCurtainCells().ToList();
            //if (panels.Count != cells.Count)
            //    return null;

            //for (int i = 0; i < panels.Count; i++)
            //{
            //    FamilyInstance panel = panels[i] as FamilyInstance;
            //    if (panel == null || panel.get_BoundingBox(null) == null)
            //        continue;

            //    foreach (PolyCurve pc in cells[i].CurveLoops.FromRevit())
            //    {
            //        if (panel.Category.Id.IntegerValue == (int)Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
            //            result.Add(panel.DoorFromRevit(settings, refObjects));
            //        else
            //            result.Add(panel.WindowFromRevit(settings, refObjects));
            //    }
            //}
            
            return null;
        }

        /***************************************************/
    }
}
