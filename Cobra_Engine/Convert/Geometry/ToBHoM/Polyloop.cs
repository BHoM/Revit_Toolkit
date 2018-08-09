using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using BH.oM.Adapters.Revit;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Polyline ToBHoM(this Polyloop polyloop, PullSettings pullSettings = null)
        {
            if (pullSettings == null)
                pullSettings = PullSettings.Default;

            IList<XYZ> aXYZs = polyloop.GetPoints();
            if (aXYZs == null)
                return null;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            if (aXYZs.Count > 0)
            {
                foreach (XYZ aXYZ in aXYZs)
                    aPointList.Add(aXYZ.ToBHoM(pullSettings));

                aPointList.Add(aXYZs[0].ToBHoM(pullSettings));
            }

            return Geometry.Create.Polyline(aPointList);
        }

        /***************************************************/
    }
}