using Autodesk.Revit.DB;
using BH.oM.Geometry;
using BH.oM.Adapters.Revit.Settings;
using System.Linq;

namespace BH.UI.Cobra.Engine
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
                aCurveArray.Append(aICurve.ToRevitCurve(pushSettings));

            return aCurveArray;
        }

        /***************************************************/
    }
}