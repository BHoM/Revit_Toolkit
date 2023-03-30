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
using BH.oM.Adapters.Revit;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the projection of an XYZ point on the datum XY plane.")]
        [Input("point", "An XYZ point from which to get a projection on the XY plane.")]
        [Output("point", "The projection of an XYZ point on the XY plane.")]
        public static XYZ ToXY(this XYZ point)
        {
            return new XYZ(point.X, point.Y, 0);
        }

        /***************************************************/

        [Description("Returns the projection of a line on the datum XY plane.")]
        [Input("line", "A line from which to get a projection on the XY plane.")]
        [Output("line", "The projection of a line on the XY plane.")]
        public static Line ToXY(this Line line)
        {
            XYZ p0 = line.GetEndPoint(0).ToXY();
            XYZ p1 = line.GetEndPoint(1).ToXY();

            if (p0.DistanceTo(p1) <= Tolerance.ShortCurve)
            {
                return null;
            }

            return Line.CreateBound(p0, p1);
        }

        /***************************************************/

        [Description("Returns the projection of a planar curve on the datum XY plane.")]
        [Input("planarCurve", "A planar curve from which to get a projection on the XY plane.")]
        [Output("planarCurve", "The projection of a planar curve on the XY plane.")]
        public static Curve ToXY(this Curve planarCurve)
        {
            double zDiff = planarCurve.Evaluate(0.5, false).Z;
            Transform tr = Transform.CreateTranslation(new XYZ(0, 0, -zDiff));
            return planarCurve.CreateTransformed(tr);
        }

        /***************************************************/

        [Description("Returns the projection of a list of planar curves on the datum XY plane.")]
        [Input("planarCurves", "A list of planar curves from which to get a projection on the XY plane.")]
        [Output("planarCurves", "The projection of a list of planar curves on the XY plane.")]
        public static List<Curve> ToXY(this List<Curve> planarCurves)
        {
            return planarCurves.Select(x => x.ToXY()).ToList();
        }

        /***************************************************/
    }
}


