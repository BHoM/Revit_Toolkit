using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;

namespace BH.UI.Cobra.Engine
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static Dictionary<ElementId, List<FilterQuery>> FilterQueryDictionary(this FilterQuery filterQuery, UIDocument uIDocument)
        {
            if (uIDocument == null || filterQuery == null)
                return null;

            Dictionary<ElementId, List<FilterQuery>> aResult = new Dictionary<ElementId, List<FilterQuery>>();

            IEnumerable<FilterQuery> aFilterQueries = BH.Engine.Adapters.Revit.Query.FilterQueries(filterQuery);
            if (aFilterQueries != null && aFilterQueries.Count() > 0)
            {
                QueryType aQueryType = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery);

                Dictionary<ElementId, List<FilterQuery>> aFilterQueryDictionary = null;
                foreach (FilterQuery aFilterQuery in aFilterQueries)
                {
                    Dictionary<ElementId, List<FilterQuery>> aFilterQueryDictionary_Temp = FilterQueryDictionary(aFilterQuery, uIDocument);
                    if (aFilterQueryDictionary == null)
                    {
                        aFilterQueryDictionary = aFilterQueryDictionary_Temp;
                    }
                    else
                    {
                        if (aQueryType == QueryType.LogicalAnd)
                            aFilterQueryDictionary = Query.LogicalAnd(aFilterQueryDictionary, aFilterQueryDictionary_Temp);
                        else
                            aFilterQueryDictionary = Query.LogicalOr(aFilterQueryDictionary, aFilterQueryDictionary_Temp);
                    }
                }
                aResult = aFilterQueryDictionary;
            }
            else
            {
                IEnumerable<ElementId> aElementIds = ElementIds(filterQuery, uIDocument);
                if (aElementIds != null)
                {
                    foreach(ElementId aElementId in aElementIds)
                    {
                        List<FilterQuery> aFilterQueryList = null;
                        if (!aResult.TryGetValue(aElementId, out aFilterQueryList))
                        {
                            aFilterQueryList = new List<FilterQuery>();
                            aResult.Add(aElementId, aFilterQueryList);
                        }
                        aFilterQueryList.Add(filterQuery);
                    }
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}