using Autodesk.Revit.DB;
using System.Collections.Generic;
using BH.oM.Adapters.Revit;

namespace BH.UI.Cobra.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.ICurve ToBHoM(this Edge edge, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            return ToBHoM(edge.AsCurve(), pullSettings);
        }

        /***************************************************/

        internal static List<oM.Geometry.ICurve> ToBHoM(this EdgeArray edgeArray, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Edge aEdge in edgeArray)
            {
                result.Add(aEdge.ToBHoM(pullSettings));
            }

            return result;
        }

        /***************************************************/

        internal static List<List<oM.Geometry.ICurve>> ToBHoM(this EdgeArrayArray edgeArray, PullSettings pullSettings = null)
        {
            pullSettings.DefaultIfNull();

            List<List<oM.Geometry.ICurve>> result = new List<List<oM.Geometry.ICurve>>();
            foreach (EdgeArray ea in edgeArray)
            {
                result.Add(ea.ToBHoM(pullSettings));
            }
            return result;
        }

        /***************************************************/
    }
}