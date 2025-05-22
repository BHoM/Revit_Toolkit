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
                locationPoint = curve.Evaluate(0.5, true); // Midpoint of the curve
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
