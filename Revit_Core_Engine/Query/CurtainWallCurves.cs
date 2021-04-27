/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Geometry;
using BH.oM.Dimensional;
using BH.Engine.Facade;
using System;
using System.Collections.Generic;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {

        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static List<ICurve> CurtainWallCurves(this Wall wall)
        {
            List<ICurve> result = new List<ICurve>();
            List<IElement1D> allCrvs = new List<IElement1D>();

            PullGeometryConfig geometryConfig = new PullGeometryConfig();
            Options geometryOptions = Create.Options(Autodesk.Revit.DB.ViewDetailLevel.Fine, geometryConfig.IncludeNonVisible = true, false);
            List<Curve> objs = wall.Curves(geometryOptions);
            foreach (Curve crv in objs)
            {
                allCrvs.Add(crv.IFromRevit());
            }
            
            //Get only external curves from all curves
            foreach (ICurve crv in allCrvs)
            {
                List<IElement1D> adjElems = crv.AdjacentElements(allCrvs);
                if (adjElems.Count == 1)
                    result.Add(crv);
            }

            return result;
        }

        /***************************************************/

    }
}
