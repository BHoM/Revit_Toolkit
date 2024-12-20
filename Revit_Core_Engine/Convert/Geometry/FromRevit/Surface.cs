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
using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
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
        public static PlanarSurface FromRevit(this PlanarFace face)
        {
            if (face == null)
                return null;

            PolyCurve boundary = face.ExternalCurveLoop().FromRevit();
            BoundingBox boundaryBox = boundary.Bounds();

            PolyCurve[] loops = face.GetEdgesAsCurveLoops().Select(x => x.FromRevit()).ToArray();
            List<ICurve> openings = loops.Where(x => !x.Bounds().IsEqual(boundaryBox)).Cast<ICurve>().ToList();

            return new PlanarSurface(boundary, openings);
        }


        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        [Description("Converts a Revit Face to BH.oM.Geometry.ISurface.")]
        [Input("face", "Revit Face to be converted.")]
        [Output("surface", "BH.oM.Geometry.ISurface resulting from converting the input Revit Face.")]
        public static oM.Geometry.ISurface IFromRevit(this Autodesk.Revit.DB.Face face)
        {
            return FromRevit(face as dynamic);
        }


        /***************************************************/
        /****              Fallback Methods             ****/
        /***************************************************/

        private static oM.Geometry.ISurface FromRevit(this Autodesk.Revit.DB.Face face)
        {
            BH.Engine.Base.Compute.RecordError(String.Format("Revit face of type {0} could not be converted to BHoM due to a missing convert method.", face.GetType()));
            return null;
        }

        /***************************************************/
    }
}






