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

        internal static List<Curve> ToRevitCurveList(this oM.Environment.Elements.Panel panel, PushSettings pushSettings = null)
        {
            if (panel == null || panel.ExternalEdges == null)
                return null;

            List<Curve> aResult = new List<Curve>();
            foreach (oM.Environment.Elements.Edge aEdge in panel.ExternalEdges)
            {
                List<Curve> aCurveList = ToRevitCurveList(aEdge.Curve, pushSettings);
                if (aCurveList != null && aCurveList.Count > 0)
                    aResult.AddRange(aCurveList);
            }
            return aResult;
        }


        internal static List<Curve> ToRevitCurveList(this ICurve curve, PushSettings pushSettings = null)
        {
            if (curve == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            List<Curve> aResult = new List<Curve>();
            if (curve is oM.Geometry.Arc)
                aResult.Add(curve.ToRevit(pushSettings));
            if (curve is oM.Geometry.Ellipse)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is Circle)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is oM.Geometry.Line)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is NurbsCurve)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is Polyline)
            {
                List<ICurve> aCureList = Query.Curves((Polyline)curve);
                if (aCureList == null)
                    return null;

                aCureList.ForEach(x => aResult.Add(x.ToRevit(pushSettings)));
            }
            else if (curve is PolyCurve)
            {
                PolyCurve aPolyCurve = (PolyCurve)curve;
                if (aPolyCurve.Curves == null)
                    return null;

                foreach (ICurve aCurve in aPolyCurve.Curves)
                {
                    List<Curve> aCurveList = aCurve.ToRevitCurveList(pushSettings);
                    if (aCurveList != null && aCurveList.Count > 0)
                    {
                        aResult.AddRange(aCurveList);
                    }
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}