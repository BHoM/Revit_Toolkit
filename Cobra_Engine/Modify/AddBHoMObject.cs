using Autodesk.Revit.DB;
using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Modify Methods
    /// </summary>
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<IBHoMObject>> AddBHoMObject(this Dictionary<ElementId, List<IBHoMObject>> objects, IBHoMObject bHoMObject)
        {
            if (bHoMObject == null && bHoMObject == null)
                return null;

            Dictionary<ElementId, List<IBHoMObject>> aResult = null;
            if (objects == null)
                aResult = new Dictionary<ElementId, List<IBHoMObject>>();
            else
                aResult = new Dictionary<ElementId, List<IBHoMObject>>(objects);

            if (bHoMObject == null)
                return new Dictionary<ElementId, List<IBHoMObject>>(objects);

            ElementId aElementId = Query.ElementId(bHoMObject);
            if (aElementId == null)
                aElementId = ElementId.InvalidElementId;

            List<IBHoMObject> aBHoMObjectList = null;
            if (aResult.TryGetValue(aElementId, out aBHoMObjectList))
            {
                if (aBHoMObjectList == null)
                    aBHoMObjectList = new List<IBHoMObject>();

                if (aBHoMObjectList.Find(x => x != null && x.BHoM_Guid == bHoMObject.BHoM_Guid) == null)
                    aBHoMObjectList.Add(bHoMObject);
            }
            else
            {
                aResult.Add(aElementId, new List<IBHoMObject>() { bHoMObject });
            }

            return aResult;
        }

        /***************************************************/
    }
}

