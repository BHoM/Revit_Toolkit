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

using BH.oM.Geometry;
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [DeprecatedAttribute("3.2", "This method is a unreliable duplicate of BH.Engine.Spatial.Compute.DistributeOutlines.")]
        [Description("Returns the PolyCurves from list that lie outside any other PolyCurve. QUICK CHECK (BOUNDING BOX).")]
        [Input("polyCurves", "Collection of PolyCurves to be queried for containment.")]
        [Output("outerPolyCurves")]
        public static List<PolyCurve> OuterPolyCurves(this IEnumerable<PolyCurve> polyCurves)
        {
            if (polyCurves == null || polyCurves.Count() == 0)
                return null;

            if (polyCurves.Count() == 1)
                return new List<PolyCurve>() { polyCurves.ElementAt(0) };

            List<PolyCurve> result = new List<PolyCurve>();

            List<PolyCurve> polycurves = polyCurves.ToList();

            polycurves.Sort((x, y) => Geometry.Query.Area(x).CompareTo(Geometry.Query.Area(y)));

            while (polycurves.Count > 0)
            {
                PolyCurve pcurve = polycurves[0];

                bool external = true;

                Point point = Geometry.Query.PointInRegion(pcurve);
                for (int i = 1; i < polycurves.Count; i++)
                {
                    if (Geometry.Query.IsContaining(polycurves[i], new List<Point>() { point }))
                    {
                        external = false;
                        break;
                    }
                }

                if (external)
                    result.Add(pcurve);

                polycurves.RemoveAt(0);
            }

            return result;
        }

        /***************************************************/
    }
}



