using System.Collections.Generic;
using System.ComponentModel;

using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Adds reference object to existsing reference Dictionary. Method will create new dictionary if refObjects is null")]
        [Input("refObjects", "Existing reference objects")]
        [Input("bHoMObject", "BHoM object to be added")]
        [Output("RefObjects")]
        public static Dictionary<int, List<IBHoMObject>> AddRefObject(this Dictionary<int, List<IBHoMObject>> refObjects, IBHoMObject bHoMObject)
        {
            if (bHoMObject == null && bHoMObject == null)
                return null;

            Dictionary<int, List<IBHoMObject>> aResult = null;
            if (refObjects == null)
                aResult = new Dictionary<int, List<IBHoMObject>>();
            else
                aResult = new Dictionary<int, List<IBHoMObject>>(refObjects);

            if (bHoMObject == null)
                return new Dictionary<int, List<IBHoMObject>>(refObjects);

            int aId = Query.ElementId(bHoMObject);

            List<IBHoMObject> aBHoMObjectList = null;
            if (aResult.TryGetValue(aId, out aBHoMObjectList))
            {
                if (aBHoMObjectList == null)
                    aBHoMObjectList = new List<IBHoMObject>();

                if (aBHoMObjectList.Find(x => x != null && x.BHoM_Guid == bHoMObject.BHoM_Guid) == null)
                    aBHoMObjectList.Add(bHoMObject);
            }
            else
            {
                aResult.Add(aId, new List<IBHoMObject>() { bHoMObject });
            }

            return aResult;
        }

        /***************************************************/
    }
}