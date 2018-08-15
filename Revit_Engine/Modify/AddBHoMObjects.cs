using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Modify
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<int, List<IBHoMObject>> AddBHoMObjects(this Dictionary<int, List<IBHoMObject>> objects, IEnumerable<IBHoMObject> bHoMObjects)
        {
            if (bHoMObjects == null && bHoMObjects == null)
                return null;

            Dictionary<int, List<IBHoMObject>> aResult = null;
            if (objects == null)
                aResult = new Dictionary<int, List<IBHoMObject>>();
            else
                aResult = new Dictionary<int, List<IBHoMObject>>(objects);

            foreach (BHoMObject aBHoMObject in bHoMObjects)
                aResult = aResult.AddBHoMObject(aBHoMObject);

            return aResult;
        }

        /***************************************************/
    }
}