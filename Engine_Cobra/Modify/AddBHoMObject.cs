using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.UI.Cobra.Engine
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AddBHoMObject(this Dictionary<int, List<IBHoMObject>> objects, IBHoMObject bHoMObject)
        {
            if (bHoMObject == null && bHoMObject == null)
                return null;

            Dictionary<int, List<IBHoMObject>> aResult = null;
            if (objects == null)
                aResult = new Dictionary<int, List<IBHoMObject>>();
            else
                aResult = new Dictionary<int, List<IBHoMObject>>(objects);

            if (bHoMObject == null)
                return new Dictionary<int, List<IBHoMObject>>(objects);

            ElementId aElementId = Query.ElementId(bHoMObject);
            if (aElementId == null)
                aElementId = ElementId.InvalidElementId;

            List<IBHoMObject> aBHoMObjectList = null;
            if (aResult.TryGetValue(aElementId.IntegerValue, out aBHoMObjectList))
            {
                if (aBHoMObjectList == null)
                    aBHoMObjectList = new List<IBHoMObject>();

                if (aBHoMObjectList.Find(x => x != null && x.BHoM_Guid == bHoMObject.BHoM_Guid) == null)
                    aBHoMObjectList.Add(bHoMObject);
            }
            else
            {
                aResult.Add(aElementId.IntegerValue, new List<IBHoMObject>() { bHoMObject });
            }

            return aResult;
        }

        /***************************************************/
    }
}