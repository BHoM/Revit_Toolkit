using System.Collections.Generic;

using Autodesk.Revit.DB;

using BH.oM.DataManipulation.Queries;


namespace BH.UI.Revit.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterQuery>> LogicalOr(this Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary_1, Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary_2)
        {
            if (filterQueryDictionary_1 == null || filterQueryDictionary_2 == null)
                return null;

            if (filterQueryDictionary_1.Count == 0)
                return new Dictionary<ElementId, List<FilterQuery>>(filterQueryDictionary_2);

            if (filterQueryDictionary_2.Count == 0)
                return new Dictionary<ElementId, List<FilterQuery>>(filterQueryDictionary_1);


            Dictionary<ElementId, List<FilterQuery>> aResult = new Dictionary<ElementId, List<FilterQuery>>();
            foreach(KeyValuePair<ElementId, List<FilterQuery>> aKeyValuePair in filterQueryDictionary_1)
            {
                List<FilterQuery> aFilterQueryList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aFilterQueryList))
                {
                    aFilterQueryList = new List<FilterQuery>();
                    aResult.Add(aKeyValuePair.Key, aFilterQueryList);
                }

                foreach(FilterQuery aFilterQuery in aKeyValuePair.Value)
                    if (!aFilterQueryList.Contains(aFilterQuery))
                        aFilterQueryList.Add(aFilterQuery);
            }

            foreach (KeyValuePair<ElementId, List<FilterQuery>> aKeyValuePair in filterQueryDictionary_2)
            {
                List<FilterQuery> aFilterQueryList = null;
                if (!aResult.TryGetValue(aKeyValuePair.Key, out aFilterQueryList))
                {
                    aFilterQueryList = new List<FilterQuery>();
                    aResult.Add(aKeyValuePair.Key, aFilterQueryList);
                }

                foreach (FilterQuery aFilterQuery in aKeyValuePair.Value)
                    if (!aFilterQueryList.Contains(aFilterQuery))
                        aFilterQueryList.Add(aFilterQuery);
            }

            return aResult;
        }

        /***************************************************/

    }
}