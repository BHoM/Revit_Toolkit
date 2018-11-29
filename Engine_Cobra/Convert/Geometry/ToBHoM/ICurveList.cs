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

        internal static List<oM.Geometry.ICurve> ToBHoM(this List<Curve> curves, PullSettings pullSettings = null)
        {
            if (curves == null)
                return null;

            pullSettings = pullSettings.DefaultIfNull();

            return curves.Select(c => c.ToBHoM(pullSettings)).ToList();
        }

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

        internal static List<oM.Geometry.ICurve> ToBHoM(this EdgeArray edgeArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Edge aEdge in edgeArray)
            {
                result.Add(aEdge.ToBHoM(pullSettings));
            }

            return result;
        }
    }
}