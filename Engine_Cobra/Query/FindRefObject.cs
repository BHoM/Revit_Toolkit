using System.Collections.Generic;
using System.Linq;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using Autodesk.Revit.DB;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static T FindRefObject<T>(this PullSettings pullSettings, int elementId) where T : IBHoMObject
        {
            if (pullSettings.RefObjects == null)
                return default(T);

            List<T> aTList = FindRefObjects<T>(pullSettings, elementId);
            if (aTList != null && aTList.Count > 0)
                return aTList.First();

            return default(T);
        }

        /***************************************************/

        public static T FindRefObject<T>(this PushSettings pushSettings, Document Document, Guid guid) where T : Element
        {
            if (pushSettings.RefObjects == null)
                return null;

            List<T> aTList = FindRefObjects<T>(pushSettings, Document, guid);
            if (aTList != null && aTList.Count > 0)
                return aTList.First();

            return default(T);
        }

        /***************************************************/
    }
}
