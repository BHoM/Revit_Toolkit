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
using BH.oM.Base.Attributes;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Returns the 3D location point of a given Revit element. The method determines the most appropriate point based on the element type, such as the spatial element calculation point, the element's location point, the midpoint of its location curve, or the center of its bounding box.")]
        [Input("element", "The Revit element from which to extract the location point.")]
        [Input("useRoomCalculationPoint", "If true and the element is a FamilyInstance with a spatial element calculation point, use that point as the location.")]
        [Output("locationPoint", "The 3D point representing the location of the input Revit element")]
        public static XYZ LocationPoint(this Element element, bool useRoomCalculationPoint = false)
        {
            if (element == null)
                return null;

            XYZ locationPoint = null;

            // Handle FamilyInstance with Room Calculation Point
            if (element is FamilyInstance fi)
            {
                if (useRoomCalculationPoint && fi.HasSpatialElementCalculationPoint)
                    locationPoint = fi.GetSpatialElementCalculationPoint();
            }

            // Handle LocationPoint
            if (locationPoint == null && element.Location is LocationPoint lp)
            {
                locationPoint = lp?.Point;
            }
            // Handle LocationCurve
            else if (locationPoint == null && element.Location is LocationCurve lc)
            {
                Curve curve = lc?.Curve;
                locationPoint = curve?.Evaluate(0.5, true); // Midpoint of the curve
            }

            // Fallback to bounding box center
            if (locationPoint == null)
            {
                BoundingBoxXYZ bbox = element.PhysicalBounds();
                if (bbox != null)
                    locationPoint = (bbox.Max + bbox.Min) / 2;
            }

            return locationPoint;
        }

        /***************************************************/
    }
}
