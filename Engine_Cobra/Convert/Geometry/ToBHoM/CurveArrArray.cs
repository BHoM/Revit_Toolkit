using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<List<oM.Geometry.ICurve>> ToBHoM(this CurveArrArray curveArrArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<List<oM.Geometry.ICurve>> aResult = new List<List<oM.Geometry.ICurve>>();
            foreach (CurveArray aCurveArray in curveArrArray)
                aResult.Add(ToBHoM(aCurveArray, pullSettings));

            return aResult;
        }

        /***************************************************/
    }
}