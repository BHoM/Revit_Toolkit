/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<Curve> ToRevitCurveList(this oM.Environment.Elements.Panel panel)
        {
            if (panel == null || panel.ExternalEdges == null)
                return null;

            List<Curve> result = new List<Curve>();
            foreach (oM.Environment.Elements.Edge edge in panel.ExternalEdges)
            {
                List<Curve> curveList = ToRevitCurveList(edge.Curve);
                if (curveList != null && curveList.Count > 0)
                    result.AddRange(curveList);
            }
            return result;
        }

        /***************************************************/

        public static List<Curve> ToRevitCurveList(this ICurve curve)
        {
            if (curve == null)
                return null;

            List<Curve> result = new List<Curve>();
            if (curve is oM.Geometry.Arc)
                result.Add(curve.ToRevit());
            if (curve is oM.Geometry.Ellipse)
                result.Add(curve.ToRevit());
            else if (curve is Circle)
                result.Add(curve.ToRevit());
            else if (curve is oM.Geometry.Line)
                result.Add(curve.ToRevit());
            else if (curve is NurbsCurve)
                result.Add(curve.ToRevit());
            else if (curve is Polyline)
            {
                List<ICurve> curves = Query.Curves((Polyline)curve);
                if (curves == null)
                    return null;

                curves.ForEach(x => result.Add(x.ToRevit()));
            }
            else if (curve is PolyCurve)
            {
                PolyCurve polycurve = (PolyCurve)curve;
                if (polycurve.Curves == null)
                    return null;

                foreach (ICurve crv in polycurve.Curves)
                {
                    List<Curve> curves = crv.ToRevitCurveList();
                    if (curves != null && curves.Count > 0)
                    {
                        result.AddRange(curves);
                    }
                }
            }

            return result;
        }

        /***************************************************/
    }
}