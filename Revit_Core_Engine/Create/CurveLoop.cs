/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Create
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates a CurveLoop representing a polyline defined by a given collection of points.")]
        [Input("points", "Collection of points defining the CurveLoop to be created.")]
        [Input("close", "If true, the method will ensure the returned CurveLoop is closed.")]
        [Output("loop", "CurveLoop representing a polyline defined by the input collection of points.")]
        public static CurveLoop CurveLoop(List<XYZ> points, bool close = true)
        {
            if (points == null)
                return null;

            CurveLoop cl = new CurveLoop();
            for (int i = 1; i < points.Count; i++)
            {
                cl.Append(Line.CreateBound(points[i - 1], points[i]));
            }

            if (close)
                cl.Append(Line.CreateBound(points[points.Count - 1], points[0]));

            return cl;
        }

        /***************************************************/
    }
}

