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
using BH.oM.Reflection.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        [Description("Converts a list of Revit Curves to list of BH.oM.Geometry.ICurves.")]
        [Input("curves", "List of Revit Curves to be converted.")]
        [Output("curves", "List of BH.oM.Geometry.ICurves resulting from converting the input list of Revit Curves.")]
        public static List<oM.Geometry.ICurve> FromRevit(this List<Curve> curves)
        {
            if (curves == null)
                return null;

            return curves.Select(c => c.IFromRevit()).ToList();
        }

        /***************************************************/

        [Description("Converts a Revit CurveArray to list of BH.oM.Geometry.ICurves.")]
        [Input("curveArray", "Revit CurveArray to be converted.")]
        [Output("curves", "List of BH.oM.Geometry.ICurves resulting from converting the input Revit CurveArray.")]
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

        [Description("Converts a Revit EdgeArray to list of BH.oM.Geometry.ICurves.")]
        [Input("edgeArray", "Revit EdgeArray to be converted.")]
        [Output("curves", "List of BH.oM.Geometry.ICurves resulting from converting the input Revit EdgeArray.")]
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

        [Description("Converts a Revit Sketch to list of BH.oM.Geometry.PolyCurves.")]
        [Input("sketch", "Revit Sketch to be converted.")]
        [Output("curves", "List of BH.oM.Geometry.PolyCurves resulting from converting the input Revit Sketch.")]
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

        [Description("Converts a Revit CurveArrArray to list of BH.oM.Geometry.PolyCurves.")]
        [Input("curveArrArray", "Revit CurveArrArray to be converted.")]
        [Output("curves", "List of BH.oM.Geometry.PolyCurves resulting from converting the input Revit CurveArrArray.")]
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

        [Description("Converts a Revit EdgeArrayArray to list of BH.oM.Geometry.PolyCurves.")]
        [Input("edgeArrArray", "Revit EdgeArrayArray to be converted.")]
        [Output("curves", "List of BH.oM.Geometry.PolyCurves resulting from converting the input Revit EdgeArrayArray.")]
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

