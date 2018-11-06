using System.ComponentModel;

using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        [Description("Creates FilterQuery which filters all elements by given Family Name or/and Family sType Name.")]
        [Input("familyName", "Family Name. Keep default value (null) to seek through all Family Type Names.")]
        [Input("familyTypeName", "Family Type Name")]
        [Output("FilterQuery")]
        public static FilterQuery FamilyFilterQuery(string familyName = null, string familyTypeName = null)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.Family;
            aFilterQuery.Equalities[Convert.FilterQuery.FamilyName] = familyName;
            aFilterQuery.Equalities[Convert.FilterQuery.FamilyTypeName] = familyTypeName;
            return aFilterQuery;
        }
    }
}
