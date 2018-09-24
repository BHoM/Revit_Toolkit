using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using BH.oM.Base;
using BH.oM.DataManipulation.Queries;


namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<FilterQuery> FilterQueries(this Dictionary<FilterQuery, List<Element>> filterQueryDictionary, ElementId ElementId)
        {
            if (filterQueryDictionary == null)
                return null;

            List<FilterQuery> aFilterQueryList = new List<FilterQuery>();
            foreach (KeyValuePair<FilterQuery, List<Element>> aKeyValuePair in filterQueryDictionary)
                if (aKeyValuePair.Value.Find(x => x.Id == ElementId) != null)
                    aFilterQueryList.Add(aKeyValuePair.Key);

            return aFilterQueryList;
        }

        /***************************************************/
    }
}
