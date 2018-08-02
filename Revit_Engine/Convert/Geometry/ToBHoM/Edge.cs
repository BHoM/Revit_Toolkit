using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Convert
    {
        /***************************************************/
        /****              Public methods               ****/
        /***************************************************/

        public static oM.Geometry.ICurve ToBHoM(this Autodesk.Revit.DB.Edge edge, bool convertUnits = true)
        {
            return ToBHoM(edge.AsCurve(), convertUnits);
        }

        /***************************************************/

        public static List<oM.Geometry.ICurve> ToBHoM(this EdgeArray edgeArray, bool convertUnits = true)
        {
            List<oM.Geometry.ICurve> result = new List<oM.Geometry.ICurve>();
            foreach (Autodesk.Revit.DB.Edge aEdge in edgeArray)
            {
                result.Add(aEdge.ToBHoM(convertUnits));
            }

            return result;
        }

        /***************************************************/

        public static List<List<oM.Geometry.ICurve>> ToBHoM(this EdgeArrayArray edgeArray, bool convertUnits = true)
        {
            List<List<oM.Geometry.ICurve>> result = new List<List<oM.Geometry.ICurve>>();
            foreach (EdgeArray ea in edgeArray)
            {
                result.Add(ea.ToBHoM(convertUnits));
            }
            return result;
        }

        /***************************************************/
    }
}