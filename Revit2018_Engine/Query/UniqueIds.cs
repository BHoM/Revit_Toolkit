using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BH.oM.Environmental.Elements;
using BH.oM.Base;

using Autodesk.Revit.DB;

namespace BH.Engine.Revit
{
    /// <summary>
    /// BHoM Revit Engine Query Methods
    /// </summary>
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        /// <summary>
        /// Reads Revit UniqueIds from BHoMObjects CustomData. Key: Utilis.AdapterId
        /// </summary>
        /// <param name="bHoMObjects">BHoMObject collection</param>
        /// <param name="removeNulls">Removes nulls from result list</param>
        /// <returns name="UniqueIds">UniqueId List</returns>
        /// <search>
        /// Query, BHoM, GetUniqueIdList, Get UniqueId List, BHoMObject, UniqueIds, UniqueId
        /// </search>
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
