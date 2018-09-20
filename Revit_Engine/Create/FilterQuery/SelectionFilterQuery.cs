using System;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery SelectionFilterQuery(Type type)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = type;
            aFilterQuery.Equalities["QueryType"] = QueryType.Selection;
            return aFilterQuery;
        }

        public static FilterQuery SelectionFilterQuery(IEquatable<int> ElementIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities["QueryType"] = QueryType.Selection;
            aFilterQuery.Equalities["ElementIds"] = ElementIds;
            return aFilterQuery;
        }

        public static FilterQuery SelectionFilterQuery(IEquatable<string> UniqueIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities["QueryType"] = QueryType.Selection;
            aFilterQuery.Equalities["UniqueIds"] = UniqueIds;
            return aFilterQuery;
        }
    }
}
