using System;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery LogicalOrFilterQuery(IEnumerable<FilterQuery> filterQueries)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.LogicalOr;
            aFilterQuery.Equalities[Convert.FilterQuery.FilterQueries] = filterQueries;
            return aFilterQuery;
        }

        public static FilterQuery LogicalOrFilterQuery(FilterQuery filterQuery_1, FilterQuery filterQuery_2)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.LogicalOr;
            aFilterQuery.Equalities[Convert.FilterQuery.FilterQueries] = new List<FilterQuery>() { filterQuery_1, filterQuery_2 };
            return aFilterQuery;
        }
    }
}
