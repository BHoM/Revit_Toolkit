using System;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery FamilyFilterQuery(string familyName = null, string familySymbolName = null)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Family;
            aFilterQuery.Equalities[Convert.FilterQuery.FamilyName] = familyName;
            aFilterQuery.Equalities[Convert.FilterQuery.FamilySymbolName] = familySymbolName;
            return aFilterQuery;
        }
    }
}
