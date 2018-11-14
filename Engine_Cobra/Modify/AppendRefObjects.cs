using System.Collections.Generic;
using System.Linq;

using BH.oM.Base;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AppendRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IBHoMObject bHoMObject)
        {
            if (refObjects == null)
                return null;

            if (bHoMObject == null)
                return refObjects;

            Dictionary<int, List<IBHoMObject>> aRefObjects = null;
            if (refObjects != null)
                aRefObjects = new Dictionary<int, List<IBHoMObject>>(refObjects);
            else
                aRefObjects = new Dictionary<int, List<IBHoMObject>>();

            int aElementId = BH.Engine.Adapters.Revit.Query.ElementId(bHoMObject);

            List<IBHoMObject> aBHoMObjectList = null;
            if (!aRefObjects.TryGetValue(aElementId, out aBHoMObjectList))
            {
                aBHoMObjectList = new List<IBHoMObject>();
                aRefObjects.Add(aElementId, aBHoMObjectList);
            }

            if (aBHoMObjectList != null)
                aBHoMObjectList.Add(bHoMObject);

            return aRefObjects;
        }

        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AppendRefObjects(this Dictionary<int, List<IBHoMObject>> refObjects, IEnumerable<IBHoMObject> bHoMObjects)
        {
            if (refObjects == null)
                return null;

            if (bHoMObjects == null || bHoMObjects.Count() == 0)
                return refObjects;

            Dictionary<int, List<IBHoMObject>> aRefObjects = null;
            if (refObjects != null)
                aRefObjects = new Dictionary<int, List<IBHoMObject>>(refObjects);
            else
                aRefObjects = new Dictionary<int, List<IBHoMObject>>();

            foreach(IBHoMObject aIBHoMObject in bHoMObjects)
            {
                int aElementId = BH.Engine.Adapters.Revit.Query.ElementId(aIBHoMObject);

                List<IBHoMObject> aBHoMObjectList = null;
                if (!aRefObjects.TryGetValue(aElementId, out aBHoMObjectList))
                {
                    aBHoMObjectList = new List<IBHoMObject>();
                    aRefObjects.Add(aElementId, aBHoMObjectList);
                }

                if (aBHoMObjectList != null)
                    aBHoMObjectList.Add(aIBHoMObject);
            }

            return aRefObjects;
        }

        /***************************************************/
    }
}
