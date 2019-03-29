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

        [Description("Returns the outer PolyCurves from list")]
        [Input("polyCurves", "PolyCurves")]
        [Output("PolyCurve")]
        public static List<PolyCurve> OuterPolyCurves(this IEnumerable<PolyCurve> polyCurves)
        {
            if (polyCurves == null || polyCurves.Count() == 0)
                return null;

            if (polyCurves.Count() == 1)
                return new List<PolyCurve>() { polyCurves.ElementAt(0) };

            List<Tuple<BoundingBox, List<int>>> aTupleList = polyCurves.ToList().ConvertAll(x => new Tuple<BoundingBox, List<int>>(Geometry.Query.Bounds(x), new List<int>()));
            for (int i = 0; i < aTupleList.Count; i++)
            {
                BoundingBox aBoundingBox_1 = aTupleList[i].Item1;
                for (int j = i + 1; j < aTupleList.Count; j++)
                {
                    BoundingBox aBoundingBox_2 = aTupleList[j].Item1;

                    if (Geometry.Query.IsContaining(aBoundingBox_2, aBoundingBox_1))
                        aTupleList[j].Item2.Add(i);
                    else if (Geometry.Query.IsContaining(aBoundingBox_1, aBoundingBox_2))
                        aTupleList[i].Item2.Add(j);
                }
            }

            List<PolyCurve> aPolyCurveList = new List<PolyCurve>();
            for (int i = 0; i < aTupleList.Count; i++)
            {
                if (aTupleList[i].Item2.Count > 0)
                    aPolyCurveList.Add(polyCurves.ElementAt(i));
            }


            return aPolyCurveList;
        }

        /***************************************************/
    }
}

