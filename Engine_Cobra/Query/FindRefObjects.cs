using System.Collections.Generic;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<IBHoMObject> FindRefObjects(this PullSettings pullSettings, int elementId)
        {
            if (pullSettings.RefObjects == null)
                return null;

            List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
            if (pullSettings.RefObjects.TryGetValue(elementId, out aBHoMObjectList))
                return aBHoMObjectList;

            return null;
        }

        /***************************************************/
    }
}
