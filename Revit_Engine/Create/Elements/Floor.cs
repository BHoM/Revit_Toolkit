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

using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.Architecture.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Common.Properties;
using BH.oM.Geometry;


namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Floor object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("edges", "External edges of Floor")]
        [Output("Floor")]
        [Deprecated("2.3", "Architecture Floor has been replaced by BH.oM.Physical.Elements.Floor as part of migration to combined physical namespace")]
        public static Floor Floor(Object2DProperties object2DProperties, ICurve edges, IEnumerable<ICurve> internalEdges = null)
        {
            if (object2DProperties == null || edges == null)
                return null;

            List<ICurve> aInternalCurveList = null;
            if(internalEdges != null && internalEdges.Count() > 0)
                aInternalCurveList = internalEdges.ToList().ConvertAll(x => x as ICurve);

            PlanarSurface aPlanarSurface = Geometry.Create.PlanarSurface(edges, aInternalCurveList);
            if (aPlanarSurface == null)
                return null;

            Floor aFloor = new Floor()
            {
                Properties = object2DProperties,
                Surface = aPlanarSurface
            };

            return aFloor;
        }

        /***************************************************/

    }
}

