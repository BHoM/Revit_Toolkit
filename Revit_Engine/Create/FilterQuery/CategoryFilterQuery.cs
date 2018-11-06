using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all by given Revit Category Name.")]
        [Input("categoryName", "Revit Category Name")]
        [Output("FilterQuery")]
        public static FilterQuery CategoryFilterQuery(string categoryName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Category;
            aFilterQuery.Equalities[Convert.FilterQuery.CategoryName] = categoryName;
            return aFilterQuery;
        }
    }
}
