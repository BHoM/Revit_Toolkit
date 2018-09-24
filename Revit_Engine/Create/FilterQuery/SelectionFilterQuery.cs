using System;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery SelectionFilterQuery()
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.IncludeSelected] = true;
            return aFilterQuery;
        }

        public static FilterQuery SelectionFilterQuery(IEnumerable<int> elementIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.ElementIds] = elementIds;
            return aFilterQuery;
        }

        public static FilterQuery SelectionFilterQuery(IEnumerable<string> uniqueIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.UniqueIds] = uniqueIds;
            return aFilterQuery;
        }
    }
}
