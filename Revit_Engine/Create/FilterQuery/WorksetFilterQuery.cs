using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters elements by given Workset Name.")]
        [Input("worksetName", "Revit Workset Name")]
        [Output("FilterQuery")]
        public static FilterQuery WorksetFilterQuery(string worksetName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Workset;
            aFilterQuery.Equalities[Convert.FilterQuery.WorksetName] = worksetName;
            return aFilterQuery;
        }
    }
}
