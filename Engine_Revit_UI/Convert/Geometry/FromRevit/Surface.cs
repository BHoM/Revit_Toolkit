/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using Autodesk.Revit.DB.Analysis;
using BH.Engine.Geometry;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****               Public Methods              ****/
        /***************************************************/

        public static oM.Geometry.PlanarSurface FromRevit(this PlanarFace face)
        {
            if (face == null)
                return null;

            IList<CurveLoop> crvLoop = face.GetEdgesAsCurveLoops();
            oM.Geometry.ICurve externalBoundary = crvLoop[0].FromRevit();
            List<oM.Geometry.ICurve> internalBoundary = crvLoop.Skip(1).Select(x => x.FromRevit() as oM.Geometry.ICurve).ToList();                           

            return new oM.Geometry.PlanarSurface { ExternalBoundary = externalBoundary, InternalBoundaries = internalBoundary };
        }

        /***************************************************/

        //public static oM.Geometry.Loft FromRevit(this CylindricalFace face)
        //{
        //    if (face == null)
        //        return null;

        //    EdgeArrayArray edgeArrays = face.EdgeLoops;
        //    EdgeArray edgeArray = edgeArrays.get_Item(0);

        //    List<oM.Geometry.ICurve> edges = new List<oM.Geometry.ICurve>();

        //    foreach(Edge edge in edgeArray)
        //    {
        //        if(edge.AsCurve() is Arc)
        //        {
        //            edges.Add(edge.FromRevit());
        //        }
        //    }

        //    return new oM.Geometry.Loft { Curves = edges };
            

        //    //CylindricalSurface cylinder = face.GetSurface() as CylindricalSurface;
        //    //oM.Geometry.Point start = new oM.Geometry.Point { X = cylinder.Origin.X, Y = cylinder.Origin.Y, Z = cylinder.Origin.Z };
        //    //oM.Geometry.Vector axis = new oM.Geometry.Vector { X = cylinder.Axis.X, Y = cylinder.Axis.Y, Z = cylinder.Axis.Z };
        //    //double radius = cylinder.Radius;
        //    //double height = (face.GetBoundingBox()).Max.V;
        //    //oM.Geometry.Line centreLine = new oM.Geometry.Line { Start = start, End = start + (height * axis) };

        //    //return new oM.Geometry.Pipe { Centreline = centreLine, Radius = radius, Capped = true };
        //}

        /***************************************************/
        /****             Interface Methods             ****/
        /***************************************************/

        public static oM.Geometry.ISurface IFromRevit(this Face face)
        {
            return FromRevit(face as dynamic);                        
        }        

        /***************************************************/
        /****              Fallback Methods             ****/
        /***************************************************/

        public static oM.Geometry.ISurface FromRevit(this Face face)
        {
            BH.Engine.Reflection.Compute.RecordError(String.Format("Revit face of type {0} could not be converted to BHoM due to a missing convert method.", face.GetType()));
            return null;
        }

        /***************************************************/
    }
}

