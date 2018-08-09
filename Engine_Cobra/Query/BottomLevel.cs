using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
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