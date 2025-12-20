/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
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

        [Description("Extracts edge loops of a given curtain cell.")]
        [Input("curtainCell", "Curtain cell to query for edge loops.")]
        [Output("edgeLoops", "Edge loops extracted from the input curtain cell.")]
        public static List<PolyCurve> EdgeLoops(this FamilyInstance curtainCell)
        {
            HostObject curtainHost = curtainCell.Host as HostObject;
            if (curtainHost == null)
                return null;

            foreach (CurtainGrid cg in curtainHost.ICurtainGrids())
            {
                List<ElementId> ids = cg.GetPanelIds().ToList();
                List<CurtainCell> cells = cg.GetCurtainCells().ToList();
                for (int i = 0; i < ids.Count; i++)
                {
                    if (ids[i].IntegerValue == curtainCell.Id.IntegerValue)
                    {
                        if (!cells[i].HasValidLocation())
                            return null;

                        // Collapse nonlinear edges of a cell to lines - valid because mullions are linear anyways
                        List<PolyCurve> outlines = new List<PolyCurve>();
                        foreach (CurveArray array in cells[i].CurveLoops)
                        {
                            PolyCurve outline = new PolyCurve();
                            foreach (Curve curve in array)
                            {
                                outline.Curves.Add(new BH.oM.Geometry.Line { Start = curve.GetEndPoint(0).PointFromRevit(), End = curve.GetEndPoint(1).PointFromRevit() });
                            }

                            outlines.Add(outline);
                        }

                        return outlines;
                    }
                }
            }

            return new List<PolyCurve>();
        }

        /***************************************************/
    }
}


