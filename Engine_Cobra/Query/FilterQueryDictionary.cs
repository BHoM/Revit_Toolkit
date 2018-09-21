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

        public static Dictionary<FilterQuery, List<Element>> FilterQueryDictionary(this FilterQuery filterQuery, UIDocument uIDocument)
        {
            if (uIDocument == null || filterQuery == null)
                return null;

            Dictionary<FilterQuery, List<Element>> aResult = new Dictionary<FilterQuery, List<Element>>();

            QueryType aQueryType = BH.Engine.Adapters.Revit.Query.QueryType(filterQuery);
            if(aQueryType == QueryType.LogicalOr || aQueryType == QueryType.LogicalAnd)
            {
                IEnumerable<FilterQuery> aFilterQueries = BH.Engine.Adapters.Revit.Query.FilterQueries(filterQuery);
                if(aFilterQueries != null && aFilterQueries.Count() > 0)
                {
                    Dictionary<FilterQuery, List<Element>> aFilterQueryDictionary = new Dictionary<FilterQuery, List<Element>>();
                    foreach (FilterQuery aFilterQuery in aFilterQueries)
                        aFilterQueryDictionary = Modify.Append(aFilterQueryDictionary, FilterQueryDictionary(aFilterQuery, uIDocument));

                    if(aQueryType == QueryType.LogicalAnd)
                        aFilterQueryDictionary = Query.LogicalAnd(aFilterQueryDictionary);

                    aResult = aFilterQueryDictionary;
                }
            }
            else
            {
                IEnumerable<Element> aElements = Elements(filterQuery, uIDocument);
                if (aElements != null && aElements.Count() > 0)
                {
                    List<Element> aElementList = null;
                    if (!aResult.TryGetValue(filterQuery, out aElementList))
                    {
                        aElementList = new List<Element>();
                        aResult.Add(filterQuery, aElementList);
                    }

                    aElementList.AddRange(aElements);
                }
            }

            return aResult;
        }

        /***************************************************/
    }
}