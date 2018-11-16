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

        public static List<oM.Geometry.PolyCurve> PolyCurves(this Face face, Transform transform = null, PullSettings pullSettings = null)
        {
            List<oM.Geometry.PolyCurve> aResult = new List<oM.Geometry.PolyCurve>();

            foreach(CurveLoop aCurveLoop in face.GetEdgesAsCurveLoops())
            {
                List<oM.Geometry.ICurve> aCurveList = new List<oM.Geometry.ICurve>();
                foreach (Curve aCurve in aCurveLoop)
                {
                    if (transform != null)
                        aCurveList.Add(Convert.ToBHoM(aCurve.CreateTransformed(transform), pullSettings));
                    else
                        aCurveList.Add(Convert.ToBHoM(aCurve, pullSettings));
                }
                aResult.Add(Create.PolyCurve(aCurveList));
            }

            return aResult;
        }

        /***************************************************/
    }
}