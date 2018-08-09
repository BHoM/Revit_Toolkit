using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Revit;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static Curve ToRevit(this ICurve curve, PushSettings pushSettings = null)
        {
            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            if (curve is oM.Geometry.Line)
            {
                oM.Geometry.Line aLine = curve as oM.Geometry.Line;
                return Autodesk.Revit.DB.Line.CreateBound(ToRevit(aLine.Start, pushSettings), ToRevit(aLine.End, pushSettings));
            }

            if (curve is oM.Geometry.Arc)
            {
                oM.Geometry.Arc aArc = curve as oM.Geometry.Arc;
                double radius = pushSettings.ConvertUnits ? UnitUtils.ConvertToInternalUnits(aArc.Radius, DisplayUnitType.DUT_METERS) : aArc.Radius;
                return Autodesk.Revit.DB.Arc.Create(ToRevit(aArc.CoordinateSystem, pushSettings), radius, aArc.StartAngle, aArc.EndAngle);
            }

            if (curve is NurbCurve)
            {
                NurbCurve aNurbCurve = curve as NurbCurve;
                return NurbSpline.Create(HermiteSpline.Create(aNurbCurve.ControlPoints.Cast<oM.Geometry.Point>().ToList().ConvertAll(x => ToRevit(x, pushSettings)), false));
            }

            if (curve is oM.Geometry.Ellipse)
            {
                oM.Geometry.Ellipse aEllipse = curve as oM.Geometry.Ellipse;
                return Autodesk.Revit.DB.Ellipse.CreateCurve(ToRevit(aEllipse.Centre, pushSettings), aEllipse.Radius1, aEllipse.Radius2, ToRevit(aEllipse.Axis1, pushSettings), ToRevit(aEllipse.Axis2, pushSettings), 0, 1);
            }

            return null;
        }

        /***************************************************/

        internal static CurveArray ToRevit(this PolyCurve polyCurve, PushSettings pushSettings = null)
        {
            if (polyCurve == null)
                return null;

            if (pushSettings == null)
                pushSettings = PushSettings.Default;

            CurveArray aCurveArray = new CurveArray();
            foreach (ICurve aICurve in polyCurve.Curves)
                aCurveArray.Append(aICurve.ToRevit(pushSettings));

            return aCurveArray;
        }

        /***************************************************/
    }
}