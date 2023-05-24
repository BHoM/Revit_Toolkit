/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Gets the location point of a Revit element with additional options for dealing with curve-based objects.")]
        [Input("elem", "A generic Revit element.")]
        [Input("pointOnCurve", "A value to specify when the input element is curve-based, if we need its midpoint, lowest endpoint or highest endpoint.")]
        [Output("xyz", "The location point of a Revit element.")]
        public static XYZ LocationPoint(this Element elem, PointOnCurvePosition pointOnCurve)
        {
            XYZ ePoint = null;
            Location eLocation = elem.Location;

            if (eLocation is LocationPoint)
            {
                ePoint = (eLocation as LocationPoint).Point;
            }
            else if (eLocation is LocationCurve)
            {
                Curve eCurve = (eLocation as LocationCurve).Curve;

                if (pointOnCurve == PointOnCurvePosition.Middle)
                    ePoint = eCurve.Evaluate(0.5, true);
                else
                {
                    XYZ pnt1 = eCurve.Evaluate(0, true);
                    XYZ pnt2 = eCurve.Evaluate(1, true);

                    if (pointOnCurve == PointOnCurvePosition.Lowest)
                        ePoint = (pnt1.Z > pnt2.Z) ? pnt2 : pnt1;
                    else if (pointOnCurve == PointOnCurvePosition.Highest)
                        ePoint = (pnt1.Z > pnt2.Z) ? pnt1 : pnt2;
                }
            }

            return ePoint;
        }

        /***************************************************/
    }
}