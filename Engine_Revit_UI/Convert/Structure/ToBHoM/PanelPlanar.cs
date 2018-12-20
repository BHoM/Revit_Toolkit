/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Structure.Elements;
using BH.oM.Structure.Properties.Surface;
using BHS = BH.Engine.Structure;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Wall wall, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PanelPlanar> aResult = pullSettings.FindRefObjects<PanelPlanar>(wall.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

            ISurfaceProperty aProperty2D = wall.WallType.ToBHoMSurfaceProperty(pullSettings);
            
            List<oM.Geometry.ICurve> outlines = wall.Outlines(pullSettings);
            aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structure.Elements.Edge>(), Openings = new List<oM.Structure.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, wall) as PanelPlanar;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, wall, pullSettings.ConvertUnits) as PanelPlanar;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                aResult[i] = panel;
            }

            return aResult;
        }

        /***************************************************/

        internal static List<PanelPlanar> ToBHoMPanelPlanar(this Floor floor, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PanelPlanar> aResult = pullSettings.FindRefObjects<PanelPlanar>(floor.Id.IntegerValue);
            if (aResult != null && aResult.Count > 0)
                return aResult;

            ISurfaceProperty aProperty2D = aProperty2D = floor.FloorType.ToBHoMSurfaceProperty(pullSettings);

            List<oM.Geometry.ICurve> outlines = floor.Outlines(pullSettings);
            aResult = outlines != null ? BHS.Create.PanelPlanar(outlines) : new List<PanelPlanar> { new PanelPlanar { ExternalEdges = new List<oM.Structure.Elements.Edge>(), Openings = new List<oM.Structure.Elements.Opening>(), Property = aProperty2D } };

            for (int i = 0; i < aResult.Count; i++)
            {
                PanelPlanar panel = aResult[i] as PanelPlanar;
                panel.Property = aProperty2D;

                panel = Modify.SetIdentifiers(panel, floor) as PanelPlanar;
                if (pullSettings.CopyCustomData)
                    panel = Modify.SetCustomData(panel, floor, pullSettings.ConvertUnits) as PanelPlanar;

                pullSettings.RefObjects = pullSettings.RefObjects.AppendRefObjects(panel);

                aResult[i] = panel;
            }

            return aResult;
        }

        /***************************************************/
    }
}