using BH.oM.DataManipulation.Queries;
using BH.oM.Adapters.Revit.Enums;
using BH.oM.Base;

namespace BH.Engine.Adapters.Revit
{
    public static partial class Create
    {
        public static FilterQuery SelectionSetFilterQuery(string slectionSetName)
        {
            FilterQuery aFilterQuery = new FilterQuery();
            aFilterQuery.Type = typeof(BHoMObject);
            aFilterQuery.Equalities[Convert.FilterQuery.QueryType] = QueryType.SelectionSet;
            aFilterQuery.Equalities[Convert.FilterQuery.SelectionSetName] = slectionSetName;
            return aFilterQuery;
        }
    }
}
