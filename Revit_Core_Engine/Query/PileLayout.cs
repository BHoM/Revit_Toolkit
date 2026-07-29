using BH.Engine.Adapters.Revit;
using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;
using BH.oM.Physical.Elements;
using BH.oM.Spatial.Layouts;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Revit.Engine.Core
{
    public static partial class Query
    {
        [Description("Builds an ExplicitLayout of pile positions in the oriented cap coordinate system.")]
        public static ExplicitLayout PileFoundationLayout(this PileFoundation pileFoundation, Polyline orientedOutline, RevitSettings settings = null)
        {
            settings = settings.DefaultIfNull();

            Polyline outline = pileFoundation.PileCap.PadFoundationOutline();
            (Vector translation, double rotation) = outline.TransformToOriginInXY();
            if (translation == null || double.IsNaN(rotation))
                return null;

            List<Point> points = new List<Point>();
            foreach (Pile pile in pileFoundation.Piles)
            {
                Line line = pile.Location as Line;
                if (line == null)
                    return null;

                Point basePt = line.Start.Z < line.End.Z ? line.Start : line.End;
                Point local = basePt.Translate(translation).Rotate(new Point(), Vector.ZAxis, rotation);
                points.Add(new Point { X = local.X, Y = local.Y, Z = 0 });
            }

            return new ExplicitLayout(points);
        }
    }
}
