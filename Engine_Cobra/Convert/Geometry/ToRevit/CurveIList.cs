using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static IList<Curve> ToRevitCurveIList(this ICurve curve, PushSettings pushSettings = null)
        {
            if (curve == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            List<Curve> aResult = new List<Curve>();
            if (curve is oM.Geometry.Arc)
                aResult.Add(curve.ToRevit(pushSettings));
            if (curve is oM.Geometry.Ellipse)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is Circle)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is oM.Geometry.Line)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is NurbCurve)
                aResult.Add(curve.ToRevit(pushSettings));
            else if (curve is Polyline)
            {
                Polyline aPolyline = (Polyline)curve;

                if (aPolyline.ControlPoints != null && aPolyline.ControlPoints.Count > 1)
                {
                    for (int i = 1; i < aPolyline.ControlPoints.Count; i++)
                        aResult.Add(BH.Engine.Geometry.Create.Line(aPolyline.ControlPoints[i - 1], aPolyline.ControlPoints[i]).ToRevit(pushSettings));

                    aResult.Add(BH.Engine.Geometry.Create.Line(aPolyline.ControlPoints[aPolyline.ControlPoints.Count - 1], aPolyline.ControlPoints[0]).ToRevit(pushSettings));
                }

            }
            else if (curve is PolyCurve)
            {
                ((PolyCurve)curve).Curves.ToList().ForEach(x => aResult.Add(x.ToRevit(pushSettings)));
            }

            return aResult;
        }

        /***************************************************/
    }
}