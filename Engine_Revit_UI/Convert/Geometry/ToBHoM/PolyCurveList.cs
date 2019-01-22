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

using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<PolyCurve> ToBHoM(this Sketch sketch, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            SketchPlane aSketchPlane = sketch.SketchPlane;
            oM.Geometry.CoordinateSystem.Cartesian aCartesian = ToBHoM(aSketchPlane.GetPlane(), pullSettings);

            Location aLocation = sketch.Location;

            Vector aVector = BH.Engine.Geometry.Create.Vector(aCartesian.Origin);

            List<PolyCurve> aResult = new List<PolyCurve>();
            foreach (CurveArray aCurveArray in sketch.Profile)
            {
                PolyCurve aPolyCurve = BH.Engine.Geometry.Create.PolyCurve(ToBHoM(aCurveArray, pullSettings));

                aPolyCurve = BH.Engine.Geometry.Modify.Translate(aPolyCurve, aVector);

                aResult.Add(aPolyCurve);
            }
                
            return aResult;
        }

        /***************************************************/

        internal static List<PolyCurve> ToBHoM(this CurveArrArray curveArrArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PolyCurve> aResult = new List<PolyCurve>();
            foreach (CurveArray aCurveArray in curveArrArray)
                aResult.Add(BH.Engine.Geometry.Create.PolyCurve(ToBHoM(aCurveArray, pullSettings)));

            return aResult;
        }

        /***************************************************/

        internal static List<PolyCurve> ToBHoM(this EdgeArrayArray edgeArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PolyCurve> aResult = new List<PolyCurve>();
            foreach (EdgeArray ea in edgeArray)
            {
                aResult.Add(BH.Engine.Geometry.Create.PolyCurve(ea.ToBHoM(pullSettings)));
            }
            return aResult;
        }

        /***************************************************/



        /***************************************************/
    }
}