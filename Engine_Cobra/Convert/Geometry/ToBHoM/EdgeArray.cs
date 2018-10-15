using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
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

        /***************************************************/
    }
}