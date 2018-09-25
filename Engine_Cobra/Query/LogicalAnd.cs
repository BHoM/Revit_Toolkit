using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.DataManipulation.Queries;


namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterQuery>> LogicalAnd(this Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary_1, Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary_2)
        {
            if (filterQueryDictionary_1 == null || filterQueryDictionary_2 == null)
                return null;

            Dictionary<ElementId, List<FilterQuery>> aResult = new Dictionary<ElementId, List<FilterQuery>>();

            if (filterQueryDictionary_1.Count() == 0 || filterQueryDictionary_2.Count() == 0)
                return aResult;

            foreach(KeyValuePair<ElementId, List<FilterQuery>> aKeyValuePair in filterQueryDictionary_1)
            {
                if (aKeyValuePair.Value == null || aKeyValuePair.Value.Count == 0)
                    continue;

                List<FilterQuery> aFilterQueryList = null;
                if (filterQueryDictionary_2.TryGetValue(aKeyValuePair.Key, out aFilterQueryList))
                {
                    if (aFilterQueryList == null || aFilterQueryList.Count == 0)
                        continue;

                    List<FilterQuery> aFilterQueryList_Temp = new List<FilterQuery>(aKeyValuePair.Value);
                    aFilterQueryList_Temp.AddRange(aFilterQueryList);
                    aResult.Add(aKeyValuePair.Key, aFilterQueryList_Temp);
                }
            }
            return aResult;
        }

        /***************************************************/

    }
}