using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.Adapters.Revit.Settings;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        static public Level NextLevel(this Level Level)
        {
            if (Level == null)
                return null;

            List<Level> aLevelList = new FilteredElementCollector(Level.Document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (aLevelList == null || aLevelList.Count < 2)
                return null;

            aLevelList.Sort((x, y) => x.ProjectElevation.CompareTo(y.ProjectElevation));

            int aIndex = aLevelList.FindIndex(x => x.Id == Level.Id);
            if (aIndex == -1)
                return null;

            if (aIndex == aLevelList.Count - 1)
                return null;

            return aLevelList[aIndex + 1];
        }

        /***************************************************/
    }
}