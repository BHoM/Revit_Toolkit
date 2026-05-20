using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Modify
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        [Description("Transforms a given Polyline into a standardized orientation by translating it to the origin and rotating it in XY plane.")]
        [Input("outline", "Polyline to be oriented to the origin.")]
        [Output("orientedOutline", "Oriented polyline translated to the origin and rotated in XY plane.")]
        public static Polyline OrientToOrigin(this Polyline outline)
        {
            (Vector translation, double rotation) = outline.TransformToOriginInXY();
            if (translation == null || double.IsNaN(rotation))
                return null;

            Vector moveToOrigin = (new Point() - outline.Centroid()).ProjectToXY();
            return outline.Translate(moveToOrigin).Rotate(new Point(), Vector.ZAxis, rotation);
        }
    }
}
