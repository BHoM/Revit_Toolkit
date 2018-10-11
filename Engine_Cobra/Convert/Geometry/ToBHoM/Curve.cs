using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this Curve curve, PullSettings pullSettings = null)
        {
            if (curve == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            if (curve is Line)
                return BH.Engine.Geometry.Create.Line(ToBHoM(curve.GetEndPoint(0), pullSettings), ToBHoM(curve.GetEndPoint(1), pullSettings));

            if (curve is Arc)
            {
                Arc aArc = curve as Arc;
                double radius = pullSettings.ConvertUnits ? UnitUtils.ConvertFromInternalUnits(aArc.Radius, DisplayUnitType.DUT_METERS) : aArc.Radius;
                Plane plane = Plane.CreateByOriginAndBasis(aArc.Center, aArc.XDirection, aArc.YDirection);
                double startAngle = aArc.XDirection.AngleOnPlaneTo(aArc.GetEndPoint(0) - aArc.Center, aArc.Normal);
                double endAngle = aArc.XDirection.AngleOnPlaneTo(aArc.GetEndPoint(1) - aArc.Center, aArc.Normal);
                if (startAngle > endAngle)
                {
                    startAngle -= 2 * Math.PI;
                }
                return BH.Engine.Geometry.Create.Arc(ToBHoM(plane, pullSettings), radius, startAngle, endAngle);
            }
            
            return null;
        }

        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this List<Curve> curves, PullSettings pullSettings = null)
        {
            if (curves == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return curves.Select(c => c.ToBHoM(pullSettings)).ToList();
        }

        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this LocationCurve locationCurve, PullSettings pullSettings = null)
        {
            if (locationCurve == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return ToBHoM(locationCurve.Curve, pullSettings);
        }


        /***************************************************/

        internal static oM.Geometry.PolyCurve ToBHoM(this CurveLoop curveLoop, PullSettings pullSettings = null)
        {
            if (curveLoop == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Geometry.ICurve> aICurveList = new List<oM.Geometry.ICurve>();
            foreach (Curve aCurve in curveLoop)
                aICurveList.Add(aCurve.ToBHoM(pullSettings));

            return BH.Engine.Geometry.Create.PolyCurve(aICurveList);
        }

        /***************************************************/
    }
}