/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.Physical.Elements;
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Extracts the panels from a Revit curtain grid and returns them in a form of BHoM IOpenings.")]
        [Input("curtainGrid", "Revit curtain grid to extract the panels from.")]
        [Input("document", "Revit document, to which the curtain grid belongs.")]
        [Input("settings", "Revit adapter settings to be used while performing the query.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("panels", "Panels extracted from the input Revit curtain grid and converted to BHoM IOpenings.")]
        public static List<IOpening> CurtainPanels(this CurtainGrid curtainGrid, Document document, RevitSettings settings = null, Dictionary<string, List<IBHoMObject>> refObjects = null)
        {
            if (curtainGrid == null)
                return null;

            List<IOpening> result = new List<IOpening>();
            List<Element> panels = curtainGrid.GetPanelIds().Select(x => document.GetElement(x)).ToList();
            List<CurtainCell> cells = curtainGrid.GetCurtainCells().ToList();
            if (panels.Count != cells.Count)
                return null;

            for (int i = 0; i < panels.Count; i++)
            {
                FamilyInstance panel = panels[i] as FamilyInstance;
                if (panel == null)
                    continue;

                if (panel.get_BoundingBox(null) == null)
                {
                    ElementId hostPanelId = (panel as Panel)?.FindHostPanel();
                    if (hostPanelId == null || document.GetElement(hostPanelId)?.get_BoundingBox(null) == null)
                        continue;
                }

                foreach (PolyCurve pc in cells[i].CurveLoops.FromRevit())
                {
                    if (panel.Category.Id.IntegerValue == (int)Autodesk.Revit.DB.BuiltInCategory.OST_Doors)
                        result.Add(panel.DoorFromRevit(BH.Engine.Geometry.Create.PlanarSurface(pc), settings, refObjects));
                    else
                        result.Add(panel.WindowFromRevit(BH.Engine.Geometry.Create.PlanarSurface(pc), settings, refObjects));
                }
            }
            
            return result;
        }

        /***************************************************/
    }
}

