using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****             Internal methods              ****/
        /***************************************************/

        internal static oM.Geometry.Polyline ToBHoM(this Polyloop polyloop, bool convertUnits = true)
        {
            IList<XYZ> aXYZs = polyloop.GetPoints();
            if (aXYZs == null)
                return null;

            List<oM.Geometry.Point> aPointList = new List<oM.Geometry.Point>();
            if (aXYZs.Count > 0)
            {
                foreach (XYZ aXYZ in aXYZs)
                    aPointList.Add(aXYZ.ToBHoM(convertUnits));

                aPointList.Add(aXYZs[0].ToBHoM(convertUnits));
            }

            return Geometry.Create.Polyline(aPointList);
        }

        /***************************************************/
    }
}