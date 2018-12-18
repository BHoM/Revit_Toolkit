using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;

using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Polyline ToBHoM(this Polyloop polyloop, PullSettings pullSettings = null)
        {
            pullSettings = pullSettings.DefaultIfNull();

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

            return BH.Engine.Geometry.Create.Polyline(aPointList);
        }

        /***************************************************/
    }
}