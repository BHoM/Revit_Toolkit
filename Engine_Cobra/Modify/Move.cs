using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.DataManipulation.Queries;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using BH.oM.Adapters.Revit.Elements;
using BH.oM.Structure.Elements;
using BH.Engine.Structure;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Location Move(this Location location, Vector vector, PushSettings pushSettings = null)
        {
            if (location == null || vector == null)
                return null;

            if (location.IsReadOnly)
                return null;

            XYZ aXYZ = vector.ToRevit(pushSettings);
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

            Curve aCurve = iCurve.ToRevit(pushSettings);
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

        public static Location Move(this Location location, IBHoMObject iBHoMObject, PushSettings pullSettings = null)
        {
            if (location == null || iBHoMObject == null)
                return null;
           
            return Move(location, iBHoMObject as dynamic, pullSettings);
        }

        /***************************************************/

        public static Location Move(this Location location, GenericObject genericObject, PushSettings pullSettings = null)
        {
            if (location == null || genericObject == null)
                return null;

            return Move(location, genericObject.Location, pullSettings);
        }

        /***************************************************/

        public static Location Move(this Location location, FramingElement framingElement, PushSettings pullSettings = null)
        {
            if (location == null || framingElement == null)
                return null;

            return Move(location, framingElement.LocationCurve, pullSettings);
        }

        /***************************************************/ 
    }
}