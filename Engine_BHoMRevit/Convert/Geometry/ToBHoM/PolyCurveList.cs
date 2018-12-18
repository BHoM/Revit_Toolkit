using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Geometry;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static List<PolyCurve> ToBHoM(this CurveArrArray curveArrArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PolyCurve> aResult = new List<PolyCurve>();
            foreach (CurveArray aCurveArray in curveArrArray)
                aResult.Add(BH.Engine.Geometry.Create.PolyCurve(ToBHoM(aCurveArray, pullSettings)));

            return aResult;
        }

        /***************************************************/

        internal static List<PolyCurve> ToBHoM(this EdgeArrayArray edgeArray, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

            List<PolyCurve> aResult = new List<PolyCurve>();
            foreach (EdgeArray ea in edgeArray)
            {
                aResult.Add(BH.Engine.Geometry.Create.PolyCurve(ea.ToBHoM(pullSettings)));
            }
            return aResult;
        }

        /***************************************************/

        internal static List<PolyCurve> ToBHoMPolyCurveList(this PlanarFace planarFace, PullSettings pullSettings = null)
        {
            List<PolyCurve> aResult = new List<PolyCurve>();

            EdgeArrayArray aEdgeArrayArray = planarFace.EdgeLoops;
            if (aEdgeArrayArray == null && aEdgeArrayArray.Size == 0)
                return aResult;

            for (int i = 0; i < aEdgeArrayArray.Size; i++)
            {
                EdgeArray aEdgeArray = aEdgeArrayArray.get_Item(i);
                List<ICurve> aCurveList = new List<ICurve>();
                foreach (Edge aEdge in aEdgeArray)
                {
                    Curve aCurve = aEdge.AsCurve();
                    if (aCurve != null)
                        aCurveList.Add(aCurve.ToBHoM(pullSettings));
                }

                if (aCurveList != null && aCurveList.Count > 0)
                    aResult.Add(BH.Engine.Geometry.Create.PolyCurve(aCurveList));
            }

            return aResult;
        }
    }
}