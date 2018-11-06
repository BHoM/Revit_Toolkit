using System.ComponentModel;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which combines other FilterQueries by logical or operator.")]
        [Input("filterQueries", "Filter Queries to be combined")]
        [Output("FilterQuery")]
        public static FilterQuery LogicalOrFilterQuery(IEnumerable<FilterQuery> filterQueries)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.LogicalOr;
            aFilterQuery.Equalities[Convert.FilterQuery.FilterQueries] = filterQueries;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which combines two FilterQueries by logical or operator.")]
        [Input("filterQuery_1", "First FilterQuery to be combined")]
        [Input("filterQuery_2", "Second FilterQuery to be combined")]
        [Output("FilterQuery")]
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
