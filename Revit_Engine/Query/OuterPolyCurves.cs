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

using System.ComponentModel;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Environment.Elements;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Returns the outer PolyCurves from list. QUICK CHECK (Centroid)")]
        [Input("polyCurves", "PolyCurves")]
        [Output("PolyCurves")]
        public static List<PolyCurve> OuterPolyCurves(this IEnumerable<PolyCurve> polyCurves)
        {
            if (polyCurves == null || polyCurves.Count() == 0)
                return null;

            if (polyCurves.Count() == 1)
                return new List<PolyCurve>() { polyCurves.ElementAt(0) };

            List<PolyCurve> aResult = new List<PolyCurve>();

            List<PolyCurve> aPolyCurveList = polyCurves.ToList();
            aPolyCurveList.Sort((x, y) => Geometry.Query.Area(x).CompareTo(Geometry.Query.Area(x)));

            while (aPolyCurveList.Count > 0)
            {
                PolyCurve aPolyCurve = aPolyCurveList[0];

                Point aPoint = Geometry.Query.Centroid(aPolyCurve);

                bool aExternal = true;

                for (int i = 1; i < aPolyCurveList.Count; i++)
                {
                    if (Geometry.Query.IsContaining(aPolyCurveList[i], new List<Point>() { aPoint }))
                    {
                        aExternal = false;
                        break;
                    }
                }

                if (aExternal)
                    aResult.Add(aPolyCurve);

                aPolyCurveList.RemoveAt(0);
            }

            return aPolyCurveList;
        }

        /***************************************************/
    }
}

