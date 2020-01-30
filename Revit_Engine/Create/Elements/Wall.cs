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
        /****              Public methods               ****/
        /***************************************************/

        [Description("Creates Wall object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("locationCurve", "Location curve of the wall (bottom edge)")]
        [Input("height", "Wall height")]
        [Output("Wall")]
        [Deprecated("2.3", "Architecture Wall has been replaced by BH.oM.Physical.Elements.Wall as part of migration to combined physical namespace")]
        public static Wall Wall(Object2DProperties object2DProperties, ICurve locationCurve, double height)
        {
            if (object2DProperties == null || locationCurve == null || height <= 0)
                return null;

            if (Geometry.Query.IIsClosed(locationCurve))
                return null;

            Point point1 = Geometry.Query.IStartPoint(locationCurve);
            Point point2 = Geometry.Query.IEndPoint(locationCurve);

            ICurve curve = Geometry.Modify.ITranslate(locationCurve, Geometry.Create.Vector(0, 0, height));

            Line line1 = Geometry.Create.Line(Geometry.Query.IEndPoint(locationCurve), Geometry.Query.IStartPoint(curve));
            Line line2 = Geometry.Create.Line(Geometry.Query.IEndPoint(curve), Geometry.Query.IStartPoint(locationCurve));

            PolyCurve polycurve = Geometry.Create.PolyCurve(new ICurve[] { locationCurve, line1, curve, line2 });

            Wall wall = new Wall()
            {
                Properties = object2DProperties,
                Surface = Geometry.Create.PlanarSurface(polycurve)
            };

            return wall;
        }

        /***************************************************/

        [Description("Creates Wall object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("edges", "Edges of Wall (profile)")]
        [Output("Wall")]
        [Deprecated("2.3", "Architecture Wall has been replaced by BH.oM.Physical.Elements.Wall as part of migration to combined physical namespace")]
        public static Wall Wall(Object2DProperties object2DProperties, ICurve edges, IEnumerable<ICurve> internalEdges = null)
        {
            if (object2DProperties == null || edges == null)
                return null;

            PlanarSurface planarSrf = Geometry.Create.PlanarSurface(edges);
            if (planarSrf == null)
                return null;

            Wall wall = new Wall()
            {
                Properties = object2DProperties,
                Surface = planarSrf
            };

            return wall;
        }

        /***************************************************/
    }
}


