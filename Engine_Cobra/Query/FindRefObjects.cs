using System.Collections.Generic;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;

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

            List<IBHoMObject> aBHoMObjectList = null;
            if (pullSettings.RefObjects.TryGetValue(elementId, out aBHoMObjectList))
                return aBHoMObjectList;

            return null;
        }

        /***************************************************/

        public static List<int> FindRefObjects(this PushSettings pushSettings, Guid guid)
        {
            if (pushSettings.RefObjects == null)
                return null;

            List<int> aBHoMObjectList = null;
            if (pushSettings.RefObjects.TryGetValue(guid, out aBHoMObjectList))
                return aBHoMObjectList;

            return null;
        }

        /***************************************************/
    }
}
