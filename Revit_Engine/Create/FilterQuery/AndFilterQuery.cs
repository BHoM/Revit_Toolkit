using System;
using System.Collections.Generic;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery AndFilterQuery(IEnumerable<FilterQuery> filterQueries)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.And;
            aFilterQuery.Equalities[Convert.FilterQuery.FilterQueries] = filterQueries;
            return aFilterQuery;
        }
    }
}
