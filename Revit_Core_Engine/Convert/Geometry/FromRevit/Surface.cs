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
using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using System;
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

        [Description("Converts a Revit PlanarFace to BH.oM.Geometry.PlanarSurface.")]
        [Input("face", "Revit PlanarFace to be converted.")]
        [Output("surface", "BH.oM.Geometry.PlanarSurface resulting from converting the input Revit PlanarFace.")]
        public static oM.Geometry.PlanarSurface FromRevit(this PlanarFace face)
        {
            if (face == null)
                return null;

            IList<CurveLoop> crvLoops = face.GetEdgesAsCurveLoops();

            if (crvLoops.Count > 1)
            {
                IEnumerable<IEnumerable<oM.Geometry.Point>> loopPoints = crvLoops
                    .Select(loop => loop
                        .SelectMany(curve => curve.Tessellate()
                            .Select(point => point.PointFromRevit())));

                oM.Geometry.Polyline convexHull = BH.Engine.Geometry.Compute.ConvexHull(loopPoints.SelectMany(x => x).ToList());

                List<int> hullPointCounts = loopPoints.Select(points => points.Where(pnt => pnt.IsOnCurve(convexHull)).Count()).ToList();

                //Make sure the first loop has the most number of control points on the convex hull, so it will always be the main external boundary.
                crvLoops = crvLoops.OrderByDescending(loop => hullPointCounts[crvLoops.IndexOf(loop)]).ToList();
            }

            oM.Geometry.ICurve externalBoundary = crvLoops[0].FromRevit();
            List<oM.Geometry.ICurve> internalBoundary = crvLoops.Skip(1).Select(x => x.FromRevit() as oM.Geometry.ICurve).ToList();

            return new oM.Geometry.PlanarSurface(externalBoundary, internalBoundary);
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Converts a Revit Face to BH.oM.Geometry.ISurface.")]
        [Input("face", "Revit Face to be converted.")]
        [Output("surface", "BH.oM.Geometry.ISurface resulting from converting the input Revit Face.")]
        public static oM.Geometry.ISurface IFromRevit(this Face face)
        {
            return FromRevit(face as dynamic);
        }


        /***************************************************/
        /****              Fallback Methods             ****/
        /***************************************************/

        private static oM.Geometry.ISurface FromRevit(this Face face)
        {
            BH.Engine.Base.Compute.RecordError(String.Format("Revit face of type {0} could not be converted to BHoM due to a missing convert method.", face.GetType()));
            return null;
        }

        /***************************************************/
    }
}





