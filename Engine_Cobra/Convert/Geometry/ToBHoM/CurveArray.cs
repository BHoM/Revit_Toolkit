using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this CurveArray curveArray, PullSettings pullSettings = null)
        {
            if (curveArray == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            for (int i = 0; i < curveArray.Size; i++)
            {
                result.Add(curveArray.get_Item(i).ToBHoM(pullSettings));
            }
            return result;
        }

        /***************************************************/
    }
}