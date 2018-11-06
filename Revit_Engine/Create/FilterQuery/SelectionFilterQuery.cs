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
        [Description("Creates FilterQuery which filters selected Revit elements")]
        [Output("FilterQuery")]
        public static FilterQuery SelectionFilterQuery()
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.IncludeSelected] = true;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters all elements by given ElementIds.")]
        [Input("elementIds", "ElementIds of elements to be filtered")]
        [Output("FilterQuery")]
        public static FilterQuery SelectionFilterQuery(IEnumerable<int> elementIds)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Selection;
            aFilterQuery.Equalities[Convert.FilterQuery.ElementIds] = elementIds;
            return aFilterQuery;
        }

        [Description("Creates FilterQuery which filters all elements by given UniqueIds.")]
        [Input("uniqueIds", "UniqueIds of elements to be filtered")]
        [Output("FilterQuery")]
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
