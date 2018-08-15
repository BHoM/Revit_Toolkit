using BH.oM.Base;
using System.Collections.Generic;

namespace BH.Engine.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<string> UniqueIds(IEnumerable<BHoMObject> bHoMObjects, bool removeNulls = true)
        {
            if (bHoMObjects == null)
                return null;

            List<string> aUniqueIdList = new List<string>();
            foreach (BHoMObject aBHoMObject in bHoMObjects)
            {
                string aUniqueId = UniqueId(aBHoMObject);
                if (string.IsNullOrEmpty(aUniqueId) && removeNulls)
                    continue;

                aUniqueIdList.Add(aUniqueId);
            }

            return aUniqueIdList;
        }

        /***************************************************/
    }
}
