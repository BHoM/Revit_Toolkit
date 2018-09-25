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

        public static IEnumerable<FilterQuery> FilterQueries(this Dictionary<ElementId, List<FilterQuery>> filterQueryDictionary, ElementId ElementId)
        {
            if (filterQueryDictionary == null)
                return null;

            List<FilterQuery> aFilterQueryList = null;
            if (!filterQueryDictionary.TryGetValue(ElementId, out aFilterQueryList))
                return null;

            return aFilterQueryList;
        }

        /***************************************************/
    }
}
