/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Structure;
using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Finds the plane of a truss element. Returns null if the truss is not planar.")]
        [Input("truss", "Truss element to query for its plane.")]
        [Input("settings", "Revit adapter settings.")]
        [Output("plane", "Plane of the input truss. Null if the truss is nonplanar.")]
        public static BH.oM.Geometry.Plane TrussPlane(this Truss truss, RevitSettings settings = null)
        {
            if (truss == null)
            {
                BH.Engine.Base.Compute.RecordError("Extraction of truss plane failed because the queried truss cannot be null.");
                return null;
            }

            settings = settings.DefaultIfNull();

            List<BH.oM.Geometry.Point> points = new List<oM.Geometry.Point>();
            foreach (Curve curve in truss.Curves)
            {
                points.AddRange(curve.Tessellate().Select(x => x.PointFromRevit()));
            }

            if (!points.IsCoplanar(settings.DistanceTolerance))
            {
                BH.Engine.Base.Compute.RecordError("Extraction of truss plane failed because the truss is not planar.");
                return null;
            }

            return points.FitPlane(settings.DistanceTolerance);
        }

        /***************************************************/
    }
}




