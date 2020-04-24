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

using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static List<oM.Geometry.ICurve> FromRevit(this List<Curve> curves)
        {
            if (curves == null)
                return null;

            return curves.Select(c => c.IFromRevit()).ToList();
        }

        /***************************************************/

        public static List<oM.Geometry.ICurve> FromRevit(this CurveArray curveArray)
        {
            if (curveArray == null)
                return null;

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Curve curve in curveArray)
            {
                result.Add(curve.IFromRevit());
            }

            return result;
        }

        /***************************************************/

        public static List<oM.Geometry.ICurve> FromRevit(this EdgeArray edgeArray)
        {
            if (edgeArray == null)
                return null;

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Edge edge in edgeArray)
            {
                result.Add(edge.FromRevit());
            }

            return result;
        }

        /***************************************************/


        public static List<oM.Geometry.PolyCurve> FromRevit(this Sketch sketch)
        {
            if (sketch == null || sketch.Profile == null)
                return null;

            List<oM.Geometry.PolyCurve> result = new List<oM.Geometry.PolyCurve>();
            foreach (CurveArray curveArray in sketch.Profile)
            {
                result.Add(new oM.Geometry.PolyCurve { Curves = curveArray.FromRevit() });
            }

            return result;
        }

        /***************************************************/

        public static List<oM.Geometry.PolyCurve> FromRevit(this CurveArrArray curveArrArray)
        {
            if (curveArrArray == null)
                return null;

            List<oM.Geometry.PolyCurve> result = new List<oM.Geometry.PolyCurve>();
            foreach (CurveArray curveArray in curveArrArray)
            {
                result.Add(new oM.Geometry.PolyCurve { Curves = curveArray.FromRevit() });
            }

            return result;
        }

        /***************************************************/

        public static List<oM.Geometry.PolyCurve> FromRevit(this EdgeArrayArray edgeArrArray)
        {
            if (edgeArrArray == null)
                return null;

            List<oM.Geometry.PolyCurve> result = new List<oM.Geometry.PolyCurve>();
            foreach (EdgeArray edgeArray in edgeArrArray)
            {
                result.Add(new oM.Geometry.PolyCurve { Curves = edgeArray.FromRevit() });
            }

            return result;
        }

        /***************************************************/

    }
}
