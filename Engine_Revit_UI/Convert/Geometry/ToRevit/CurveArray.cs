using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using System.Collections.Generic;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        internal static CurveArray ToRevitCurveArray(this PolyCurve polyCurve, PushSettings pushSettings = null)
        {
            if (polyCurve == null)
                return null;

            pushSettings = pushSettings.DefaultIfNull();

            CurveArray aCurveArray = new CurveArray();
            foreach (ICurve aICurve in polyCurve.Curves)
            {
                List<Curve> aCurveList = aICurve.ToRevitCurveList(pushSettings);
                if (aCurveList == null || aCurveList.Count == 0)
                    continue;

                aCurveList.ForEach(x => aCurveArray.Append(x));
            } 

            return aCurveArray;
        }

        /***************************************************/
    }
}