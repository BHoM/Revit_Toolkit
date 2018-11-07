using System.Collections.Generic;
using System.Linq;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IBHoMObject FindRefObject(this PullSettings pullSettings, int elementId)
        {
            if (pullSettings.RefObjects == null)
                return null;

            List<IBHoMObject> aBHoMObjectList = new List<IBHoMObject>();
            if (pullSettings.RefObjects.TryGetValue(elementId, out aBHoMObjectList))
                if (aBHoMObjectList != null && aBHoMObjectList.Count > 0)
                    return aBHoMObjectList.First();

            return null;
        }

        /***************************************************/
    }
}
