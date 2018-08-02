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

        public static Dictionary<ElementId, List<BHoMObject>> AddBHoMObject(this Dictionary<ElementId, List<BHoMObject>> objects, BHoMObject bHoMObject)
        {
            if (bHoMObject == null && bHoMObject == null)
                return null;

            Dictionary<ElementId, List<BHoMObject>> aResult = null;
            if (objects == null)
                aResult = new Dictionary<ElementId, List<BHoMObject>>();
            else
                aResult = new Dictionary<ElementId, List<BHoMObject>>(objects);

            if (bHoMObject == null)
                return new Dictionary<ElementId, List<BHoMObject>>(objects);

            ElementId aElementId = Query.ElementId(bHoMObject);
            if (aElementId == null)
                aElementId = ElementId.InvalidElementId;

            List<BHoMObject> aBHoMObjectList = null;
            if (aResult.TryGetValue(aElementId, out aBHoMObjectList))
            {
                if (aBHoMObjectList == null)
                    aBHoMObjectList = new List<BHoMObject>();

                if (aBHoMObjectList.Find(x => x != null && x.BHoM_Guid == bHoMObject.BHoM_Guid) == null)
                    aBHoMObjectList.Add(bHoMObject);
            }
            else
            {
                aResult.Add(aElementId, new List<BHoMObject>() { bHoMObject });
            }

            return aResult;
        }

        /***************************************************/
    }
}

