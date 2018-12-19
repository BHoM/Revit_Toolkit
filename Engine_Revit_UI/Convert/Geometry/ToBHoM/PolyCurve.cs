using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
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