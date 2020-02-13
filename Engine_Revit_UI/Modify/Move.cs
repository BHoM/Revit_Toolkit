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

using Autodesk.Revit.DB;

using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Structure.Elements;

namespace BH.UI.Revit.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static Location Move(this Location location, Vector vector)
        {
            if (location == null || vector == null)
                return null;

            if (location.IsReadOnly)
                return null;

            XYZ xyz = vector.ToRevit();
            if (xyz.GetLength() < Tolerance.MicroDistance)
                return location;

            if (location.Move(xyz))
                return location;
            else
                return null;
        }

        /***************************************************/

        public static LocationPoint Move(this LocationPoint locationPoint, oM.Geometry.Point point)
        {
            if (locationPoint == null || point == null)
                return null;

            if (locationPoint.IsReadOnly)
                return null;

            XYZ xyz = point.ToRevit();

            if (xyz.IsAlmostEqualTo(locationPoint.Point, Tolerance.MicroDistance))
                return locationPoint;

            locationPoint.Point = xyz;
            return locationPoint;
        }

        /***************************************************/

        public static LocationCurve Move(this LocationCurve locationCurve, ICurve iCurve)
        {
            if (locationCurve == null || iCurve == null)
                return null;

            if (locationCurve.IsReadOnly)
                return null;

            Curve curve = iCurve.IToRevit();
            if (curve.IsSimilar(locationCurve.Curve))
                return locationCurve;

            try
            {
                locationCurve.Curve = curve;
            }
            catch
            {

            }

            if (curve.IsSimilar(locationCurve.Curve))
                return locationCurve;
            else
                return null;
        }

        /***************************************************/

        public static Location Move(this Location location, IGeometry geometry)
        {
            if (location == null || geometry == null)
                return null;

            if (location.IsReadOnly)
                return null;

            if (geometry is Vector)
                return location.Move((Vector)geometry);

            if (location is LocationPoint && geometry is oM.Geometry.Point)
                return ((LocationPoint)location).Move((oM.Geometry.Point)geometry);

            if (location is LocationCurve&& geometry is ICurve)
                return ((LocationCurve)location).Move((ICurve)geometry);

            return null;
        }

        /***************************************************/

        public static Location Move(this Location location, ModelInstance modelInstance)
        {
            if (location == null || modelInstance == null)
                return null;

            return location.Move(modelInstance.Location);
        }

        /***************************************************/

        public static Location Move(this Element element, oM.Base.IBHoMObject bhomObject)
        {
            if (element.Location == null)
                return null;

            if (bhomObject is BH.oM.Environment.Elements.Space)
                return element.Move((BH.oM.Environment.Elements.Space)bhomObject);

            return element.Location.Move(bhomObject as dynamic);
        }

        public static Location Move(this Element element, BH.oM.Environment.Elements.Space space)
        {
            if (element == null || space == null)
                return null;

            Level level = space.Location.Z.BottomLevel(element.Document);
            if (level == null)
                return null;

            oM.Geometry.Point point = BH.Engine.Geometry.Create.Point(space.Location.X, space.Location.Y, level.Elevation);

            return element.Location.Move(point);
        }

        /***************************************************/

        public static Location Move(this Location location, BH.oM.Physical.Elements.IFramingElement framingElement)
        {
            if (location == null || framingElement == null)
                return null;

            return location.Move(framingElement.Location);
        }

        /***************************************************/ 
    }
}
