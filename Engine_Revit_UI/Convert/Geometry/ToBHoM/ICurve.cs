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

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this Curve curve, PullSettings pullSettings = null)
        {
            if (curve == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            if (curve is Line)
                return BH.Engine.Geometry.Create.Line(ToBHoM(curve.GetEndPoint(0), pullSettings), ToBHoM(curve.GetEndPoint(1), pullSettings));

            if (curve is Arc)
            {
                Arc aArc = curve as Arc;
                double radius = pullSettings.ConvertUnits ? UnitUtils.ConvertFromInternalUnits(aArc.Radius, DisplayUnitType.DUT_METERS) : aArc.Radius;
                Plane plane = Plane.CreateByOriginAndBasis(aArc.Center, aArc.XDirection, aArc.YDirection);
                double startAngle = aArc.XDirection.AngleOnPlaneTo(aArc.GetEndPoint(0) - aArc.Center, aArc.Normal);
                double endAngle = aArc.XDirection.AngleOnPlaneTo(aArc.GetEndPoint(1) - aArc.Center, aArc.Normal);
                if (startAngle > endAngle)
                {
                    startAngle -= 2 * Math.PI;
                }
                return BH.Engine.Geometry.Create.Arc(ToBHoM(plane, pullSettings), radius, startAngle, endAngle);
            }
            
            return null;
        }

        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve, PullSettings pullSettings = null)
        {
            if (locationCurve == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoM(locationCurve.Curve, pullSettings);
        }

        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this Edge edge, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoM(edge.AsCurve(), pullSettings);
        }

        /***************************************************/
    }
}