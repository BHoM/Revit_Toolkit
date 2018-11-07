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
    }
}
