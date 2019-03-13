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

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Creates Wall object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("location", "Location curve")]
        [Input("height", "Wall height")]
        [Output("Wall")]
        public static Wall Wall(Object2DProperties object2DProperties, ICurve location, double height)
        {
            //TODO: Impelent Create Wall

            Wall aWall = new Wall()
            {
                Properties = object2DProperties
            };

            return aWall;
        }

        /***************************************************/

        [Description("Creates Wall object.")]
        [Input("object2DProperties", "Object2DProperties")]
        [Input("externalBoundary", "External Boundary of Wall")]
        [Output("Wall")]
        public static Wall Wall(Object2DProperties object2DProperties, ICurve externalBoundary)
        {
            if (object2DProperties == null || externalBoundary == null)
                return null;

            PlanarSurface aPlanarSurface = Geometry.Create.PlanarSurface(externalBoundary);
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

