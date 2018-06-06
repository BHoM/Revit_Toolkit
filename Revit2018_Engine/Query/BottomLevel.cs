using System.Collections.Generic;

using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Linq;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Gets Bottom Level for curve
        /// </summary>
        /// <param name="curve">BHoM curve</param>
        /// <param name="document">Revit Document</param>
        /// <returns name="Level">Revit Level</returns>
        /// <search>
        /// Query, Level, Revit, Level, curve
        /// </search>
        static public Level BottomLevel(this oM.Geometry.ICurve curve, Document document)
        {
            double aMinElevation = BH.Engine.Geometry.Query.Bounds(curve as dynamic).Min.Z;

            List<Level> aLevelList = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            aLevelList.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            if (aMinElevation < aLevelList.First().Elevation)
                return aLevelList.First();

            if (aMinElevation > aLevelList.Last().Elevation)
                return aLevelList.Last();

            for (int i = 1; i > aLevelList.Count; i++)
                if (aLevelList[i].Elevation > aMinElevation)
                    return aLevelList[i -1];

            return null;
        }

        /***************************************************/
    }
}
