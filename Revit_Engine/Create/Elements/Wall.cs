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

using System.ComponentModel;

using BH.oM.Architecture.Elements;
using BH.oM.Reflection.Attributes;
using BH.oM.Common.Properties;
using BH.oM.Geometry;
using System.Collections.Generic;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Wall object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("locationCurve", "Location curve of the wall (bottom edge)")]
        [Input("height", "Wall height")]
        [Output("Wall")]
        public static Wall Wall(Object2DProperties object2DProperties, ICurve locationCurve, double height)
        {
            if (object2DProperties == null || locationCurve == null || height <= 0)
                return null;

            if (Geometry.Query.IIsClosed(locationCurve))
                return null;

            Point aPoint_1 = Geometry.Query.IStartPoint(locationCurve);
            Point aPoint_2 = Geometry.Query.IEndPoint(locationCurve);

            ICurve aICurve = Geometry.Modify.ITranslate(locationCurve, Geometry.Create.Vector(0, 0, height));

            Line aLine_1 = Geometry.Create.Line(Geometry.Query.IEndPoint(locationCurve), Geometry.Query.IStartPoint(aICurve));
            Line aLine_2 = Geometry.Create.Line(Geometry.Query.IEndPoint(aICurve), Geometry.Query.IStartPoint(locationCurve));

            PolyCurve aPolyCurve = Geometry.Create.PolyCurve(new ICurve[] { locationCurve, aLine_1, aICurve, aLine_2 });

            Wall aWall = new Wall()
            {
                Properties = object2DProperties,
                Surface = Geometry.Create.PlanarSurface(aPolyCurve)
            };

            return aWall;
        }

        /***************************************************/

        [Description("Creates Wall object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("edges", "Edges of Wall (profile)")]
        [Output("Wall")]
        public static Wall Wall(Object2DProperties object2DProperties, ICurve edges, IEnumerable<ICurve> internalEdges = null)
        {
            if (object2DProperties == null || edges == null)
                return null;

            PlanarSurface aPlanarSurface = Geometry.Create.PlanarSurface(edges);
            if (aPlanarSurface == null)
                return null;

            Wall aWall = new Wall()
            {
                Properties = object2DProperties,
                Surface = aPlanarSurface
            };

            return aWall;
        }

        /***************************************************/
    }
}

