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
using BH.Engine.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Curve ToRevitCurve(this ICurve curve, PushSettings pushSettings = null)
        {
            pushSettings = pushSettings.DefaultIfNull();

            if (curve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = curve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aLine.Start, pushSettings), ToRevit(aLine.End, pushSettings));
            }

            if (curve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = curve as oM.Geometry.Arc;
                double radius = pushSettings.ConvertUnits ? UnitUtils.ConvertToInternalUnits(aArc.Radius, DisplayUnitType.DUT_METERS) : aArc.Radius;
                return Autodesk.Revit.DB.Arc.Create(ToRevit(aArc.CoordinateSystem, pushSettings), radius, aArc.StartAngle, aArc.EndAngle);
            }

            if (curve is NurbsCurve)
            {
                NurbsCurve aNurbCurve = curve as NurbsCurve;
                List<double> knots = aNurbCurve.Knots.ToList();
                knots.Insert(0, knots[0]);
                knots.Add(knots[knots.Count - 1]);
                return NurbSpline.CreateCurve(aNurbCurve.Degree(), knots, aNurbCurve.ControlPoints.Select(x => x.ToRevit(pushSettings)).ToList(), aNurbCurve.Weights);
                //return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevit(x, pushSettings)), false));
            }

            if (curve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = curve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevit(aEllipse.Centre, pushSettings), aEllipse.Radius1, aEllipse.Radius2, ToRevit(aEllipse.Axis1, pushSettings), ToRevit(aEllipse.Axis2, pushSettings), 0, 1);
            }

            if (curve is Polyline)
            {
                Polyline aPolyline = curve as Polyline;
                if (aPolyline.ControlPoints.Count == 2)
                    return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aPolyline.ControlPoints[0], pushSettings), ToRevit(aPolyline.ControlPoints[1], pushSettings));
            }

            return null;
        }

        /***************************************************/
    }
}