using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.Engine.Geometry;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<oM.Geometry.PolyCurve> PolyCurves(this Face face, PullSettings pullSettings = null)
        {
            List<oM.Geometry.PolyCurve> aResult = new List<oM.Geometry.PolyCurve>();

            foreach(EdgeArray aEdgeArray in face.EdgeLoops)
            {
                List<oM.Geometry.ICurve> aCurveList = Convert.ToBHoM(aEdgeArray, pullSettings);
                aResult.Add(Create.PolyCurve(aCurveList));
            }

            return aResult;
        }

        /***************************************************/
    }
}