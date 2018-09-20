using System;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery CategoryFilterQuery(string CategoryName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities["QueryType"] = QueryType.Category;
            aFilterQuery.Equalities["CategoryName"] = CategoryName;
            return aFilterQuery;
        }
    }
}
