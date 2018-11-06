using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all elements by given Revit Type/Class name.")]
        [Input("typeName", "Revit Type/Class name")]
        [Output("FilterQuery")]
        public static FilterQuery TypeNameFilterQuery(string typeName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.TypeName;
            aFilterQuery.Equalities[Convert.FilterQuery.TypeName] = typeName;
            return aFilterQuery;
        }
    }
}
