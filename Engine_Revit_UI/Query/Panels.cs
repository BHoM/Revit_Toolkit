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
using BH.Engine.Environment;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<oM.Environment.Elements.Panel> Panels(this GeometryElement geometryElement, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> result = new List<oM.Environment.Elements.Panel>();
            foreach (GeometryObject geomObject in geometryElement)
            {
                Solid solid = geomObject as Solid;
                if (solid == null)
                    continue;

                PlanarFace planarFace = Query.Top(solid);
                if (planarFace == null)
                    continue;

                List<BH.oM.Environment.Elements.Panel> panels = planarFace.Panels(settings);
                if (panels == null || panels.Count < 1)
                    continue;

                result.AddRange(panels);
            }

            return result;
        }

        /***************************************************/

        public static List<oM.Environment.Elements.Panel> Panels(this PlanarFace planarFace, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            List<oM.Environment.Elements.Panel> result = new List<oM.Environment.Elements.Panel>();

            List<PolyCurve> polycurves = planarFace.PolyCurves(null, settings);

            foreach (PolyCurve polycurve in polycurves)
            {
                oM.Environment.Elements.Panel panel = BH.Engine.Environment.Create.Panel(externalEdges: polycurve.ToEdges());
                result.Add(panel);
            }

            return result;
        }

        /***************************************************/
    }
}
