using BH.Engine.Geometry;
using BH.oM.Base.Attributes;
using BH.oM.Geometry;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        [Description("Computes translation and rotation to orient a Polyline to the origin in XY.")]
        [Input("outline", "Polyline to transform.")]
        [Output("result", "Centroid projected to XY and rotation angle in radians around Z.")]
        public static (Vector, double) TransformToOriginInXY(this Polyline outline)
        {
            Vector translation = null;
            double rotation = double.NaN;

            List<Point> pts = outline?.ControlPoints;
            if (pts == null || pts.Count < 3)
                return (translation, rotation);

            int n = pts.Count;
            if (n >= 3 && (pts[n - 1] - pts[0]).Length() <= BH.oM.Geometry.Tolerance.Distance)
                n--;

            if (n < 3)
                return (translation, rotation);

            Point centroid = outline.Centroid();
            if (centroid == null)
                return (translation, rotation);

            translation = (centroid - new Point()).ProjectToXY();
            Vector longestEdge = outline.LongestEdgeInXY();
            if (longestEdge != null)
                rotation = Vector.XAxis.SignedAngle(longestEdge, Vector.ZAxis);

            return (translation, rotation);
        }
    }
}
