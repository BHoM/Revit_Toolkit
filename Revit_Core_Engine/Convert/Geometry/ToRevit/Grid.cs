/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Converts BH.oM.Spatial.SettingOut.Grid to a Revit Grid or MultiSegmentGrid.")]
        [Input("grid", "BH.oM.Spatial.SettingOut.Grid to be converted.")]
        [Input("document", "Revit document, in which the output of the convert will be created.")]
        [Input("settings", "Revit adapter settings to be used while performing the convert.")]
        [Input("refObjects", "Optional, a collection of objects already processed in the current adapter action, stored to avoid processing the same object more than once.")]
        [Output("grid", "Revit Grid or MultiSegmentGrid resulting from converting the input BH.oM.Spatial.SettingOut.Grid.")]
        public static Element ToRevitGrid(this oM.Spatial.SettingOut.Grid grid, Document document, RevitSettings settings = null, Dictionary<Guid, List<int>> refObjects = null)
        {
            Element revitGrid = refObjects.GetValue<Grid>(document, grid.BHoM_Guid);
            if (revitGrid != null)
                return revitGrid;

            settings = settings.DefaultIfNull();

            if (BH.Engine.Geometry.Query.IIsClosed(grid.Curve))
            {
                BH.Engine.Base.Compute.RecordError("Element could not be created: Revit allows only open curve-based grids. BHoM_Guid: " + grid.BHoM_Guid);
                return null;
            }

            List<BH.oM.Geometry.ICurve> gridCurves = BH.Engine.Geometry.Query.ISubParts(grid.Curve).ToList();
            if (gridCurves.Count == 0)
                return null;
            else if (gridCurves.Count == 1)
            {
                Curve curve = grid.Curve.IToRevit();

                if (curve is Line)
                    revitGrid = Grid.Create(document, (Line)curve);
                else  if (curve is Arc)
                    revitGrid = Grid.Create(document, (Arc)curve);
                else
                {
                    BH.Engine.Base.Compute.RecordError("Element could not be created: Revit allows only line- and arc-based grids. BHoM_Guid: " + grid.BHoM_Guid);
                    return null;
                }
            }
            else
            {
                CurveLoop loop = new CurveLoop();
                foreach (BH.oM.Geometry.ICurve curve in gridCurves)
                {
                    Curve revitCurve = curve.IToRevit();
                    if (revitCurve is Line || revitCurve is Arc)
                        loop.Append(revitCurve);
                    else
                    {
                        BH.Engine.Base.Compute.RecordError("Element could not be created: Revit allows only line- and arc-based grids. BHoM_Guid: " + grid.BHoM_Guid);
                        return null;
                    }
                }
                
                Plane plane;
                try
                {
                    plane = loop.GetPlane();
                }
                catch
                {
                    BH.Engine.Base.Compute.RecordError("Grid curves need to be coplanar. BHoM_Guid: " + grid.BHoM_Guid);
                    return null;
                }

                SketchPlane sketchPlane = SketchPlane.Create(document, plane);
                ElementId gridTypeId = document.GetDefaultElementTypeId(ElementTypeGroup.GridType);
                ElementId gridId = MultiSegmentGrid.Create(document, gridTypeId, loop, sketchPlane.Id);
                revitGrid = document.GetElement(gridId);
            }

            revitGrid.CheckIfNullPush(grid);
            if (revitGrid == null)
                return null;

            try
            {
                revitGrid.Name = grid.Name;
            }
            catch
            {
                BH.Engine.Base.Compute.RecordWarning(String.Format("Grid name '{0}' was not unique, name '{1}' has been assigned instead. BHoM_Guid: {2}", grid.Name, revitGrid.Name, grid.BHoM_Guid));
            }

            // Copy parameters from BHoM object to Revit element
            revitGrid.CopyParameters(grid, settings);

            refObjects.AddOrReplace(grid, revitGrid);
            return revitGrid;
        }

        /***************************************************/
    }
}



