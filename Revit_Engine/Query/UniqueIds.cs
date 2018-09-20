using System.Collections.Generic;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/
        
        public static List<string> UniqueIds(IEnumerable<IBHoMObject> bHoMObjects, bool removeNulls = true)
        {
            if (bHoMObjects == null)
                return null;

            List<string> aUniqueIdList = new List<string>();
            foreach (IBHoMObject aBHoMObject in bHoMObjects)
            {
                string aUniqueId = UniqueId(aBHoMObject);
                if (string.IsNullOrEmpty(aUniqueId) && removeNulls)
                    continue;

                aUniqueIdList.Add(aUniqueId);
            }

            return aUniqueIdList;
        }

        /***************************************************/

        public static IEnumerable<string> UniqueIds(this FilterQuery filterQuery)
        {
            if (filterQuery == null)
                return null;

            if (!filterQuery.Equalities.ContainsKey(Convert.FilterQuery.UniqueIds))
                return null;

            return filterQuery.Equalities[Convert.FilterQuery.UniqueIds] as IEnumerable<string>;
        }

        /***************************************************/
    }
}
