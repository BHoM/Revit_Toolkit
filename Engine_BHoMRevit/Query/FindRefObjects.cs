using System.Collections.Generic;

using BH.oM.Adapters.Revit.Settings;
using BH.oM.Base;
using System;
using System.Linq;
using Autodesk.Revit.DB;

namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static List<T> FindRefObjects<T>(this PullSettings pullSettings, int elementId) where T : IBHoMObject
        {
            if (pullSettings.RefObjects == null)
                return null;

            List<IBHoMObject> aBHoMObjectList = null;
            if (pullSettings.RefObjects.TryGetValue(elementId, out aBHoMObjectList))
                if (aBHoMObjectList != null)
                    return aBHoMObjectList.FindAll(x => x is T).Cast<T>().ToList();

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

        public static List<T> FindRefObjects<T>(this PushSettings pushSettings, Document Document, Guid guid) where T : Element
        {
            if (pushSettings.RefObjects == null)
                return null;

            List<int> aBHoMObjectList = null;
            if (!pushSettings.RefObjects.TryGetValue(guid, out aBHoMObjectList))
                return null;

            if (aBHoMObjectList == null)
                return null;

            List<T> aResult = new List<T>();
            if (aBHoMObjectList.Count == 0)
                return aResult;

            return aBHoMObjectList.ConvertAll(x => Document.GetElement(new ElementId(x))).FindAll(x => x is T).Cast<T>().ToList();
        }
    }
}
