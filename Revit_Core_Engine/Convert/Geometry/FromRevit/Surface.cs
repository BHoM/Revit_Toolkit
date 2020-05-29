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

namespace BH.Revit.Engine.Core
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

            return new oM.Geometry.PlanarSurface(externalBoundary, internalBoundary);
        }        


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

