/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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

        public static Location Move(this Location location, Vector vector, PushSettings pushSettings = null)
        {
            if (location == null || vector == null)
                return null;

            if (location.IsReadOnly)
                return null;

            XYZ aXYZ = vector.ToRevit();
            if (aXYZ.GetLength() < Tolerance.MicroDistance)
                return location;

            if (location.Move(aXYZ))
                return location;
            else
                return null;
        }

        /***************************************************/

        public static LocationPoint Move(this LocationPoint locationPoint, oM.Geometry.Point point, PushSettings pushSettings = null)
        {
            if (locationPoint == null || point == null)
                return null;

            if (locationPoint.IsReadOnly)
                return null;

            XYZ aXYZ = point.ToRevit(pushSettings);

            if (aXYZ.IsAlmostEqualTo(locationPoint.Point, Tolerance.MicroDistance))
                return locationPoint;

            locationPoint.Point = aXYZ;
            return locationPoint;
        }

        /***************************************************/

        public static LocationCurve Move(this LocationCurve locationCurve, ICurve iCurve, PushSettings pushSettings = null)
        {
            if (locationCurve == null || iCurve == null)
                return null;

            if (locationCurve.IsReadOnly)
                return null;

            Curve aCurve = iCurve.ToRevitCurve();
            if (Query.IsSimilar(aCurve, locationCurve.Curve))
                return locationCurve;

            try
            {
                locationCurve.Curve = aCurve;
            }
            catch
            {

            }

            if (Query.IsSimilar(aCurve, locationCurve.Curve))
                return locationCurve;
            else
                return null;
        }

        /***************************************************/

        public static Location Move(this Location location, IGeometry geometry, PushSettings pushSettings = null)
        {
            if (location == null || geometry == null)
                return null;

            if (location.IsReadOnly)
                return null;

            if (geometry is Vector)
                return Move(location, (Vector)geometry, pushSettings);

            if (location is LocationPoint && geometry is oM.Geometry.Point)
                return Move((LocationPoint)location, (oM.Geometry.Point)geometry, pushSettings);

            if (location is LocationCurve&& geometry is ICurve)
                return Move((LocationCurve)location, (ICurve)geometry, pushSettings);

            return null;
        }

        /***************************************************/

        public static Location Move(this Location location, ModelInstance modelInstance, PushSettings pullSettings = null)
        {
            if (location == null || modelInstance == null)
                return null;

            return Move(location, modelInstance.Location, pullSettings);
        }

        /***************************************************/

        public static Location Move(this Element element, oM.Base.IBHoMObject BHoMObject, PushSettings pullSettings = null)
        {
            if (element.Location == null)
                return null;

            if (BHoMObject is BH.oM.Environment.Elements.Space)
                return Move(element, (BH.oM.Environment.Elements.Space)BHoMObject, pullSettings);

            return Move(element.Location, BHoMObject as dynamic, pullSettings);
        }

        public static Location Move(this Element element, BH.oM.Environment.Elements.Space space, PushSettings pullSettings = null)
        {
            if (element == null || space == null)
                return null;

            Level aLevel = Query.BottomLevel(space.Location.Z, element.Document);
            if (aLevel == null)
                return null;

            oM.Geometry.Point aPoint = BH.Engine.Geometry.Create.Point(space.Location.X, space.Location.Y, aLevel.Elevation);

            return Move(element.Location, aPoint, pullSettings);
        }

        /***************************************************/

        public static Location Move(this Location location, BH.oM.Physical.Elements.IFramingElement framingElement, PushSettings pullSettings = null)
        {
            if (location == null || framingElement == null)
                return null;

            return Move(location, framingElement.Location, pullSettings);
        }

        /***************************************************/ 
    }
}